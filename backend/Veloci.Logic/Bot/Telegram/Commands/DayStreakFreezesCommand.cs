using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Bot.Telegram.Commands.Core;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class DayStreakFreezesCommand : ITelegramCommand
{
    private readonly IRepository<Pilot> _pilots;

    public DayStreakFreezesCommand(IRepository<Pilot> pilots)
    {
        _pilots = pilots;
    }

    public string[] Keywords => ["/day-streak-freezes", "/dsf"];
    public string Description => "`/day-streak-freezes {pilotName}` або `/dsf {pilotName}` - Кількість заморозок у пілота";
    public async Task<string> ExecuteAsync(string[]? parameters)
    {
        if (parameters is null || parameters.Length == 0)
            return "все добре, але не вистачає імені пілота";

        var pilotName = string.Join(' ', parameters);
        var pilot = await _pilots.FindAsync(pilotName);

        return pilot is null
            ? $"Не знаю такого пілота 😕"
            : $"Заморозок: {pilot.DayStreakFreezes}";
    }

    public bool RemoveMessageAfterDelay => false;
}
