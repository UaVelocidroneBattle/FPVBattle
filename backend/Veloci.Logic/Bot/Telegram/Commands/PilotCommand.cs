using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Achievements.Base;
using Veloci.Logic.Bot.Telegram.Commands.Core;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class PilotCommand : ITelegramCommand
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<Competition> _competitions;
    private readonly IEnumerable<IAchievement> _achievements;

    public PilotCommand(
        IRepository<Pilot> pilots,
        IRepository<Competition> competitions,
        IServiceProvider serviceProvider)
    {
        _pilots = pilots;
        _competitions = competitions;
        _achievements = serviceProvider.GetServices<IAchievement>();
    }

    public string[] Keywords => ["/pilot", "/p"];
    public string Description => "`/pilot {pilotName}` або `/p {pilotName}` - Pilot's profile";
    public async Task<string> ExecuteAsync(string[]? parameters)
    {
        if (parameters is null || parameters.Length == 0)
            return "все добре, але не вистачає імені пілота";

        var pilotName = string.Join(' ', parameters);
        var pilot = await _pilots.FindAsync(pilotName);

        if (pilot is null)
            return $"Не знаю такого пілота 😕";

        var totalFlightDays = await _competitions
            .GetAll()
            .NotCancelled()
            .Where(comp => comp.CompetitionResults.Any(res => res.PlayerName == pilotName))
            .CountAsync();

        var lastRaceDateText = pilot.LastRaceDate.HasValue
            ? pilot.LastRaceDate.Value.ToString("dd MMM yyyy")
            : "-";

        return $"👤 *{pilot.Name}*{Environment.NewLine}{Environment.NewLine}" +
               $"Last race date: *{lastRaceDateText}*{Environment.NewLine}" +
               $"Day streak: *{pilot.DayStreak}*{Environment.NewLine}" +
               $"Max day streak: *{pilot.MaxDayStreak}*{Environment.NewLine}" +
               $"Freezies: *{pilot.DayStreakFreezeCount}*{Environment.NewLine}" +
               $"Total flight days: *{totalFlightDays}*{Environment.NewLine}" +
               $"Achievements: *{pilot.Achievements?.Count ?? 0}/{_achievements.Count()}*";
    }

    public bool RemoveMessageAfterDelay => false;
}
