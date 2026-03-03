namespace Veloci.Logic.Features.Cups;

/// <summary>
/// Extension methods for ICupService
/// </summary>
public static class CupServiceExtensions
{
    /// <summary>
    /// Gets the Telegram channel ID for a cup, if configured
    /// </summary>
    /// <param name="cupService">Cup service instance</param>
    /// <param name="cupId">Cup identifier</param>
    /// <returns>Telegram channel ID, or null if cup doesn't exist or has no Telegram channel configured</returns>
    public static string? GetTelegramChannelId(this ICupService cupService, string cupId)
    {
        if (!cupService.CupExists(cupId))
            return null;

        return cupService.GetCupOptions(cupId).Channels?.Telegram?.ChannelId;
    }
}
