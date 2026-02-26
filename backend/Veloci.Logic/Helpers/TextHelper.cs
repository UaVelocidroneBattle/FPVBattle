namespace Veloci.Logic.Helpers;

public static class TextHelper
{
    public static string CountryFlag(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode) || countryCode.Length != 2)
            return string.Empty;

        return string.Concat(countryCode.ToUpper().Select(c => char.ConvertFromUtf32(c + 0x1F1A5)));
    }

    public static string CountryFlagWithSpace(string? countryCode)
    {
        var flag = CountryFlag(countryCode);
        return flag.Length > 0 ? $"{flag} " : string.Empty;
    }

    public static string Trim(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return string.Concat(text.AsSpan(0, maxLength - 2), "..");
    }
}
