using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Achievements.Base;

public interface IAchievement
{
    string Name { get; }
    string Title { get; }
    string Description { get; }
}

public interface IAchievementAfterTimeUpdate : IAchievement
{
    Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas);
}

public interface IAchievementAfterCompetition : IAchievement
{
    Task<bool> CheckAsync(Pilot pilot, Competition competition);
}

public interface IAchievementAfterSeason : IAchievement
{
    Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults);
}

public interface IGlobalAchievement : IAchievement
{
    Task CheckAsync();
}
