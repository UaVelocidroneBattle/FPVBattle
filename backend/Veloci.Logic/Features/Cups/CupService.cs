using Microsoft.Extensions.Options;

namespace Veloci.Logic.Features.Cups;

/// <summary>
/// Service for accessing and validating cup configurations
/// </summary>
public interface ICupService
{
    /// <summary>
    /// Gets configuration options for a specific cup
    /// </summary>
    /// <param name="cupId">Cup identifier (e.g., "5inch", "whoop")</param>
    /// <returns>Cup configuration options</returns>
    /// <exception cref="ArgumentException">Thrown when cup ID is not found in configuration</exception>
    CupOptions GetCupOptions(string cupId);

    /// <summary>
    /// Gets all enabled cup IDs
    /// </summary>
    /// <returns>Collection of enabled cup identifiers</returns>
    IEnumerable<string> GetEnabledCupIds();

    /// <summary>
    /// Checks if a cup exists in configuration
    /// </summary>
    /// <param name="cupId">Cup identifier to check</param>
    /// <returns>True if cup exists, false otherwise</returns>
    bool CupExists(string cupId);

    /// <summary>
    /// Gets all cup configurations (enabled and disabled)
    /// </summary>
    /// <returns>Dictionary of cup ID to cup options</returns>
    IReadOnlyDictionary<string, CupOptions> GetAllCups();
}

public class CupService : ICupService
{
    private readonly CupsConfiguration _config;

    public CupService(IOptions<CupsConfiguration> config)
    {
        _config = config.Value;
    }

    public CupOptions GetCupOptions(string cupId)
    {
        if (!_config.Cups.TryGetValue(cupId, out var options))
        {
            throw new ArgumentException($"Cup '{cupId}' not found in configuration", nameof(cupId));
        }

        return options;
    }

    public IEnumerable<string> GetEnabledCupIds()
    {
        return _config.Cups
            .Where(c => c.Value.IsEnabled)
            .Select(c => c.Key);
    }

    public bool CupExists(string cupId)
    {
        return _config.Cups.ContainsKey(cupId);
    }

    public IReadOnlyDictionary<string, CupOptions> GetAllCups()
    {
        return _config.Cups;
    }
}
