using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using FpvBattle.Application.Features.Auth.Models;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FpvBattle.Application.Features.Auth.Services;

/// <summary>
/// Issues short-lived JWT access tokens and long-lived opaque refresh tokens.
/// Refresh tokens are stored hashed and rotated on every use.
/// </summary>
public class TokenService
{
    private readonly IApplicationDbContext _db;
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly IOptions<AuthOptions> _options;

    public TokenService(IApplicationDbContext db, IAccessTokenIssuer accessTokenIssuer, IOptions<AuthOptions> options)
    {
        _db = db;
        _accessTokenIssuer = accessTokenIssuer;
        _options = options;
    }

    public async Task<AuthTokensModel> IssueTokensAsync(ApplicationUser user)
    {
        var options = _options.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(options.AccessTokenLifetimeMinutes);
        var refreshToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(refreshToken),
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(options.RefreshTokenLifetimeDays)
        });
        await _db.SaveChangesAsync();

        return new AuthTokensModel
        {
            AccessToken = _accessTokenIssuer.CreateAccessToken(user, expiresAt),
            AccessTokenExpiresAt = expiresAt,
            RefreshToken = refreshToken
        };
    }

    public async Task<RefreshToken?> FindActiveTokenAsync(string refreshToken)
    {
        var tokenHash = Hash(refreshToken);
        var stored = await _db.RefreshTokens.Where(t => t.TokenHash == tokenHash).FirstOrDefaultAsync();

        return stored?.IsActive == true ? stored : null;
    }

    public async Task RevokeAsync(RefreshToken token)
    {
        token.RevokedOn = DateTime.UtcNow;
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync();
    }

    private static string Hash(string token)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
