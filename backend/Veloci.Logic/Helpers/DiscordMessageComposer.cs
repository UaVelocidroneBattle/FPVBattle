using System.Globalization;
using System.Text;
using Veloci.Data.Domain;
using Veloci.Logic.Bot;
using Veloci.Logic.Services.YearResults;

namespace Veloci.Logic.Helpers;

public class DiscordMessageComposer
{
    public string TimeUpdate(IEnumerable<TrackTimeDelta> deltas)
    {
        var messages = deltas.Select(TimeUpdate);
        return string.Join($"{Environment.NewLine}{Environment.NewLine}", messages);
    }

    public string StartCompetition(Track track)
    {
        var rating = string.Empty;

        if (track.Rating?.Value is not null)
        {
            rating = $"Попередній рейтинг: **{Math.Round(track.Rating.Value.Value, 1):F1}**/3{Environment.NewLine}{Environment.NewLine}";
        }

        return $"## 📅  Вітаємо на щоденному **UA Velocidrone Battle**!{Environment.NewLine}{Environment.NewLine}" +
               $"Трек дня:{Environment.NewLine}" +
               $"{track.Map.Name} - **{track.Name}**{Environment.NewLine}{Environment.NewLine}" +
               $"{rating}" +
               $"[Velocidrone leaderboard](https://www.velocidrone.com/leaderboard/{track.Map.MapId}/{track.TrackId}/All){Environment.NewLine}⠀";
    }

    public BotPoll Poll(string trackName)
    {
        var question = $"Оцініть трек {trackName}{Environment.NewLine}{Environment.NewLine}" +
               $"Не забувайте оцінювати треки!";

        var options = new List<BotPollOption>
        {
            new (3, "Один із кращих"),
            new (2, "Подобається"),
            new (1, "Нормальний"),
            new (-1, "Не дуже"),
            new (-2, "Лайно")
        };

        return new BotPoll
        {
            Question = question,
            Options = options
        };
    }

    public string BadTrackRating()
    {
        return "😔 Бачу трек не сподобався. Більше його не буде";
    }

    public string TempLeaderboard(List<CompetitionResults>? results)
    {
        var message = $"### 🧐 Leaderboard:{Environment.NewLine}";

        if (results is null || results.Count == 0)
        {
            return $"{message}```Чекаємо на перші результати```";
        }

        var rows = TempLeaderboardRows(results);
        return $"{message}" +
               $"```{string.Join($"{Environment.NewLine}", rows)}```";
    }

    public string Leaderboard(IEnumerable<CompetitionResults> results)
    {
        var rows = results.Select(LeaderboardRow);
        var divider = Environment.NewLine;
        return $"### 🏆 Leaderboard{Environment.NewLine}" +
               $"{string.Join($"{divider}", rows)}";
    }

    public string TempSeasonResults(IEnumerable<SeasonResult> results, bool includeExtraNewLine = true)
    {
        var rows = results.Select(TempSeasonResultsRow);
        var divider = includeExtraNewLine ? $"{Environment.NewLine}{Environment.NewLine}" : Environment.NewLine;
        return $"## 🗓 Проміжні результати місяця{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{divider}", rows)}" +
               $"{Environment.NewLine}{Environment.NewLine}⠀";
    }

    public string SeasonResults(IEnumerable<SeasonResult> results)
    {
        var rows = results.Select(SeasonResultsRow);
        return $"# 🏁 Фінальні результати місяця{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{Environment.NewLine}{Environment.NewLine}", rows)}" +
               $"{Environment.NewLine}{Environment.NewLine}⠀";
    }

    public string MedalCount(IEnumerable<SeasonResult> results, bool includeExtraNewLine = true)
    {
        var rows = results
            .Select(MedalCountRow)
            .Where(row => row is not null);

        var divider = includeExtraNewLine ? $"{Environment.NewLine}{Environment.NewLine}" : Environment.NewLine;

        return $"## Медалі за місяць{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{divider}", rows)}";
    }

    public IEnumerable<string> YearResults(YearResultsModel model)
    {
        var first = $"🎉 *UA Velocidrone Battle WRAPPED 📈 {model.Year}*{Environment.NewLine}" +
               $"або трохи цифр за минулий рік{Environment.NewLine}{Environment.NewLine}" +
               $"📊 *{model.TotalTrackCount} треків!* Це стільки ми пролетіли минулого року.{Environment.NewLine}" +
               $"Із них унікальних - *{model.UniqueTrackCount}*. Так, деякі треки повторювались, але такі вже у нас алгоритми.{Environment.NewLine}" +
               $"З іншого боку, це гарний привід покращити свій же результат і стати ще швидшим.{Environment.NewLine}{Environment.NewLine}" +
               $"👎 *{model.TracksSkipped} треків* були настільки ганебні, що довелось їх одразу замінити.{Environment.NewLine}{Environment.NewLine}" +
               $"👍 Але ваш улюблений трек року:{Environment.NewLine}" +
               $"*{model.FavoriteTrack}*{Environment.NewLine}" +
               $"Це переможець за вашими голосами!";

        var second = $"👥 В минулому році тут з'являлись імена *{model.TotalPilotCount}* пілотів.{Environment.NewLine}{Environment.NewLine}" +
                     $"🥷 *Чемпіон відвідувань: {model.PilotWhoCameTheMost.name}.* Цей відчайдух пролетів *{model.PilotWhoCameTheMost.count} треків* за рік!{Environment.NewLine}" +
                     $"{model.PilotWhoCameTheMost.name}, ти точно людина? 🤖{Environment.NewLine}{Environment.NewLine}" +
                     $"🧐 *Приз за рідкісні появи: {model.PilotWhoCameTheLeast.name}* Він з'явився всього {model.PilotWhoCameTheLeast.count} {UkrainianHelper.GetTimesString(model.PilotWhoCameTheLeast.count)}.{Environment.NewLine}" +
                     $"{model.PilotWhoCameTheLeast.name}, ми тут без тебе сумуємо!{Environment.NewLine}{Environment.NewLine}" +
                     $"🥇 *Містер Золото: {model.PilotWithTheMostGoldenMedal.name}.* Цей геній зібрав *{model.PilotWithTheMostGoldenMedal.count}* золотих медалей!";

        var third = $"🏆 А ось *ТОП-3* пілотів, які набрали найбільшу сумарну кількість балів за рік:{Environment.NewLine}{Environment.NewLine}";

        foreach (var pilot in model.Top3Pilots)
        {
            third += $"*{pilot.Key}* - *{pilot.Value}* балів{Environment.NewLine}";
        }

        third += $"{Environment.NewLine}Непогано, авжеж? Дякуємо, що продовжуєте літати і стаєте ще швидшими! 🚀";

        return new List<string>()
        {
            first,
            second,
            third
        };
    }

    public string DayStreakAchievement(Pilot pilot)
    {
        return pilot.DayStreak switch
        {
            10 or 20 => $"🎉 **{pilot.Name}** має вже **{pilot.DayStreak}** day streak",
            50 => $"🎉 **{pilot.Name}** досягнув **{pilot.DayStreak}** day streak",
            75 => $"🎉 **{pilot.Name}** тримає **{pilot.DayStreak}** day streak",
            100 => $"🎉 **{pilot.Name}** подолав **{pilot.DayStreak}** day streak",
            150 => $"🎉 **{pilot.Name}** перетнув **{pilot.DayStreak}** day streak",
            200 => $"🎉 **{pilot.Name}** має неймовірні **{pilot.DayStreak}** day streak",
            250 => $"🎉 **{pilot.Name}** має вже **{pilot.DayStreak}** day streak",
            300 => $"🎉 **{pilot.Name}** досягнув вражаючих **{pilot.DayStreak}** day streak",
            365 => $"🎉 **{pilot.Name}** відзначає **{pilot.DayStreak}** day streak. Цілий рік!",
            500 => $"🎉 **{pilot.Name}** подолав **{pilot.DayStreak}** day streak. Це вау!",
            1000 => $"🎉 **{pilot.Name}** має вражаючі **{pilot.DayStreak}** day streak",
            _ => string.Empty
        };
    }

    public string DayStreakPotentialLose(IEnumerable<Pilot> pilots)
    {
        var message = $"## ⚠️ УВАГА!{Environment.NewLine}" +
                      $"Загроза втрати day streak:{Environment.NewLine}{Environment.NewLine}";

        foreach (var pilot in pilots)
        {
            message += $"**{pilot.Name}** - **{pilot.DayStreak}** streak ({GetFreezieText(pilot.DayStreakFreezeCount)}){Environment.NewLine}";
        }

        message += $"{Environment.NewLine}Швиденько запускайте симулятори і летіть! 🚀" +
                   $"{Environment.NewLine}У вас менше години.";

        return message;
    }

    #region Private

    private string TimeUpdate(TrackTimeDelta delta)
    {
        var timeChangePart = delta.TimeChange.HasValue ? $" ({MsToSec(delta.TimeChange.Value)}s)" : string.Empty;
        var rankOldPart = delta.RankOld.HasValue ? $" (#{delta.RankOld})" : string.Empty;
        var modelPart = delta.ModelName is not null ? $" / {delta.ModelName}" : string.Empty;

        return $"✈️  **{delta.PlayerName}**{modelPart}{Environment.NewLine}" +
               $"⏱️  {MsToSec(delta.TrackTime)}s{timeChangePart} / #{delta.Rank}{rankOldPart}";
    }

    private List<string> TempLeaderboardRows(List<CompetitionResults> results)
    {
        var positionLength = results.Count().ToString().Length + 2;
        var pilotNameLength = results.Max(r => r.PlayerName.Length) + 2;
        var timeLength = results.Max(r => MsToSec(r.TrackTime).ToString().Length) + 3;
        var rows = new List<string>();

        foreach (var result in results)
        {
            rows.Add($"{FillWithSpaces(result.LocalRank, positionLength)}{FillWithSpaces(result.PlayerName, pilotNameLength)}{FillWithSpaces(MsToSec(result.TrackTime) + "s", timeLength)}{result.ModelName}");
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

        return $"{icon} - **{time.PlayerName}** ({MsToSec(time.TrackTime)}s) / Балів: **{time.Points}**";
    }

    private string TempSeasonResultsRow(SeasonResult result)
    {
        return $"{result.Rank} - **{result.PlayerName}** - {result.Points} балів";
    }

    private string SeasonResultsRow(SeasonResult result)
    {
        var icon = result.Rank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => $"#{result.Rank}"
        };

        return $"{icon} - **{result.PlayerName}** - {result.Points} балів";
    }

    private string? MedalCountRow(SeasonResult result)
    {
        if (result is { GoldenCount: 0, SilverCount: 0, BronzeCount: 0 })
            return null;

        var medals = $"{MedalsRow("🥇", result.GoldenCount)}{MedalsRow("🥈", result.SilverCount)}{MedalsRow("🥉", result.BronzeCount)}";
        return $"*{result.PlayerName}*:{Environment.NewLine}{medals}";
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

    private static string MsToSec(int ms) => (ms / 1000.0).ToString(CultureInfo.InvariantCulture);

    private static string GetFreezieText(int number) => number == 1 ? $"{number} freezie" : $"{number} freezies";

    #endregion
}
