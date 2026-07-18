using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Auth.Models;

namespace Veloci.Logic.Features.Auth.Services;

/// <summary>
/// Issues short-lived JWT access tokens and long-lived opaque refresh tokens.
/// Refresh tokens are stored hashed and rotated on every use.
/// </summary>
public class TokenService
{
    private readonly IRepository<RefreshToken> _refreshTokens;
    private readonly IOptions<AuthOptions> _options;

    public TokenService(IRepository<RefreshToken> refreshTokens, IOptions<AuthOptions> options)
    {
        _refreshTokens = refreshTokens;
        _options = options;
    }

    public async Task<AuthTokensModel> IssueTokensAsync(ApplicationUser user)
    {
        var options = _options.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(options.AccessTokenLifetimeMinutes);
        var refreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));

        await _refreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(refreshToken),
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(options.RefreshTokenLifetimeDays)
        });

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
        var stored = await _refreshTokens.GetAll(t => t.TokenHash == tokenHash).FirstOrDefaultAsync();

        return stored?.IsActive == true ? stored : null;
    }

    public async Task RevokeAsync(RefreshToken token)
    {
        token.RevokedOn = DateTime.UtcNow;
        await _refreshTokens.UpdateAsync(token);
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
