namespace Veloci.Logic.Features.Cups;

/// <summary>
/// Root configuration for all cups in appsettings.json
/// </summary>
public class CupsConfiguration
{
    public const string SectionName = "Cups";

    public Dictionary<string, CupOptions> Definitions { get; set; } = new();
}

/// <summary>
/// Configuration options for a single cup (competition type)
/// </summary>
public class CupOptions
{
    public string Name { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public ScheduleOptions Schedule { get; set; } = new();

    public TrackFilterOptions TrackFilter { get; set; } = new();

    public ChannelOptions Channels { get; set; } = new();
}

/// <summary>
/// Scheduling configuration for competition start/stop times
/// </summary>
public class ScheduleOptions
{
    /// <summary>
    /// Start time in HH:mm format (e.g., "15:03")
    /// </summary>
    public string StartTime { get; set; } = "15:03";

    /// <summary>
    /// Time zone for the schedule (e.g., "UTC")
    /// </summary>
    public string TimeZone { get; set; } = "UTC";
}

/// <summary>
/// Track filtering configuration for cup-specific track selection
/// </summary>
public class TrackFilterOptions
{
    /// <summary>
    /// Patterns to exclude tracks (for 5-inch cup: Whoop, Micro, etc.)
    /// </summary>
    public List<string> BlacklistPatterns { get; set; } = new();

    /// <summary>
    /// Patterns to include tracks (for whoop cup: Whoop, Micro, Toothpick)
    /// Takes precedence over blacklist when specified.
    /// </summary>
    public List<string> WhitelistPatterns { get; set; } = new();
}

/// <summary>
/// Channel configuration for Discord and Telegram
/// </summary>
public class ChannelOptions
{
    public TelegramChannelOptions? Telegram { get; set; }

    public DiscordChannelOptions? Discord { get; set; }
}

/// <summary>
/// Telegram-specific channel configuration
/// </summary>
public class TelegramChannelOptions
{
    /// <summary>
    /// Telegram channel ID (e.g., "-1001234567890")
    /// </summary>
    public string ChannelId { get; set; } = string.Empty;
}

/// <summary>
/// Discord-specific channel configuration
/// </summary>
public class DiscordChannelOptions
{
    /// <summary>
    /// Discord channel name (e.g., "5inch-daily")
    /// </summary>
    public string Channel { get; set; } = string.Empty;
}
