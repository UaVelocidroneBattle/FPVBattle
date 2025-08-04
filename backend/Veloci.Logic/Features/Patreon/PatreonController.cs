using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Veloci.Logic.Features.Patreon.Services;

namespace Veloci.Logic.Features.Patreon;

public class PatreonController : Controller
{
    private readonly ILogger<PatreonController> _logger;
    private readonly IPatreonOAuthService _oauthService;
    private readonly IPatreonTokenManager _tokenManager;

    public PatreonController(
        ILogger<PatreonController> logger,
        IPatreonOAuthService oauthService,
        IPatreonTokenManager tokenManager)
    {
        _logger = logger;
        _oauthService = oauthService;
        _tokenManager = tokenManager;
    }

    public IActionResult Connect()
    {
        try
        {
            var state = Guid.NewGuid().ToString();
            var authUrl = _oauthService.GenerateAuthorizationUrl(state);
            return Redirect(authUrl);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Patreon OAuth configuration error");
            return BadRequest("Patreon configuration is not set up");
        }
    }

    public async Task<IActionResult> Callback(string? code, string? state, string? error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogError("Patreon OAuth error: {Error}", error);
            return BadRequest($"Authorization failed: {error}");
        }

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogError("Authorization code not received from Patreon");
            return BadRequest("Authorization code not received");
        }

        try
        {
            var tokenResponse = await _oauthService.ExchangeCodeForTokensAsync(code);
            if (tokenResponse == null)
            {
                return BadRequest("Failed to exchange authorization code for tokens");
            }

            await _tokenManager.UpdateStoredTokensAsync(tokenResponse.AccessToken, tokenResponse.RefreshToken,
                tokenResponse.ExpiresIn, tokenResponse.Scope);

            return View("Tokens", tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Patreon OAuth callback");
            return BadRequest("An error occurred during authorization");
        }
    }
}
