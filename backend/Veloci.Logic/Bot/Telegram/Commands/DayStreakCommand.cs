using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Bot.Telegram.Commands.Core;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class DayStreakCommand : ITelegramCommand
{
    private readonly IRepository<Pilot> _pilots;

    public DayStreakCommand(IRepository<Pilot> pilots)
    {
        _pilots = pilots;
    }

    public string[] Keywords => ["/day-streak", "/ds"];
    public string Description => "`/day-streak {pilotName}` або `/ds {pilotName}` - Your day streak statistics";
    public async Task<string> ExecuteAsync(string[]? parameters)
    {
        if (parameters is null || parameters.Length == 0)
            return "все добре, але не вистачає імені пілота";

        var pilotName = string.Join(' ', parameters);
        var pilot = await _pilots.FindAsync(pilotName);

        if (pilot is null)
            return $"Не знаю такого пілота 😕";

        return $"Day streak: *{pilot.DayStreak}*{Environment.NewLine}" +
               $"Max day streak: *{pilot.MaxDayStreak}*{Environment.NewLine}" +
               $"Freezies: *{pilot.DayStreakFreezeCount}*";
    }

    public bool RemoveMessageAfterDelay => false;
}
