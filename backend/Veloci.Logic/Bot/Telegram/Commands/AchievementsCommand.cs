using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Bot.Telegram.Commands.Core;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class AchievementsCommand : ITelegramCommand
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IEnumerable<IAchievement> _achievements;

    public AchievementsCommand(IRepository<Pilot> pilots, IServiceProvider serviceProvider)
    {
        _pilots = pilots;
        _achievements = serviceProvider.GetServices<IAchievement>();
    }

    public string[] Keywords => ["/achievements", "/ach"];
    public string Description => "`/achievements {pilotName}` або `/ach {pilotName}` - Your achievements";
    public async Task<string> ExecuteAsync(string[]? parameters)
    {
        if (parameters is null || parameters.Length == 0)
            return "все добре, але не вистачає імені пілота";

        var pilotName = string.Join(' ', parameters);
        var pilot = await _pilots.GetAll().ByName(pilotName).FirstOrDefaultAsync();

        if (pilot is null)
            return "Не знаю такого пілота 😕";

        return $"Твої досягнення ({pilot.Achievements.Count}/{_achievements.Count()}):{Environment.NewLine}{Environment.NewLine}" +
               $"{string.Join(Environment.NewLine, _achievements.Select(a => AchievementRow(a, pilot)))}";
    }

    private string AchievementRow(IAchievement achievement, Pilot pilot)
    {
        var icon = pilot.Achievements.Any(a => a.Name == achievement.Name)
            ? "☑️"
            : "◻️";

        return $"️{icon} *{achievement.Title}* ({achievement.Description})";
    }

    public bool RemoveMessageAfterDelay => false;
}
