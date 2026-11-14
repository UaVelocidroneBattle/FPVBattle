using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Auth.Services;

public interface IAccessTokenIssuer
{
    string CreateAccessToken(ApplicationUser user, DateTime expiresAt);
}
