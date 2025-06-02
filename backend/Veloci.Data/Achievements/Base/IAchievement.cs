using Veloci.Data.Domain;

namespace Veloci.Data.Achievements.Base;

public interface IAchievement
{
    string Name { get; }
    string Title { get; }
}

public interface IAchievementPilotCheck : IAchievement
{
    Task<bool> CheckAsync(Pilot pilot);
}

public interface IAchievementSelfCheck : IAchievement
{
    Task CheckAsync();
}
