namespace Veloci.Logic.Features.Auth.Models;

public class GoogleSignInRequest
{
    public required string IdToken { get; set; }
}

public class RefreshRequest
{
    public required string RefreshToken { get; set; }
}

public class AuthTokensModel
{
    public required string AccessToken { get; set; }

    public required DateTime AccessTokenExpiresAt { get; set; }

    public required string RefreshToken { get; set; }
}

public class CurrentUserModel
{
    public required string Id { get; set; }

    public required string Email { get; set; }

    public required string DisplayName { get; set; }
}
