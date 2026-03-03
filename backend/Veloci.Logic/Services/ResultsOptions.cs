namespace Veloci.Logic.Services;

/// <summary>
/// Global configuration for race results filtering
/// </summary>
public class ResultsOptions
{
    public const string SectionName = "Results";

    /// <summary>
    /// Country codes whose results are always excluded, regardless of whitelist membership (e.g., "RU")
    /// </summary>
    public List<string> CountriesBlackList { get; set; } = new();
}