using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Services;
using ILogger = Serilog.ILogger;

namespace Veloci.Web.Controllers;

[ApiController]
[Route("/api/migration/[action]")]
public class MigrationController : ControllerBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<MigrationController>();
    private readonly IRepository<PilotAchievement> _achievements;

    public MigrationController(IRepository<PilotAchievement> achievements)
    {
        _achievements = achievements;
    }

    [HttpGet]
    public async Task RecalibrateEarlyLateAchievements()
    {
        var hourRanges = new Dictionary<string, (int From, int To)>
        {
            ["EarlyBird"] = (4, 8),
            ["LateBird"] = (1, 4),
        };

        var achievementNames = hourRanges.Keys.ToArray();

        var achievements = await _achievements
            .GetAll()
            .Include(a => a.Pilot)
            .Where(a => achievementNames.Contains(a.Name) && a.Pilot.Country != "UA")
            .ToListAsync();

        Log.Information("Recalibrating {Count} early/late achievements for non-UA pilots", achievements.Count);

        var removed = 0;

        foreach (var achievement in achievements)
        {
            var (hourFrom, hourTo) = hourRanges[achievement.Name];
            var localTime = TimeZoneService.GetLocalTime(achievement.Date, achievement.Pilot.Country);
            var isValid = localTime.Hour >= hourFrom && localTime.Hour < hourTo;

            if (!isValid)
            {
                Log.Information("Removing {Achievement} from {Pilot} ({Country}): local time was {LocalTime}",
                    achievement.Name, achievement.Pilot.Name, achievement.Pilot.Country, localTime);
                await _achievements.RemoveAsync(achievement.Id);
                removed++;
            }
        }

        Log.Information("Recalibration complete. Removed {Removed} of {Total} achievements", removed, achievements.Count);
    }
}
