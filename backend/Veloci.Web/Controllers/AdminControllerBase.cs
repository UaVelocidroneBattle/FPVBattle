using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Veloci.Web.Controllers;

/// <summary>
/// Base class for the admin UI pages (track queue, whitelist, user pilots, claims).
/// Requires a signed-in user via the Identity cookie; role-based restrictions
/// will be added here once roles are introduced.
/// </summary>
[Authorize]
public abstract class AdminControllerBase : Controller;
