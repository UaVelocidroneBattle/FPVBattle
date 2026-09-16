using System.ComponentModel;
using System.Globalization;

namespace Veloci.Logic.Features.Leagues;

public enum LookBackUnit
{
    Day,
    Week,
    Month
}

/// <summary>
/// How far back a pace rating calculation reaches, written in configuration as "30 days",
/// "2 weeks" or "1 month". A month is a calendar month, so a calculation run on the 1st covers
/// exactly the month that just ended — which is the window the monthly league distribution reads.
/// </summary>
[TypeConverter(typeof(LookBackPeriodConverter))]
public readonly record struct LookBackPeriod(int Count, LookBackUnit Unit)
{
    public DateTime StartOfWindowEndingAt(DateTime end) => Unit switch
    {
        LookBackUnit.Day => end.AddDays(-Count),
        LookBackUnit.Week => end.AddDays(-7 * Count),
        LookBackUnit.Month => end.AddMonths(-Count),
        _ => throw new InvalidOperationException($"Unsupported look back unit '{Unit}'.")
    };

    public static LookBackPeriod Parse(string value)
    {
        var parts = value?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

        if (parts.Length == 2 &&
            int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var count) &&
            count > 0 &&
            TryParseUnit(parts[1], out var unit))
        {
            return new LookBackPeriod(count, unit);
        }

        throw new FormatException(
            $"'{value}' is not a look back period. Expected a count and a unit, such as '30 days', '2 weeks' or '1 month'.");
    }

    // Trailing "s" so that both "1 month" and "2 months" read naturally in config.
    private static bool TryParseUnit(string text, out LookBackUnit unit) =>
        Enum.TryParse(text.TrimEnd('s', 'S'), ignoreCase: true, out unit) && Enum.IsDefined(unit);

    public override string ToString() => $"{Count} {Unit.ToString().ToLowerInvariant()}{(Count == 1 ? "" : "s")}";
}

public class LookBackPeriodConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text ? LookBackPeriod.Parse(text) : base.ConvertFrom(context, culture, value);
}
