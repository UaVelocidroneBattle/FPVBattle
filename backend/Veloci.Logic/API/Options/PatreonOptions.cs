using System.ComponentModel.DataAnnotations;

namespace Veloci.Logic.API.Options;

public class PatreonOptions
{
    public const string SectionName = "Patreon";

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    [Required]
    public string RedirectUri { get; set; } = string.Empty;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }
}