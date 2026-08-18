namespace Veloci.Web.Controllers.Admin.Statistics;

/// <summary>
/// A time scale the reader can pick for the participation chart. Only the selected
/// range is ever queried, so the page never pays for history nobody is looking at.
/// </summary>
/// <param name="Key">Value used in the URL and in the range picker.</param>
/// <param name="Days">Days back from today, or <c>null</c> for the whole history.</param>
public record ParticipationRange(string Key, string Label, int? Days)
{
    public static IReadOnlyList<ParticipationRange> All { get; } =
    [
        new("month", "Month", 30),
        new("6months", "6 months", 182),
        new("year", "Year", 365),
        new("all", "All", null)
    ];

    public static ParticipationRange Default => All[0];

    public static ParticipationRange Resolve(string? key) =>
        All.FirstOrDefault(range => range.Key == key) ?? Default;
}

public class ParticipationViewModel
{
    public required IReadOnlyList<ParticipationRange> Ranges { get; init; }

    public required ParticipationRange SelectedRange { get; init; }

    /// <summary>
    /// The selected range's chart payload as JSON, embedded in the page so the first
    /// paint needs no round trip. Later range switches fetch their own from the API.
    /// </summary>
    public required string ChartJson { get; init; }
}

/// <summary>
/// Shape handed to the browser. Property names are camel-cased by the serializer,
/// so the script reads <c>days</c> / <c>series</c>.
/// </summary>
public record ParticipationChart(IReadOnlyList<string> Days, IReadOnlyList<CupSeries> Series);

/// <summary>
/// A cup's numbers. Series order is the cup's configuration order, which is what the
/// page turns into a colour — the palette itself lives with the other design tokens
/// in site.css, not here.
/// </summary>
public record CupSeries(string Name, IReadOnlyList<int?> Counts);
