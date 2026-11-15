using FpvBattle.Application.Features.Achievements.Base;
using FpvBattle.Application.Persistence;
using FpvBattle.Application.Persistence.Queries;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Features.Achievements.Collection;

/// <summary>
/// Base for achievements awarded to pilots who raced in every competition of a cup during the season.
/// </summary>
public abstract class HaunterAchievementBase(IApplicationDbContext db) : IAchievementAfterSeason
{
    public abstract string Name { get; }
    public abstract string Title { get; }
    public abstract string Description { get; }
    public abstract string? CupId { get; }

    public async Task<bool> CheckAsync(Pilot pilot, List<LeagueSeasonLeaderboard> seasonLeaderboards)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        // Season is the month that has just ended, see CompetitionConductor.StopSeasonAsync
        var seasonEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var seasonStart = seasonEnd.AddMonths(-1);

        var seasonCompetitions = db.Competitions
            .InRange(seasonStart, seasonEnd)
            .ForCup(CupId!)
            .Closed();

        var totalDays = await seasonCompetitions.CountAsync();
        var flownDays = await seasonCompetitions.ForPilot(pilot.Id).CountAsync();

        return totalDays > 0 && flownDays == totalDays;
    }
}
