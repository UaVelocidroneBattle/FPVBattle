using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FpvBattle.Application.Features.Auth.Models;
using FpvBattle.Application.Features.Auth.Services;
using FpvBattle.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FpvBattle.Infrastructure.Auth;

public class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly IOptions<AuthOptions> _options;

    public JwtAccessTokenIssuer(IOptions<AuthOptions> options)
    {
        _options = options;
    }

    public string CreateAccessToken(ApplicationUser user, DateTime expiresAt)
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
}
