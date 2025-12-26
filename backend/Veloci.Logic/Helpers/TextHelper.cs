namespace Veloci.Logic.Helpers;

public static class TextHelper
{
    public static string Trim(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return string.Concat(text.AsSpan(0, maxLength - 2), "..");
    }
}
