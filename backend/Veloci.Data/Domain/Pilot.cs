using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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

    public int? Id { get; set; }

    /// <summary>
    /// The day when the pilot last raced.
    /// </summary>
    public DateTime? LastRaceDate { get; set; }
    public int DayStreak { get; set; }
    public int MaxDayStreak { get; set; }
    public virtual ICollection<PilotAchievement> Achievements { get; set; }
    public virtual ICollection<DayStreakFreeze> DayStreakFreezes { get; set; }
    public int DayStreakFreezeCount => DayStreakFreezes.Count(fr => fr.SpentOn == null);

    /// <summary>
    /// Called when competition is finished and pilot took place in it.
    /// </summary>
    /// <param name="today">Date of the competition to record</param>
    public void OnRaceFlown(DateTime today)
    {
        if (LastRaceDate.HasValue && LastRaceDate.Value.Date == today.Date)
            return;

        LastRaceDate = today;

        IncrementDayStreak();

        AddFreezie(today);
    }

    private void IncrementDayStreak()
    {
        DayStreak++;

        if (DayStreak > MaxDayStreak)
            MaxDayStreak = DayStreak;
    }

    private void AddFreezie(DateTime today)
    {
        // Every 30 days, the pilot gets a day streak freeze
        if (DayStreak % 30 == 0)
        {
            DayStreakFreezes.Add(new DayStreakFreeze(today));
        }
    }

    /// <summary>
    /// Resets pilot's daystreak.
    /// If there are available freezes, spends a freezie.
    /// If freezie has already been spent on this day, just skip it.
    /// </summary>
    /// <param name="today"></param>
    public void ResetDayStreak(DateTime today)
    {
        if (SpendFreeze(today))
            return;

        DayStreak = 0;
    }

    private bool SpendFreeze(DateTime today)
    {
        if (HasFreezieSpentOn(today)) return true;

        var freeze = GetFirstSpareFreezie();

        if (freeze == null) return false;

        freeze.SpentOn = today;

        return true;
    }

    private DayStreakFreeze? GetFirstSpareFreezie()
    {
        return DayStreakFreezes
            .OrderBy(fr => fr.CreatedOn)
            .FirstOrDefault(fr => fr.SpentOn == null);
    }

    private bool HasFreezieSpentOn(DateTime today)
    {
        return DayStreakFreezes.Any(fr => fr.SpentOn.HasValue && fr.SpentOn.Value.Date == today.Date);
    }


    public bool HasAchievement(string achievementName)
    {
        Achievements ??= new List<PilotAchievement>();
        return Achievements.Any(achievement => achievement.Name == achievementName);
    }
}

public static class PilotExtensions
{
    public static async Task ResetDayStreaksAsync(this IQueryable<Pilot> allPilots, DateTime today)
    {
        await allPilots
            .Where(p => p.LastRaceDate < today && p.DayStreak > 0)
            .ForEachAsync(pilot => pilot.ResetDayStreak(today));
    }
}
