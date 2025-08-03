using System.ComponentModel.DataAnnotations;

namespace Veloci.Logic.Features.Patreon.Models;

public class PatreonOptions
{
    public const string SectionName = "Patreon";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public bool EnableSync { get; set; } = false;
}