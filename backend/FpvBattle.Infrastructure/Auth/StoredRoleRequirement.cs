using FpvBattle.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FpvBattle.Infrastructure.Auth;

/// <summary>
/// Checks the role in the database rather than in the access token, so removing
/// a role takes effect at once instead of when the token expires.
/// </summary>
public sealed record StoredRoleRequirement(string Role) : IAuthorizationRequirement;

public sealed class StoredRoleHandler : AuthorizationHandler<StoredRoleRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public StoredRoleHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, StoredRoleRequirement requirement)
    {
        var user = await _userManager.GetUserAsync(context.User);

        if (user is not null && await _userManager.IsInRoleAsync(user, requirement.Role))
            context.Succeed(requirement);
    }
}
