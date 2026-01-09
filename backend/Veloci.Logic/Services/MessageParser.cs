using System.Text.RegularExpressions;

namespace Veloci.Logic.Services;

public static class MessageParser
{
    private static readonly Regex CompetitionRestartRegex =
        new("новий трек, будь ласка", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsCompetitionRestart(string message) => CompetitionRestartRegex.IsMatch(message);

    private static readonly Regex CompetitionStopRegex =
        new("на сьогодні досить", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsCompetitionStop(string message) => CompetitionStopRegex.IsMatch(message);
}
