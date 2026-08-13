using System.Text;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Leagues.Models;
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

    public string StartCompetition(Track track, string? quadOfTheDay)
    {
        var rating = string.Empty;

        if (track.Rating?.Value is not null)
        {
            rating = $"Попередній рейтинг: *{Math.Round(track.Rating.Value.Value, 1):F1}*{Environment.NewLine}{Environment.NewLine}";
        }

        return $"📅 Вітаємо на *FPV Battle*!{Environment.NewLine}{Environment.NewLine}" +
               $"Трек дня:{Environment.NewLine}" +
               $"{TrackHeading(track)}{Environment.NewLine}{Environment.NewLine}" +
               $"{rating}" +
               $"{QuadOfTheDayText(quadOfTheDay)}" +
               $"Leaderboard:{Environment.NewLine}" +
               $"*https://www.velocidrone.com/leaderboard/{track.Map.MapId}/{track.TrackId}/All*{Environment.NewLine}{Environment.NewLine}" +
               $"👾 Інструкція, статистика і інше тут:{Environment.NewLine}*https://fpv-battle.fun/*{Environment.NewLine}";
    }

    public string BadTrackRating()
    {
        return "😔 Бачу трек не сподобався. Більше його не буде";
    }

    public string TempLeaderboard(List<LeagueLeaderboard> leaderboard, Track track, string? quadOfTheDay)
    {
        var showHeaders = leaderboard.Count > 1;

        var sections = leaderboard.Select(l =>
        {
            var header = showHeaders ? $"*{l.League.ToUpper()}*{Environment.NewLine}{Environment.NewLine}" : string.Empty;
            var rows = TempLeaderboardRows(l.Results);
            return $"{header}`{TelegramMarkdown.EscapeCodeText(string.Join(Environment.NewLine, rows))}`";
        });

        return $"🧐 Проміжні результати:{Environment.NewLine}{Environment.NewLine}" +
               $"{TrackHeading(track)}{Environment.NewLine}{Environment.NewLine}" +
               QuadOfTheDayText(quadOfTheDay) +
               string.Join($"{Environment.NewLine}{Environment.NewLine}", sections);
    }

    public string Leaderboard(List<LeagueLeaderboard> leaderboard, string trackName)
    {
        var showHeaders = leaderboard.Count > 1;

        var sections = leaderboard.Select(l =>
        {
            var header = showHeaders ? $"*{l.League!.ToUpper()}*{Environment.NewLine}{Environment.NewLine}" : string.Empty;
            var rows = l.Results.Any() ? string.Join(Environment.NewLine, l.Results.Select(LeaderboardRow)) : "поки нікого";
            return $"{header}{rows}";
        });

        return $"🏆 Результати дня{Environment.NewLine}" +
               $"Трек: *{TelegramMarkdown.EscapeUserText(trackName)}*{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{Environment.NewLine}{Environment.NewLine}", sections)}";
    }

    public string TempSeasonResults(List<LeagueSeasonLeaderboard> leaderboard)
    {
        var showHeaders = leaderboard.Count > 1;

        var sections = leaderboard.Select(l =>
        {
            var header = showHeaders ? $"*{l.League!.ToUpper()}*{Environment.NewLine}{Environment.NewLine}" : string.Empty;
            var rows = l.Results.Select(TempSeasonResultsRow);
            return $"{header}{string.Join(Environment.NewLine, rows)}";
        });

        return $"🗓 Проміжні результати місяця{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{Environment.NewLine}{Environment.NewLine}", sections)}";
    }

    public string SeasonResults(List<LeagueSeasonLeaderboard> leaderboard)
    {
        var showHeaders = leaderboard.Count > 1;

        var sections = leaderboard.Select(l =>
        {
            var header = showHeaders ? $"*{l.League!.ToUpper()}*{Environment.NewLine}{Environment.NewLine}" : string.Empty;
            var rows = l.Results.Select(SeasonResultsRow);
            return $"{header}{string.Join($"{Environment.NewLine}", rows)}";
        });

        return $"🏁 Фінальні результати місяця{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join($"{Environment.NewLine}{Environment.NewLine}", sections)}";
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
               $"*{TelegramMarkdown.EscapeUserText(model.FavoriteTrack)}*{Environment.NewLine}" +
               $"Це переможець за вашими голосами!";

        var mostFrequentPilot = TelegramMarkdown.EscapeUserText(model.PilotWhoCameTheMost.name);
        var rarestPilot = TelegramMarkdown.EscapeUserText(model.PilotWhoCameTheLeast.name);
        var goldenPilot = TelegramMarkdown.EscapeUserText(model.PilotWithTheMostGoldenMedal.name);

        var second = $"👥 В минулому році тут з'являлись імена *{model.TotalPilotCount}* пілотів.{Environment.NewLine}{Environment.NewLine}" +
                     $"🥷 *Чемпіон відвідувань: {mostFrequentPilot}.* Цей відчайдух пролетів *{model.PilotWhoCameTheMost.count} треків* за рік!{Environment.NewLine}" +
                     $"{mostFrequentPilot}, ти точно людина? 🤖{Environment.NewLine}{Environment.NewLine}" +
                     $"🧐 *Приз за рідкісні появи: {rarestPilot}* Він з'явився всього {model.PilotWhoCameTheLeast.count} {UkrainianHelper.GetTimesString(model.PilotWhoCameTheLeast.count)}.{Environment.NewLine}" +
                     $"{rarestPilot}, ми тут без тебе сумуємо!{Environment.NewLine}{Environment.NewLine}" +
                     $"🥇 *Містер Золото: {goldenPilot}.* Цей геній зібрав *{model.PilotWithTheMostGoldenMedal.count}* золотих медалей!";

        var third = $"🏆 А ось *ТОП-3* пілотів, які набрали найбільшу сумарну кількість балів за рік:{Environment.NewLine}{Environment.NewLine}";

        foreach (var pilot in model.Top3Pilots)
        {
            third += $"*{TelegramMarkdown.EscapeUserText(pilot.Key)}* - *{pilot.Value}* балів{Environment.NewLine}";
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
            message += $"*{PilotName(pilot.Name)}* - *{pilot.DayStreak}* streak ({GetFreezieText(pilot.DayStreakFreezeCount)}){Environment.NewLine}";
        }

        message += $"{Environment.NewLine}Швиденько запускайте симулятори і летіть, у вас ще 2 години!";

        return message;
    }

    public string NewPilot(Pilot pilot)
    {
        return $"🎉 Вітаємо нового пілота {TextHelper.CountryFlagWithSpace(pilot.Country)}*{TelegramMarkdown.EscapeUserText(pilot.Name)}*";
    }

    public string PilotRenamed(string oldName, string newName)
    {
        return $"✏️ Пілот *{TelegramMarkdown.EscapeUserText(oldName)}* перейменувався на *{TelegramMarkdown.EscapeUserText(newName)}*";
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
        return $"❄️ *{TelegramMarkdown.EscapeUserText(pilotName)}* отримав додатковий freezie";
    }

    public string RestartTrack()
    {
        return "🔁️ Усі прибрали руки від контролерів, ми *міняємо трек*";
    }

    #region Private

    /// <summary>
    /// Trims a pilot name to display length and escapes it, so that a name containing markup
    /// characters cannot break the message it is placed into.
    /// </summary>
    private static string PilotName(string name) =>
        TelegramMarkdown.EscapeUserText(TextHelper.Trim(name, PilotNameMaxLength));

    /// <summary>
    /// Formats a track as a bold "map - `track`" heading, with both names escaped.
    /// </summary>
    private static string TrackHeading(Track track) =>
        $"*{TelegramMarkdown.EscapeUserText(track.Map.Name)} - `{TelegramMarkdown.EscapeCodeText(track.Name)}`*";

    private static string QuadOfTheDayText(string? quadOfTheDay) =>
        quadOfTheDay is null
            ? string.Empty
            : $"⚠️ Квад дня: *{TelegramMarkdown.EscapeUserText(quadOfTheDay)}*{Environment.NewLine}{Environment.NewLine}";

    private string TimeUpdate(TrackTimeDelta delta)
    {
        var timeChangePart = delta.TimeChange.HasValue ? $" ({TrackTimeConverter.MsToSec(delta.TimeChange.Value)}s)" : string.Empty;
        var rankOldPart = delta.RankOld.HasValue ? $" (#{delta.RankOld})" : string.Empty;
        var modelPart = delta.ModelName is not null ? $" / {TelegramMarkdown.EscapeUserText(delta.ModelName)}" : string.Empty;
        var flag = TextHelper.CountryFlagWithSpace(delta.Country);

        return $"{flag}*{PilotName(delta.Pilot.Name)}*{modelPart}{Environment.NewLine}" +
               $"⏱️ {TrackTimeConverter.MsToSec(delta.TrackTime)}s{timeChangePart} / #{delta.Rank}{rankOldPart}";
    }

    private List<string> TempLeaderboardRows(List<CompetitionResults> results)
    {
        if (results.Count == 0)
            return ["поки нікого"];

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

        return $"{icon} - *{PilotName(time.Pilot.Name)}* ({TrackTimeConverter.MsToSec(time.TrackTime)}s) / {points}";
    }

    private string TempSeasonResultsRow(SeasonResult result)
    {
        return $"{result.Rank} - *{PilotName(result.PlayerName)}* - {result.Points} балів";
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

        return $"{icon} - *{PilotName(result.PlayerName)}* - {result.Points} балів";
    }

    private static string GetFreezieText(int number) => number == 1 ? $"{number} freezie" : $"{number} freezies";

    public string LeagueUpdates(IList<LeagueUpdateModel> updates)
    {
        var sb = new StringBuilder($"🏆 *League updates:*{Environment.NewLine}{Environment.NewLine}");

        foreach (var update in updates)
        {
            var line = update switch
            {
                { OldLeague: null } => $"▫️ {TelegramMarkdown.EscapeUserText(update.PilotName)} → *{update.NewLeague?.ToUpper()}*",
                { NewLeague: null } => $"▫️ {TelegramMarkdown.EscapeUserText(update.PilotName)} покидає *{update.OldLeague?.ToUpper()}*",
                _ => $"▫️ {TelegramMarkdown.EscapeUserText(update.PilotName)} *{update.OldLeague?.ToUpper()}* → *{update.NewLeague?.ToUpper()}*"
            };

            sb.AppendLine(line);
        }

        return sb.ToString().TrimEnd();
    }

    #endregion
}
