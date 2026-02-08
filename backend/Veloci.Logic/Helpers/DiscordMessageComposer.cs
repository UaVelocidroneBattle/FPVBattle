using System.Text;
using Veloci.Data.Domain;
using Veloci.Logic.Bot;
using Veloci.Logic.Services.Statistics;
using Veloci.Logic.Services.Statistics.YearResults;

namespace Veloci.Logic.Helpers;

public class DiscordMessageComposer
{
    const int PilotNameMaxLength = 15;

    public string TimeUpdate(IEnumerable<TrackTimeDelta> deltas)
    {
        var messages = deltas.Select(TimeUpdate);
        return string.Join($"{Environment.NewLine}{Environment.NewLine}", messages);
    }

    public string StartCompetition(Track track, ICollection<string> pilotsFlownOnTrack)
    {
        var rating = string.Empty;

        if (track.Rating?.Value is not null)
        {
            rating = $"Previous rating: **{Math.Round(track.Rating.Value.Value, 1):F1}**/3{Environment.NewLine}{Environment.NewLine}";
        }

        var flownPilotsText = pilotsFlownOnTrack.Count != 0 ?
            $"Already flown this track:{Environment.NewLine}**{string.Join(", ", pilotsFlownOnTrack)}**{Environment.NewLine}" :
            $"No one has flown this track yet.{Environment.NewLine}";

        return $"## 📅  Welcome to **FPV Battle**!{Environment.NewLine}{Environment.NewLine}" +
               $"Track of the day:{Environment.NewLine}" +
               $"{track.Map.Name} - **{track.Name}**{Environment.NewLine}{Environment.NewLine}" +
               $"{rating}" +
               $"[Velocidrone leaderboard](https://www.velocidrone.com/leaderboard/{track.Map.MapId}/{track.TrackId}/All){Environment.NewLine}{Environment.NewLine}" +
               $"{flownPilotsText}{Environment.NewLine}" +
               $"👾 Instructions, statistics and more here:{Environment.NewLine}*https://ua-velocidrone.fun/*{Environment.NewLine}⠀";
    }

    public BotPoll Poll(string trackName)
    {
        var question = $"Rate the track {trackName}{Environment.NewLine}{Environment.NewLine}" +
               $"Don't forget to rate the tracks!";

        var options = new List<BotPollOption>
        {
            new (3, "One of the best"),
            new (2, "Like it"),
            new (1, "It's okay"),
            new (-1, "Not great"),
            new (-2, "Terrible")
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

    public string TempLeaderboard(List<CompetitionResults>? results)
    {
        var message = $"### 🧐 Leaderboard:{Environment.NewLine}{Environment.NewLine}⠀";

        if (results is null || results.Count == 0)
        {
            return $"{message}```Waiting for the first results```";
        }

        var rows = TempLeaderboardRows(results);
        return $"{message}" +
               $"```{string.Join($"{Environment.NewLine}", rows)}```";
    }

    public string Leaderboard(IEnumerable<CompetitionResults> results)
    {
        var rows = results.Select(LeaderboardRow);
        var divider = Environment.NewLine;
        return $"### 🏆 Leaderboard{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{divider}", rows)}";
    }

    public string TempSeasonResults(IEnumerable<SeasonResult> results, bool includeExtraNewLine = true)
    {
        var rows = results.Select(TempSeasonResultsRow);
        var divider = includeExtraNewLine ? $"{Environment.NewLine}{Environment.NewLine}" : Environment.NewLine;
        return $"### 🗓 Monthly results{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{divider}", rows)}" +
               $"{Environment.NewLine}{Environment.NewLine}⠀";
    }

    public string SeasonResults(IEnumerable<SeasonResult> results)
    {
        var rows = results.Select(SeasonResultsRow);
        return $"### 🏁 Final monthly results{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{Environment.NewLine}{Environment.NewLine}", rows)}" +
               $"{Environment.NewLine}{Environment.NewLine}⠀";
    }

    public string MedalCount(IEnumerable<SeasonResult> results, bool includeExtraNewLine = true)
    {
        var rows = results
            .Select(MedalCountRow)
            .Where(row => row is not null);

        var divider = includeExtraNewLine ? $"{Environment.NewLine}{Environment.NewLine}" : Environment.NewLine;

        return $"## Monthly medals{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{divider}", rows)}" +
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

        message += $"{Environment.NewLine}Quick, fire up your simulators and fly! 🚀";

        return message;
    }

    public string NewPilot(string name)
    {
        return $"🎉 Welcome new pilot **{name}**";
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

        return $"✈️  **{TextHelper.Trim(delta.Pilot.Name, PilotNameMaxLength)}**{modelPart}{Environment.NewLine}" +
               $"⏱️  {TrackTimeConverter.MsToSec(delta.TrackTime)}s{timeChangePart} / #{delta.Rank}{rankOldPart}";
    }

    private List<string> TempLeaderboardRows(List<CompetitionResults> results)
    {
        var positionLength = results.Count.ToString().Length + 2;
        var pilotNameLength = Math.Min(results.Max(r => r.Pilot.Name.Length), PilotNameMaxLength) + 2;
        var timeLength = results.Max(r => TrackTimeConverter.MsToSec(r.TrackTime).ToString().Length) + 3;
        var rows = new List<string>();

        foreach (var result in results)
        {
            var pilotName = TextHelper.Trim(result.Pilot.Name, PilotNameMaxLength);
            rows.Add($"{FillWithSpaces(result.LocalRank, positionLength)}{FillWithSpaces(pilotName, pilotNameLength)}{FillWithSpaces(TrackTimeConverter.MsToSec(result.TrackTime) + "s", timeLength)}{result.ModelName}");
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

        return $"{icon} - **{TextHelper.Trim(time.Pilot.Name, PilotNameMaxLength)}** ({TrackTimeConverter.MsToSec(time.TrackTime)}s) / Points: **{time.Points}**";
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

    private string? MedalCountRow(SeasonResult result)
    {
        if (result is { GoldenCount: 0, SilverCount: 0, BronzeCount: 0 })
            return null;

        var medals = $"{MedalsRow("🥇", result.GoldenCount)}{MedalsRow("🥈", result.SilverCount)}{MedalsRow("🥉", result.BronzeCount)}";
        return $"**{TextHelper.Trim(result.PlayerName, PilotNameMaxLength)}**:{Environment.NewLine}{medals}";
    }

    private string MedalsRow(string medalIcon, int count)
    {
        var result = new StringBuilder();

        for (var i = 0; i < count; i++)
        {
            result.Append(medalIcon);
        }

        return result.ToString();
    }

    private static string GetFreezieText(int number) => number == 1 ? $"{number} freezie" : $"{number} freezies";

    #endregion
}
