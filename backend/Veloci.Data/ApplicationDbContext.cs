using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;

namespace Veloci.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Competition>().ToTable("Competitions");
        builder.Entity<Track>().ToTable("Tracks");
        builder.Entity<TrackMap>().ToTable("TrackMaps");
        builder.Entity<TrackResults>().ToTable("TrackResults");
        builder.Entity<TrackTime>().ToTable("TrackTimes");
        builder.Entity<TrackTimeDelta>().ToTable("TrackTimeDeltas");
        builder.Entity<CompetitionResults>().ToTable("CompetitionResults");
        builder.Entity<Pilot>().ToTable("Pilots");
        builder.Entity<PilotAchievement>().ToTable("PilotAchievements");
        builder.Entity<DayStreakFreeze>().ToTable("DayStreakFreezes");
        builder.Entity<CompetitionVariable>().ToTable("CompetitionVariables");
        builder.Entity<PatreonSupporter>().ToTable("PatreonSupporters");
    }
}
