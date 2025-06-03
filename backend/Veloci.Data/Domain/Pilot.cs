using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Achievements.Base;

namespace Veloci.Data.Domain;

public class Pilot
{
    public Pilot()
    {
    }

    public Pilot(string name)
    {
        Name = name;
    }

    [Key]
    [MaxLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The day when the pilot last raced.
    /// </summary>
    public DateTime? LastRaceDate { get; set; }
    public int DayStreak { get; set; }
    public int MaxDayStreak { get; set; }
    public int DayStreakFreezes { get; set; }
    public virtual ICollection<PilotAchievement> Achievements { get; set; }

    public void IncreaseDayStreak(DateTime today)
    {
        if (LastRaceDate.HasValue && LastRaceDate.Value.Date == today.Date)
            return;

        DayStreak++;

        if (DayStreak > MaxDayStreak)
            MaxDayStreak = DayStreak;

        LastRaceDate = today;

        // Every 30 days, the pilot gets a day streak freeze
        if (DayStreak % 30 == 0)
        {
            DayStreakFreezes++;
        }
    }

    public void ResetDayStreak()
    {
        if (SpendFreeze())
            return;

        DayStreak = 0;
    }

    private bool SpendFreeze()
    {
        if (DayStreakFreezes <= 0)
            return false;

        DayStreakFreezes--;
        return true;
    }


    public bool HasAchievement(string achievementName)
    {
        return Achievements.Any(achievement => achievement.Name == achievementName);
    }

    public void AddAchievement(IAchievement achievement)
    {
        var pilotAchievement = new PilotAchievement
        {
            Pilot = this,
            Date = DateTime.Now,
            Name = achievement.Name
        };

        Achievements.Add(pilotAchievement);
    }
}

public static class PilotExtensions
{
    public static async Task ResetDayStreaksAsync(this IQueryable<Pilot> allPilots, DateTime today)
    {
        await allPilots
            .Where(p => p.LastRaceDate < today)
            .ForEachAsync(pilot => pilot.ResetDayStreak());
    }
}
