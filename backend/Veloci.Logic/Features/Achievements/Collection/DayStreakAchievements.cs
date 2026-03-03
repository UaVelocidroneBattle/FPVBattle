using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection;

public abstract class DayStreakAchievementBase : IAchievementAfterCompetition
{
    public string Name => $"DayStreak{Days}";
    public abstract string Title { get; }
    public string Description => $"{Days} day streak";
    public string? CupId => null;

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return pilot.MaxDayStreak >= Days;
    }

    protected abstract int Days { get; }
}

public class DayStreak10Achievement : DayStreakAchievementBase
{
    public override string Title => "The Beginning";
    protected override int Days => 10;
}

public class DayStreak20Achievement : DayStreakAchievementBase
{
    public override string Title => "Mild Addiction";
    protected override int Days => 20;
}

public class DayStreak50Achievement : DayStreakAchievementBase
{
    public override string Title => "Fifty";
    protected override int Days => 50;
}

public class DayStreak75Achievement : DayStreakAchievementBase
{
    public override string Title => "Fanatic";
    protected override int Days => 75;
}

public class DayStreak100Achievement : DayStreakAchievementBase
{
    public override string Title => "Fifty-Fifty";
    protected override int Days => 100;
}

public class DayStreak150Achievement : DayStreakAchievementBase
{
    public override string Title => "Addicted";
    protected override int Days => 150;
}

public class DayStreak200Achievement : DayStreakAchievementBase
{
    public override string Title => "Cardinal";
    protected override int Days => 200;
}

public class DayStreak250Achievement : DayStreakAchievementBase
{
    public override string Title => "Consistency";
    protected override int Days => 250;
}

public class DayStreak300Achievement : DayStreakAchievementBase
{
    public override string Title => "Tractor Driver";
    protected override int Days => 300;
}

public class DayStreak365Achievement : DayStreakAchievementBase
{
    public override string Title => "Year";
    protected override int Days => 365;
}

public class DayStreak500Achievement : DayStreakAchievementBase
{
    public override string Title => "Outstanding";
    protected override int Days => 500;
}

public class DayStreak1000Achievement : DayStreakAchievementBase
{
    public override string Title => "Living Legend";
    protected override int Days => 1000;
}
