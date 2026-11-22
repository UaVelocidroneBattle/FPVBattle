namespace FpvBattle.Application.Features.Patreon.Models;

public class PatreonOptions
{
    public const string SectionName = "Patreon";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public bool EnableSync { get; set; } = false;

    /// <summary>
    /// Freezies each paying patron gets on the 1st of the month, keyed by Patreon tier name. A tier not listed gets none
    /// </summary>
    public Dictionary<string, int> MonthlyFreeziesByTier { get; set; } = new();

    public int MonthlyFreeziesFor(string? tierName) =>
        tierName is not null && MonthlyFreeziesByTier.TryGetValue(tierName, out var freezies) ? freezies : 0;
}
