using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Web.Controllers.DayStreak;

[ApiController]
[Route("/api/daystreak/[action]")]
public class DayStreakController : ControllerBase
{
    private readonly IRepository<Pilot> _pilots;

    public DayStreakController(IRepository<Pilot> pilots)
    {
        _pilots = pilots;
    }

    [HttpGet]
    public async Task<List<DayStreakLeaderboardRow>> Leaderboard()
    {
        return await _pilots.GetAll(p => p.DayStreak > 0)
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
