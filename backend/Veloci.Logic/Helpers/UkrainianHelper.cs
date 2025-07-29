using System.Runtime.InteropServices;

namespace Veloci.Logic.Helpers;

public class UkrainianHelper
{
    /// <summary>
    /// Returns the correct form of the word "раз" depending on the number.
    /// </summary>
    public static string GetTimesString(int number)
    {
        if (number % 100 >= 11 && number % 100 <= 14)
            return "разів";

        return (number % 10) switch
        {
            1 => "раз",
            2 or 3 or 4 => "рази",
            _ => "разів"
        };
    }

    public static DateTime GetCurrentKyivTime()
    {
        var utcNow = DateTime.UtcNow;
        return ConvertToKyivTime(utcNow);
    }

    public static DateTime ConvertToKyivTime(DateTime utcTime)
    {
        var ukraineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, ukraineTimeZone);
    }
}
