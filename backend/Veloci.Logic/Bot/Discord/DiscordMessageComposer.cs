using System.Text;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Leagues.Models;
using Veloci.Logic.Helpers;
using Veloci.Logic.Services.Statistics;
using Veloci.Logic.Services.Statistics.YearResults;

namespace Veloci.Logic.Bot.Discord;

public class DiscordMessageComposer
{
    const int PilotNameMaxLength = 15;

    public string TimeUpdate(IEnumerable<TrackTimeDelta> deltas)
    {
        var messages = deltas.Select(TimeUpdate);
        return string.Join($"{Environment.NewLine}{Environment.NewLine}", messages);
    }

    public string StartCompetition(Track track, ICollection<string> pilotsFlownOnTrack, string? quadOfTheDay)
    {
        var rating = string.Empty;

        if (track.Rating?.Value is not null)
        {
            rating = $"Previous rating: **{Math.Round(track.Rating.Value.Value, 1):F1}**{Environment.NewLine}{Environment.NewLine}";
        }

        var flownPilotsText = pilotsFlownOnTrack.Count != 0 ?
            $"Already flown this track:{Environment.NewLine}**{string.Join(", ", pilotsFlownOnTrack)}**{Environment.NewLine}" :
            $"No one has flown this track yet.{Environment.NewLine}";

        var quadOfTheDayText = quadOfTheDay is null
            ? string.Empty
            : $"⚠️ Quad of the day: **{quadOfTheDay}**{Environment.NewLine}{Environment.NewLine}";

        return $"## 📅  Welcome to **FPV Battle**!{Environment.NewLine}{Environment.NewLine}" +
               $"Track of the day:{Environment.NewLine}" +
               $"{track.Map.Name} - **{track.Name}**{Environment.NewLine}{Environment.NewLine}" +
               $"{rating}" +
               $"{quadOfTheDayText}" +
               $"[Velocidrone leaderboard](https://www.velocidrone.com/leaderboard/{track.Map.MapId}/{track.TrackId}/All){Environment.NewLine}{Environment.NewLine}" +
               $"{flownPilotsText}{Environment.NewLine}" +
               $"👾 Instructions, statistics and more here:{Environment.NewLine}https://ua-velocidrone.fun/{Environment.NewLine}⠀";
    }

    public BotPoll Poll(string trackName)
    {
        var question = $"Rate the track - {trackName}";

        var options = new List<PollOption>
        {
            new (5, "⭐⭐⭐⭐⭐ - One of the best"),
            new (4, "⭐⭐⭐⭐ - Like it"),
            new (3, "⭐⭐⭐ - It's okay"),
            new (2, "⭐⭐ - Nah"),
            new (1, "⭐ - Piece of shit")
        };

        return new BotPoll
        {
            Question = question,
            Options = options
        };
    }

    public string BadTrackRating()
    {
        return "😔 Looks like the track wasn't well received. It won't appear again";
    }

    public Dictionary<string, string> TempLeaderboard(List<LeagueLeaderboard>? leaderboard, IReadOnlyList<string> configuredLeagueNames)
    {
        var showHeaders = configuredLeagueNames.Count > 1;

        if (leaderboard is null || leaderboard.Count == 0)
            return configuredLeagueNames.ToDictionary(name => name, name => LeagueNoResultsMessage(name, showHeaders));

        return leaderboard.ToDictionary(
            l => l.League,
            l => l.Results.Count == 0
                ? LeagueNoResultsMessage(l.League, showHeaders)
                : BuildLeagueMessage(l, showHeaders));
    }

    private string BuildLeagueMessage(LeagueLeaderboard league, bool showHeader)
    {
        var header = showHeader ? LeagueHeader(league.League) : string.Empty;
        var message = BuildTempLeaderboard(header, league.Results, includeModelNames: true);
        return message.Length <= 2000 ? message : BuildTempLeaderboard(header, league.Results, includeModelNames: false);
    }

    private string LeagueNoResultsMessage(string leagueName, bool showHeader)
    {
        var header = showHeader ? LeagueHeader(leagueName) : string.Empty;
        return $"{header}```No results```";
    }

    private static string LeagueHeader(string leagueName)
    {
        return $"### {leagueName.ToUpper()}{Environment.NewLine}{Environment.NewLine}⠀";
    }

    private string BuildTempLeaderboard(string header, List<CompetitionResults> results, bool includeModelNames)
    {
        var rows = TempLeaderboardRows(results, includeModelNames);
        return $"{header}```{string.Join(Environment.NewLine, rows)}```";
    }

    public Dictionary<string, string> Leaderboard(List<LeagueLeaderboard> leaderboard)
    {
        var showHeaders = leaderboard.Count > 1;

        return leaderboard.ToDictionary(
            l => l.League,
            l => BuildFinalLeagueMessage(l, showHeaders));
    }

    private string BuildFinalLeagueMessage(LeagueLeaderboard league, bool showHeader)
    {
        var title = showHeader ? $"### {league.League.ToUpper()}" : "### 🏆 Leaderboard";
        var rows = league.Results.Any() ? string.Join(Environment.NewLine, league.Results.Select(LeaderboardRow)) : "no results";
        return $"{title}{Environment.NewLine}{Environment.NewLine}{rows}";
    }

    public string TempSeasonResults(List<LeagueSeasonLeaderboard> leaderboard)
    {
        var showHeaders = leaderboard.Count > 1;
        var sectionDivider = $"{Environment.NewLine}{Environment.NewLine}";
        var sections = leaderboard.Select(l =>
        {
            var header = showHeaders ? $"**{l.League?.ToUpper()}**{Environment.NewLine}{Environment.NewLine}" : "";
            var rows = l.Results.Any() ? string.Join(Environment.NewLine, l.Results.Select(TempSeasonResultsRow)) : "no results";
            return $"{header}{rows}";
        });

        return $"### 🗓 Monthly results{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join(sectionDivider, sections)}" +
               $"{Environment.NewLine}{Environment.NewLine}⠀";
    }

    public string SeasonResults(List<LeagueSeasonLeaderboard> leaderboard)
    {
        var showHeaders = leaderboard.Count > 1;

        var sections = leaderboard.Select(l =>
        {
            var header = showHeaders ? $"**{l.League?.ToUpper()}**{Environment.NewLine}{Environment.NewLine}" : "";
            var rows = l.Results.Any() ? string.Join(Environment.NewLine, l.Results.Select(SeasonResultsRow)) : "no results";
            return $"{header}{rows}";
        });

        return $"### 🏁 Final monthly results{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{Environment.NewLine}{Environment.NewLine}", sections)}" +
               $"{Environment.NewLine}{Environment.NewLine}⠀";
    }

    public IEnumerable<string> YearResults(YearResultsModel model)
    {
        var first = $"🎉 *FPV Battle WRAPPED 📈 {model.Year}*{Environment.NewLine}" +
               $"or a few numbers from the past year{Environment.NewLine}{Environment.NewLine}" +
               $"📊 *{model.TotalTrackCount} tracks!* That's how many we flew last year.{Environment.NewLine}" +
               $"Of those, *{model.UniqueTrackCount}* were unique. Yes, some tracks repeated, but that's how our algorithms work.{Environment.NewLine}" +
               $"On the other hand, it's a great chance to beat yourself and see your progress.{Environment.NewLine}{Environment.NewLine}" +
               $"👎 *{model.TracksSkipped} tracks* were so bad they had to be replaced immediately.{Environment.NewLine}{Environment.NewLine}" +
               $"👍 But your favorite track of the year:{Environment.NewLine}" +
               $"*{model.FavoriteTrack}*{Environment.NewLine}" +
               $"This is the winner by your votes!";

        var second = $"👥 Last year we saw *{model.TotalPilotCount}* pilots here.{Environment.NewLine}{Environment.NewLine}" +
                     $"🥷 *Attendance champion: {model.PilotWhoCameTheMost.name}.* This daredevil flew *{model.PilotWhoCameTheMost.count} tracks* in a year!{Environment.NewLine}" +
                     $"{model.PilotWhoCameTheMost.name}, are you even human? 🤖{Environment.NewLine}{Environment.NewLine}" +
                     $"🧐 *Rare appearance award: {model.PilotWhoCameTheLeast.name}* Showed up only {model.PilotWhoCameTheLeast.count} time(s).{Environment.NewLine}" +
                     $"{model.PilotWhoCameTheLeast.name}, we miss you here!{Environment.NewLine}{Environment.NewLine}" +
                     $"🥇 *Mr. Gold: {model.PilotWithTheMostGoldenMedal.name}.* This genius collected *{model.PilotWithTheMostGoldenMedal.count}* gold medals!";

        var third = $"🏆 And here are the *TOP 3* pilots who scored the most total points this year:{Environment.NewLine}{Environment.NewLine}";

        foreach (var pilot in model.Top3Pilots)
        {
            third += $"*{pilot.Key}* - *{pilot.Value}* points{Environment.NewLine}";
        }

        third += $"{Environment.NewLine}Not bad, right? Thank you for continuing to fly and getting even faster! 🚀";

        return new List<string>()
        {
            first,
            second,
            third
        };
    }

    public string DayStreakPotentialLose(IEnumerable<Pilot> pilots)
    {
        var message = $"## ⚠️ WARNING!{Environment.NewLine}" +
                      $"Day streak at risk:{Environment.NewLine}{Environment.NewLine}";

        foreach (var pilot in pilots)
        {
            message += $"**{TextHelper.Trim(pilot.Name, PilotNameMaxLength)}** - **{pilot.DayStreak}** streak ({GetFreezieText(pilot.DayStreakFreezeCount)}){Environment.NewLine}";
        }

        message += $"{Environment.NewLine}Quick, fire up your simulators and fly, you have 2 hours!";

        return message;
    }

    public string NewPilot(Pilot pilot)
    {
        return $"🎉 Welcome new pilot  {TextHelper.CountryFlagWithSpace(pilot.Country)} **{pilot.Name}**";
    }

    public string PilotRenamed(string oldName, string newName)
    {
        return $"✏️ Pilot **{oldName}** renamed to **{newName}**";
    }

    public string EndOfSeasonStatistics(EndOfSeasonStatisticsDto statistics)
    {
        return $"📊 **Some statistics for season {statistics.SeasonName}**{Environment.NewLine}{Environment.NewLine}" +
               $"▪️ Average pilots per day: **{statistics.AveragePilotsLastMonth}**{Environment.NewLine}" +
               $"▪️ Average pilots per day (last 12 months): **{statistics.AveragePilotsLastYear}**{Environment.NewLine}" +
               $"▪️ Most pilots in a day: **{statistics.MaxPilotsLastMonth}**{Environment.NewLine}" +
               $"▪️ Fewest pilots in a day: **{statistics.MinPilotsLastMonth}**{Environment.NewLine}";
    }

    public string FreezieAdded(string pilotName)
    {
        return $"❄️ **{pilotName}** received an extra freezie";
    }

    public string RestartTrack()
    {
        return "🔁️ Hands off the controllers everyone, we're **changing the track**";
    }

    #region Private

    private string TimeUpdate(TrackTimeDelta delta)
    {
        var timeChangePart = delta.TimeChange.HasValue ? $" ({TrackTimeConverter.MsToSec(delta.TimeChange.Value)}s)" : string.Empty;
        var rankOldPart = delta.RankOld.HasValue ? $" (#{delta.RankOld})" : string.Empty;
        var modelPart = delta.ModelName is not null ? $" / {delta.ModelName}" : string.Empty;
        var flag = TextHelper.CountryFlagWithSpace(delta.Country);

        return $"{flag} **{TextHelper.Trim(delta.Pilot.Name, PilotNameMaxLength)}**{modelPart}{Environment.NewLine}" +
               $"⏱️  {TrackTimeConverter.MsToSec(delta.TrackTime)}s{timeChangePart} / #{delta.Rank}{rankOldPart}";
    }

    private List<string> TempLeaderboardRows(List<CompetitionResults> results, bool includeModelNames = true)
    {
        var positionLength = results.Count.ToString().Length + 2;
        var pilotNameLength = Math.Min(results.Max(r => r.Pilot.Name.Length), PilotNameMaxLength) + 2;
        var timeLength = results.Max(r => TrackTimeConverter.MsToSec(r.TrackTime).ToString().Length) + 3;
        var rows = new List<string>();

        foreach (var result in results)
        {
            var pilotName = TextHelper.Trim(result.Pilot.Name, PilotNameMaxLength);
            var modelName = includeModelNames ? result.ModelName : string.Empty;
            rows.Add($"{FillWithSpaces(result.LocalRank, positionLength)}{FillWithSpaces(pilotName, pilotNameLength)}{FillWithSpaces(TrackTimeConverter.MsToSec(result.TrackTime) + "s", timeLength)}{modelName}");
        }

        return rows;
    }

    private string FillWithSpaces(object text, int length)
    {
        var textString = text.ToString();
        var spaces = new string(' ', length - textString.Length);
        return textString + spaces;
    }

    private string LeaderboardRow(CompetitionResults time)
    {
        var icon = time.LocalRank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => $"#{time.LocalRank}"
        };

        var points = $"Pts: **{time.Points}**";

        if (time.BonusPoints > 0)
            points += $" +**{time.BonusPoints}**";

        return $"{icon} - **{TextHelper.Trim(time.Pilot.Name, PilotNameMaxLength)}** ({TrackTimeConverter.MsToSec(time.TrackTime)}s) / {points}";
    }

    private string TempSeasonResultsRow(SeasonResult result)
    {
        return $"{result.Rank} - **{TextHelper.Trim(result.PlayerName, PilotNameMaxLength)}** - {result.Points} points";
    }

    private string SeasonResultsRow(SeasonResult result)
    {
        var icon = result.Rank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => $"{result.Rank}"
        };

        return $"{icon} - **{TextHelper.Trim(result.PlayerName, PilotNameMaxLength)}** - {result.Points} points";
    }


    private static string GetFreezieText(int number) => number == 1 ? $"{number} freezie" : $"{number} freezies";

    public string LeagueUpdates(IList<LeagueUpdateModel> updates)
    {
        var sb = new StringBuilder($"🏆 **League updates:**{Environment.NewLine}{Environment.NewLine}");

        foreach (var update in updates)
        {
            var line = update switch
            {
                { OldLeague: null } => $"▫️ {update.PilotName} → **{update.NewLeague?.ToUpper()}**",
                { NewLeague: null } => $"▫️ {update.PilotName} leaves **{update.OldLeague?.ToUpper()}**",
                _ => $"▫️ {update.PilotName} **{update.OldLeague?.ToUpper()}** → **{update.NewLeague?.ToUpper()}**"
            };

            sb.AppendLine(line);
        }

        return sb.ToString().TrimEnd();
    }

    #endregion
}
