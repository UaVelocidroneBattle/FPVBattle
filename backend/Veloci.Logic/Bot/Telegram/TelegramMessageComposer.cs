using System.Text;
using Veloci.Data.Domain;
using Veloci.Logic.Helpers;
using Veloci.Logic.Services.Statistics;
using Veloci.Logic.Services.Statistics.YearResults;

namespace Veloci.Logic.Bot.Telegram;

public class TelegramMessageComposer
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
            rating = $"Попередній рейтинг: *{Math.Round(track.Rating.Value.Value, 1):F1}*/3{Environment.NewLine}{Environment.NewLine}";
        }

        var flownPilotsText = pilotsFlownOnTrack.Count != 0 ?
            $"Трек вже літали:{Environment.NewLine}*{string.Join(", ", pilotsFlownOnTrack)}*{Environment.NewLine}" :
            $"Трек ще ніхто з вас не літав.{Environment.NewLine}";

        var quadOfTheDayText = quadOfTheDay is null
            ? string.Empty
            : $"⚠️ Квад дня: *{quadOfTheDay}*{Environment.NewLine}{Environment.NewLine}";

        return $"📅 Вітаємо на *FPV Battle*!{Environment.NewLine}{Environment.NewLine}" +
               $"Трек дня:{Environment.NewLine}" +
               $"*{track.Map.Name} - `{track.Name}`*{Environment.NewLine}{Environment.NewLine}" +
               $"{rating}" +
               $"{quadOfTheDayText}" +
               $"Leaderboard:{Environment.NewLine}" +
               $"*https://www.velocidrone.com/leaderboard/{track.Map.MapId}/{track.TrackId}/All*{Environment.NewLine}{Environment.NewLine}" +
               $"{flownPilotsText}{Environment.NewLine}" +
               $"👾 Інструкція, статистика і інше тут:{Environment.NewLine}*https://ua-velocidrone.fun/*{Environment.NewLine}";
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

    public string TempLeaderboard(List<CompetitionResults> results, Track track, string? quadOfTheDay)
    {
        var quadOfTheDayText = quadOfTheDay is null
            ? string.Empty
            : $"⚠️ Квад дня: *{quadOfTheDay}*{Environment.NewLine}{Environment.NewLine}";

        var rows = TempLeaderboardRows(results);
        return $"🧐 Проміжні результати:{Environment.NewLine}{Environment.NewLine}" +
               $"*{track.Map.Name} - `{track.Name}`*{Environment.NewLine}{Environment.NewLine}" +
               quadOfTheDayText +
               $"`{string.Join($"{Environment.NewLine}", rows)}`";
    }

    public string Leaderboard(IEnumerable<CompetitionResults> results, string trackName)
    {
        var rows = results.Select(LeaderboardRow);
        var divider = Environment.NewLine;
        return $"🏆 Результати дня{Environment.NewLine}" +
               $"Трек: *{trackName}*{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{divider}", rows)}" +
               $"{Environment.NewLine}{Environment.NewLine}#dayresults";
    }

    public string TempSeasonResults(IEnumerable<SeasonResult> results)
    {
        var rows = results.Select(TempSeasonResultsRow);
        var divider = Environment.NewLine;
        return $"🗓 Проміжні результати місяця{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{divider}", rows)}";
    }

    public string SeasonResults(IEnumerable<SeasonResult> results)
    {
        var rows = results.Select(SeasonResultsRow);
        return $"🏁 Фінальні результати місяця{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{Environment.NewLine}{Environment.NewLine}", rows)}";
    }

    public string MedalCount(IEnumerable<SeasonResult> results, bool includeExtraNewLine = true)
    {
        var rows = results
            .Select(MedalCountRow)
            .Where(row => row is not null);

        var divider = includeExtraNewLine ? $"{Environment.NewLine}{Environment.NewLine}" : Environment.NewLine;

        return $"*Медалі за місяць*{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{divider}", rows)}";
    }

    public IEnumerable<string> YearResults(YearResultsModel model)
    {
        var first = $"🎉 *FPV Battle WRAPPED 📈 {model.Year}*{Environment.NewLine}" +
               $"або трохи цифр за минулий рік{Environment.NewLine}{Environment.NewLine}" +
               $"📊 *{model.TotalTrackCount} треків!* Це стільки ми пролетіли минулого року.{Environment.NewLine}" +
               $"Із них унікальних - *{model.UniqueTrackCount}*. Так, деякі треки повторювались, але такі вже у нас алгоритми.{Environment.NewLine}" +
               $"З іншого боку, це гарний привід обігнати самого себе і подивитись на свій прогрес.{Environment.NewLine}{Environment.NewLine}" +
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


    public string DayStreakPotentialLose(IEnumerable<Pilot> pilots)
    {
        var message = $"⚠️ *УВАГА!*{Environment.NewLine}" +
                      $"Загроза втрати day streak:{Environment.NewLine}{Environment.NewLine}";

        foreach (var pilot in pilots)
        {
            message += $"*{TextHelper.Trim(pilot.Name, PilotNameMaxLength)}* - *{pilot.DayStreak}* streak ({GetFreezieText(pilot.DayStreakFreezeCount)}){Environment.NewLine}";
        }

        message += $"{Environment.NewLine}Швиденько запускайте симулятори і летіть, у вас ще 2 години!";

        return message;
    }

    public string NewPilot(Pilot pilot)
    {
        return $"🎉 Вітаємо нового пілота {TextHelper.CountryFlagWithSpace(pilot.Country)}*{pilot.Name}*";
    }

    public string PilotRenamed(string oldName, string newName)
    {
        return $"✏️ Пілот *{oldName}* перейменувався на *{newName}*";
    }

    public string EndOfSeasonStatistics(EndOfSeasonStatisticsDto statistics)
    {
        return $"📊 *Трохи статистики за сезон {statistics.SeasonName}*{Environment.NewLine}{Environment.NewLine}" +
               $"▪️ Середня кількість пілотів за день: *{statistics.AveragePilotsLastMonth}*{Environment.NewLine}" +
               $"▪️ Середня кількість пілотів за день (за останні 12 місяців): *{statistics.AveragePilotsLastYear}*{Environment.NewLine}" +
               $"▪️ Найбільша кількість пілотів за день: *{statistics.MaxPilotsLastMonth}*{Environment.NewLine}" +
               $"▪️ Найменша кількість пілотів за день: *{statistics.MinPilotsLastMonth}*{Environment.NewLine}{Environment.NewLine}" +
               $"#endOfSeasonStatistics{Environment.NewLine}";
    }

    public string FreezieAdded(string pilotName)
    {
        return $"❄️ *{pilotName}* отримав додатковий freezie";
    }

    public string RestartTrack()
    {
        return "🔁️ Усі прибрали руки від контролерів, ми *міняємо трек*";
    }

    #region Private

    private string TimeUpdate(TrackTimeDelta delta)
    {
        var timeChangePart = delta.TimeChange.HasValue ? $" ({TrackTimeConverter.MsToSec(delta.TimeChange.Value)}s)" : string.Empty;
        var rankOldPart = delta.RankOld.HasValue ? $" (#{delta.RankOld})" : string.Empty;
        var modelPart = delta.ModelName is not null ? $" / {delta.ModelName}" : string.Empty;
        var flag = TextHelper.CountryFlagWithSpace(delta.Country);

        return $"{flag}*{TextHelper.Trim(delta.Pilot.Name, PilotNameMaxLength)}*{modelPart}{Environment.NewLine}" +
               $"⏱️ {TrackTimeConverter.MsToSec(delta.TrackTime)}s{timeChangePart} / #{delta.Rank}{rankOldPart}";
    }

    private List<string> TempLeaderboardRows(List<CompetitionResults> results)
    {
        var positionLength = results.Count.ToString().Length + 2;
        var pilotNameLength = Math.Min(results.Max(r => r.Pilot.Name.Length), PilotNameMaxLength) + 2;
        var rows = new List<string>();

        foreach (var result in results)
        {
            var pilotName = TextHelper.Trim(result.Pilot.Name, PilotNameMaxLength);
            rows.Add($"{FillWithSpaces(result.LocalRank, positionLength)}{FillWithSpaces(pilotName, pilotNameLength)}{TrackTimeConverter.MsToSec(result.TrackTime)}s");
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

        var points = $"Балів: *{time.Points}*";

        if (time.BonusPoints > 0)
            points += $" +*{time.BonusPoints}*";

        return $"{icon} - *{TextHelper.Trim(time.Pilot.Name, PilotNameMaxLength)}* ({TrackTimeConverter.MsToSec(time.TrackTime)}s) / {points}";
    }

    private string TempSeasonResultsRow(SeasonResult result)
    {
        return $"{result.Rank} - *{TextHelper.Trim(result.PlayerName, PilotNameMaxLength)}* - {result.Points} балів";
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

        return $"{icon} - *{TextHelper.Trim(result.PlayerName, PilotNameMaxLength)}* - {result.Points} балів";
    }

    private string? MedalCountRow(SeasonResult result)
    {
        if (result is { GoldenCount: 0, SilverCount: 0, BronzeCount: 0 })
            return null;

        var medals = $"{MedalsRow("🥇", result.GoldenCount)}{MedalsRow("🥈", result.SilverCount)}{MedalsRow("🥉", result.BronzeCount)}";
        return $"*{TextHelper.Trim(result.PlayerName, PilotNameMaxLength)}*:{Environment.NewLine}{medals}";
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
