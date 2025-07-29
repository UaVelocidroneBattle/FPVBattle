using Veloci.Data.Domain;
using Veloci.Logic.Achievements.Base;

namespace Veloci.Logic.Achievements.Collection;

public abstract class DayStreakAchievementBase : IAchievementAfterCompetition
{
    public string Name => $"DayStreak{Days}";
    public abstract string Title { get; }
    public string Description => $"{Days} day streak";

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return pilot.MaxDayStreak >= Days;
    }

    protected abstract int Days { get; }
}

public class DayStreak10Achievement : DayStreakAchievementBase
{
    public override string Title => "Початок";
    protected override int Days => 10;
}

public class DayStreak20Achievement : DayStreakAchievementBase
{
    public override string Title => "Легка залежність";
    protected override int Days => 20;
}

public class DayStreak50Achievement : DayStreakAchievementBase
{
    public override string Title => "Півсотня";
    protected override int Days => 50;
}

public class DayStreak75Achievement : DayStreakAchievementBase
{
    public override string Title => "Фанатик";
    protected override int Days => 75;
}

public class DayStreak100Achievement : DayStreakAchievementBase
{
    public override string Title => "Сотня";
    protected override int Days => 100;
}

public class DayStreak150Achievement : DayStreakAchievementBase
{
    public override string Title => "Залежний";
    protected override int Days => 150;
}

public class DayStreak200Achievement : DayStreakAchievementBase
{
    public override string Title => "Кардінал";
    protected override int Days => 200;
}

public class DayStreak250Achievement : DayStreakAchievementBase
{
    public override string Title => "Потужний";
    protected override int Days => 250;
}

public class DayStreak300Achievement : DayStreakAchievementBase
{
    public override string Title => "Тракторист";
    protected override int Days => 300;
}

public class DayStreak365Achievement : DayStreakAchievementBase
{
    public override string Title => "Хард рік";
    protected override int Days => 365;
}

public class DayStreak500Achievement : DayStreakAchievementBase
{
    public override string Title => "Видатний";
    protected override int Days => 500;
}

public class DayStreak1000Achievement : DayStreakAchievementBase
{
    public override string Title => "Жива легенда";
    protected override int Days => 1000;
}
