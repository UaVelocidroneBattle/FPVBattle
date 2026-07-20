using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.PilotBinding.Services;

namespace Veloci.Web.Controllers.Profile;

[ApiController]
[Route("/api/profile")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProfileController : ControllerBase
{
    private static readonly Serilog.ILogger Log = Serilog.Log.ForContext<ProfileController>();

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PilotBindingService _bindingService;
    private readonly IRepository<Pilot> _pilots;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        PilotBindingService bindingService,
        IRepository<Pilot> pilots)
    {
        _userManager = userManager;
        _bindingService = bindingService;
        _pilots = pilots;
    }

    [HttpGet]
    public async Task<ActionResult<ProfileModel>> Get()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        return await BuildProfileAsync(user);
    }

    /// <summary>
    /// Finds a pilot by exact name so the user can confirm it before claiming.
    /// </summary>
    [HttpGet("pilot-lookup")]
    public async Task<ActionResult<PilotLookupModel>> PilotLookup([FromQuery] string pilotName)
    {
        if (string.IsNullOrWhiteSpace(pilotName))
            return BadRequest("Pilot name is required");

        var pilot = await _bindingService.FindPilotByNameAsync(pilotName);

        Log.Information("User {UserId} looked up pilot {PilotName}: {LookupResult}",
            _userManager.GetUserId(User), pilotName, pilot is null ? "not found" : "found");

        if (pilot is null)
            return new PilotLookupModel { Found = false };

        return new PilotLookupModel
        {
            Found = true,
            AlreadyLinked = await _bindingService.IsPilotLinkedAsync(pilot.Id),
            Name = pilot.Name,
            Country = pilot.Country,
            TotalRaceDays = pilot.TotalRaceDays,
            LastRaceDate = pilot.LastRaceDate
        };
    }

    /// <summary>
    /// Opens a claim for the given pilot name. The link completes automatically
    /// once that pilot posts a result on a daily track while the claim is active.
    /// </summary>
    [HttpPost("claim")]
    public async Task<ActionResult<ProfileModel>> Claim([FromBody] ClaimPilotRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.PilotName))
            return BadRequest("Pilot name is required");

        if (request.PilotName.Trim().Length > 128)
            return BadRequest("Pilot name is too long");

        var result = await _bindingService.ClaimAsync(user, request.PilotName);

        if (result is not ClaimPilotResult.Claimed)
            Log.Warning("Rejected claim from user {UserId} for pilot {PilotName}: {Reason}",
                user.Id, request.PilotName, result);

        return result switch
        {
            ClaimPilotResult.AlreadyLinked => BadRequest("Your account is already linked to a pilot"),
            ClaimPilotResult.PilotTaken => Conflict("This pilot is already linked to another account"),
            ClaimPilotResult.PilotClaimPending => Conflict("This pilot is already being claimed by another user"),
            _ => await BuildProfileAsync(user)
        };
    }

    [HttpDelete("claim")]
    public async Task<ActionResult<ProfileModel>> CancelClaim()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        await _bindingService.CancelClaimAsync(user);
        return await BuildProfileAsync(user);
    }

    /// <summary>
    /// Permanently deletes the account: profile data, sign-in methods, sessions
    /// and pending claims. The linked pilot and their race history are kept.
    /// </summary>
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var deletion = await _userManager.DeleteAsync(user);

        if (!deletion.Succeeded)
        {
            Log.Error("Failed to delete account {UserId}: {Errors}",
                user.Id, string.Join("; ", deletion.Errors.Select(e => e.Description)));
            return Problem("Failed to delete the account", statusCode: StatusCodes.Status500InternalServerError);
        }

        Log.Information("Deleted account {UserId} on user request", user.Id);
        return NoContent();
    }

    private async Task<ProfileModel> BuildProfileAsync(ApplicationUser user)
    {
        var pilot = user.PilotId is null ? null : await _pilots.FindAsync(user.PilotId.Value);
        var claim = await _bindingService.GetClaimAsync(user);
        var email = user.Email ?? string.Empty;

        return new ProfileModel
        {
            DisplayName = user.DisplayName ?? email.Split('@')[0],
            Email = email,
            Pilot = pilot is null
                ? null
                : new LinkedPilotModel
                {
                    Id = pilot.Id,
                    Name = pilot.Name,
                    Country = pilot.Country,
                    DayStreak = pilot.DayStreak,
                    TotalRaceDays = pilot.TotalRaceDays,
                    LastRaceDate = pilot.LastRaceDate
                },
            PendingClaim = claim is null
                ? null
                : new PendingClaimModel
                {
                    PilotName = claim.PilotName,
                    ExpiresAt = claim.ExpiresOn,
                    IsExpired = claim.IsExpired
                }
        };
    }
}
