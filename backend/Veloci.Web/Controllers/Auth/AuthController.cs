using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Auth.Models;
using Veloci.Logic.Features.Auth.Services;

namespace Veloci.Web.Controllers.Auth;

[ApiController]
[Route("/api/auth/[action]")]
public class AuthController : ControllerBase
{
    private readonly GoogleAuthService _googleAuthService;
    private readonly TokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        GoogleAuthService googleAuthService,
        TokenService tokenService,
        UserManager<ApplicationUser> userManager)
    {
        _googleAuthService = googleAuthService;
        _tokenService = tokenService;
        _userManager = userManager;
    }

    /// <summary>
    /// Signs in (or signs up) a user with a Google ID token and returns app tokens.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AuthTokensModel>> Google([FromBody] GoogleSignInRequest request)
    {
        var user = await _googleAuthService.AuthenticateAsync(request.IdToken);

        if (user is null)
            return Unauthorized();

        return await _tokenService.IssueTokensAsync(user);
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new token pair. The used token is revoked.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AuthTokensModel>> Refresh([FromBody] RefreshRequest request)
    {
        var storedToken = await _tokenService.FindActiveTokenAsync(request.RefreshToken);

        if (storedToken is null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(storedToken.UserId);

        if (user is null)
            return Unauthorized();

        await _tokenService.RevokeAsync(storedToken);
        return await _tokenService.IssueTokensAsync(user);
    }

    /// <summary>
    /// Revokes the given refresh token. Possession of the token is sufficient authorization.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        var storedToken = await _tokenService.FindActiveTokenAsync(request.RefreshToken);

        if (storedToken is not null)
            await _tokenService.RevokeAsync(storedToken);

        return NoContent();
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<CurrentUserModel>> Me()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var email = user.Email ?? string.Empty;

        return new CurrentUserModel
        {
            Id = user.Id,
            Email = email,
            DisplayName = user.DisplayName ?? email.Split('@')[0]
        };
    }
}
