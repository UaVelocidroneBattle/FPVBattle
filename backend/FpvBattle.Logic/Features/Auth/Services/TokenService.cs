using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Logic.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FpvBattle.Logic.Features.Auth.Services;

/// <summary>
/// Issues short-lived JWT access tokens and long-lived opaque refresh tokens.
/// Refresh tokens are stored hashed and rotated on every use.
/// </summary>
public class TokenService
{
    private readonly IApplicationDbContext _db;
    private readonly IOptions<AuthOptions> _options;

    public TokenService(IApplicationDbContext db, IOptions<AuthOptions> options)
    {
        _db = db;
        _options = options;
    }

    public async Task<AuthTokensModel> IssueTokensAsync(ApplicationUser user)
    {
        var options = _options.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(options.AccessTokenLifetimeMinutes);
        var refreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));

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
            AccessToken = CreateAccessToken(user, expiresAt),
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

    private string CreateAccessToken(ApplicationUser user, DateTime expiresAt)
    {
        var options = _options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.JwtSigningKey));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: options.JwtIssuer,
            audience: options.JwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string Hash(string token)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
