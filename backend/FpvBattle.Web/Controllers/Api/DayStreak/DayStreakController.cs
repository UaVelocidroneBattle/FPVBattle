using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Web.Controllers.Api.DayStreak;

[ApiController]
[Route("/api/daystreak/[action]")]
public class DayStreakController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public DayStreakController(IApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<List<DayStreakLeaderboardRow>> Leaderboard()
    {
        return await _db.Pilots.Where(p => p.DayStreak > 0)
            .OrderByDescending(l => l.DayStreak)
            .Select(p => new DayStreakLeaderboardRow
            {
                PilotName = p.Name,
                DayStreak = p.DayStreak,
                MaxStreak = p.MaxDayStreak
            })
            .ToListAsync();
    }
}
