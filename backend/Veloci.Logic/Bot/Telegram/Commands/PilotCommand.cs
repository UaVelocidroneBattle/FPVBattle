using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Bot.Telegram.Commands.Core;
using Veloci.Logic.Services.Pilots;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class PilotCommand : ITelegramCommand
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IEnumerable<IAchievement> _achievements;
    private readonly IPilotProfileService _pilotProfileService;

    public PilotCommand(
        IRepository<Pilot> pilots,
        IServiceProvider serviceProvider,
        IPilotProfileService pilotProfileService)
    {
        _pilots = pilots;
        _pilotProfileService = pilotProfileService;
        _achievements = serviceProvider.GetServices<IAchievement>();
    }

    public string[] Keywords => ["/pilot", "/p"];
    public string Description => "`/pilot {pilotName}` або `/p {pilotName}` - Pilot's profile";
    public async Task<string> ExecuteAsync(TelegramCommandContext context)
    {
        if (context.Parameters is null || context.Parameters.Length == 0)
            return "все добре, але не вистачає імені пілота";

        var pilotName = string.Join(' ', context.Parameters);
        var pilot = await _pilots.GetAll().ByName(pilotName).FirstOrDefaultAsync();

        if (pilot is null)
            return $"Не знаю такого пілота 😕";

        var profile = await _pilotProfileService.GetPilotProfileAsync(pilot.Name, CancellationToken.None);

        var lastRaceDateText = profile.LastRaceDate.HasValue
            ? profile.LastRaceDate.Value.ToString("dd MMM yyyy")
            : "-";

        return $"👤 *{pilot.Name}*{Environment.NewLine}{Environment.NewLine}" +
               $"Last race date: *{lastRaceDateText}*{Environment.NewLine}" +
               $"Day streak: *{pilot.DayStreak}*{Environment.NewLine}" +
               $"Max day streak: *{pilot.MaxDayStreak}*{Environment.NewLine}" +
               $"Freezies: *{pilot.DayStreakFreezeCount}*{Environment.NewLine}" +
               $"Total flight days: *{profile.TotalRaceDays}*{Environment.NewLine}" +
               $"Achievements: *{pilot.Achievements?.Count ?? 0}/{_achievements.Count()}*";
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => true;
}
