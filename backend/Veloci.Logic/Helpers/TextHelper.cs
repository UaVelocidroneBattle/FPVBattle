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

    public static List<string> SplitIntoChunks(string message, int chunkSize)
    {
        if (message.Length <= chunkSize)
            return [message];

        var chunks = new List<string>();
        var remaining = message;

        while (remaining.Length > chunkSize)
        {
            var splitAt = remaining.LastIndexOf('\n', chunkSize - 1);

            if (splitAt <= 0)
            {
                // No newline in range — hard cut as a last resort
                chunks.Add(remaining[..chunkSize]);
                remaining = remaining[chunkSize..];
            }
            else
            {
                chunks.Add(remaining[..splitAt]);
                remaining = remaining[(splitAt + 1)..];
            }
        }

        if (remaining.Length > 0)
            chunks.Add(remaining);

        return chunks;
    }
}
