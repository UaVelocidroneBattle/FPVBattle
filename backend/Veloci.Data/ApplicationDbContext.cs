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
        builder.Entity<TrackTime>().HasIndex(t => t.UserId);
        builder.Entity<TrackTime>().Property(t => t.PlayerName).HasMaxLength(128);
        builder.Entity<TrackTime>().Property(t => t.ModelName).HasMaxLength(128);

        builder.Entity<TrackTimeDelta>().ToTable("TrackTimeDeltas");
        builder.Entity<TrackTimeDelta>().Property(t => t.PlayerName).HasMaxLength(128);
        builder.Entity<TrackTimeDelta>().Property(t => t.ModelName).HasMaxLength(128);

        builder.Entity<CompetitionResults>().ToTable("CompetitionResults");
        builder.Entity<CompetitionResults>().Property(c => c.PlayerName).HasMaxLength(128);
        builder.Entity<CompetitionResults>().Property(c => c.ModelName).HasMaxLength(128);

        builder.Entity<Pilot>().ToTable("Pilots");
        builder.Entity<Pilot>().HasKey(p => p.Id);
        builder.Entity<Pilot>().Property(p => p.Id).ValueGeneratedNever();
        builder.Entity<Pilot>().Property(p => p.Name).HasMaxLength(128);

        builder.Entity<PilotAchievement>().ToTable("PilotAchievements");
        builder.Entity<DayStreakFreeze>().ToTable("DayStreakFreezes");
        builder.Entity<CompetitionVariable>().ToTable("CompetitionVariables");

        builder.Entity<PatreonSupporter>().ToTable("PatreonSupporters");
        builder.Entity<PatreonSupporter>().HasKey(p => p.PatreonId);
        builder.Entity<PatreonSupporter>().Property(p => p.PatreonId).HasMaxLength(128);
        builder.Entity<PatreonSupporter>().Property(p => p.Name).HasMaxLength(256);
        builder.Entity<PatreonSupporter>().Property(p => p.Email).HasMaxLength(256);
        builder.Entity<PatreonSupporter>().Property(p => p.TierName).HasMaxLength(128);
        builder.Entity<PatreonSupporter>().Property(p => p.Status).HasMaxLength(64);

        builder.Entity<PatreonTokens>().ToTable("PatreonTokens");
        builder.Entity<PatreonTokens>().HasKey(p => p.Id);
        builder.Entity<PatreonTokens>().Property(p => p.AccessToken).HasMaxLength(2048);
        builder.Entity<PatreonTokens>().Property(p => p.RefreshToken).HasMaxLength(2048);
        builder.Entity<PatreonTokens>().Property(p => p.Scope).HasMaxLength(256);
    }
}
