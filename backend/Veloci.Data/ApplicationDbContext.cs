using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;

namespace Veloci.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().Property(u => u.DisplayName).HasMaxLength(256);
        builder.Entity<ApplicationUser>().Property(u => u.Locale).HasMaxLength(16);
        builder.Entity<ApplicationUser>().HasIndex(u => u.PilotId).IsUnique();
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Pilot)
            .WithMany()
            .HasForeignKey(u => u.PilotId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PilotClaim>().ToTable("PilotClaims");
        builder.Entity<PilotClaim>().HasKey(c => c.Id);
        builder.Entity<PilotClaim>().Property(c => c.PilotName).HasMaxLength(128).IsRequired();
        builder.Entity<PilotClaim>().HasIndex(c => c.UserId).IsUnique();
        builder.Entity<PilotClaim>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Competition>().ToTable("Competitions");
        builder.Entity<Competition>().Property(c => c.CupId).HasMaxLength(64).IsRequired().HasDefaultValue("open-class");
        builder.Entity<Competition>().HasIndex(c => c.StartedOn);

        builder.Entity<Track>().ToTable("Tracks");

        builder.Entity<TrackMap>().ToTable("TrackMaps");

        builder.Entity<TrackResults>().ToTable("TrackResults");

        builder.Entity<TrackTime>().ToTable("TrackTimes");
        builder.Entity<TrackTime>().HasIndex(t => t.UserId);
        builder.Entity<TrackTime>().Property(t => t.PlayerName).HasMaxLength(128);
        builder.Entity<TrackTime>().Property(t => t.ModelName).HasMaxLength(128);
        builder.Entity<TrackTime>().Property(t => t.Country).HasMaxLength(8).HasDefaultValue("UA");

        builder.Entity<TrackTimeDelta>().ToTable("TrackTimeDeltas");
        builder.Entity<TrackTimeDelta>().Property(t => t.ModelName).HasMaxLength(128);
        builder.Entity<TrackTimeDelta>().Property(t => t.Country).HasMaxLength(8).HasDefaultValue("UA");

        builder.Entity<CompetitionResults>().ToTable("CompetitionResults");
        builder.Entity<CompetitionResults>().Property(c => c.ModelName).HasMaxLength(128);

        builder.Entity<Pilot>().ToTable("Pilots");
        builder.Entity<Pilot>().HasKey(p => p.Id);
        builder.Entity<Pilot>().Property(p => p.Id).ValueGeneratedNever();
        builder.Entity<Pilot>().Property(p => p.Name).HasMaxLength(128);
        builder.Entity<Pilot>().Property(p => p.Country).HasMaxLength(8).HasDefaultValue("UA");

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

        builder.Entity<PilotNameHistoryRow>().ToTable("PilotNameHistory");
        builder.Entity<PilotNameHistoryRow>().HasKey(p => p.Id);
        builder.Entity<PilotNameHistoryRow>().Property(p => p.OldName).HasMaxLength(128);
        builder.Entity<PilotNameHistoryRow>().Property(p => p.NewName).HasMaxLength(128);

        builder.Entity<PilotPlatformAccount>().ToTable("PilotPlatformAccounts");
        builder.Entity<PilotPlatformAccount>().HasKey(p => p.Id);
        builder.Entity<PilotPlatformAccount>().Property(p => p.Username).HasMaxLength(128);

        builder.Entity<PilotPaceRating>().ToTable("PilotPaceRatings");
        builder.Entity<PilotPaceRating>().HasKey(p => p.Id);
        builder.Entity<PilotPaceRating>().Property(p => p.CupId).HasMaxLength(64).IsRequired();
        builder.Entity<PilotPaceRating>().HasIndex(p => new { p.PilotId, p.CupId });

        builder.Entity<QueuedTrack>().ToTable("TrackQueue");
        builder.Entity<QueuedTrack>().HasKey(p => p.Id);

        builder.Entity<PilotLeague>().ToTable("PilotLeagues");
        builder.Entity<PilotLeague>().HasKey(p => p.Id);
        builder.Entity<PilotLeague>().Property(p => p.CupId).HasMaxLength(64).IsRequired();
        builder.Entity<PilotLeague>().Property(p => p.League).HasMaxLength(64);
        builder.Entity<PilotLeague>().HasIndex(p => new { p.PilotId, p.CupId });

        builder.Entity<RefreshToken>().ToTable("RefreshTokens");
        builder.Entity<RefreshToken>().HasKey(t => t.Id);
        builder.Entity<RefreshToken>().Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
        builder.Entity<RefreshToken>().HasIndex(t => t.TokenHash).IsUnique();
        builder.Entity<RefreshToken>().HasIndex(t => t.UserId);
        builder.Entity<RefreshToken>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuadModel>().ToTable("QuadModels");
        builder.Entity<QuadModel>().HasKey(p => p.Id);
        builder.Entity<QuadModel>().Property(p => p.Id).ValueGeneratedNever();
        builder.Entity<QuadModel>().Property(p => p.Name).HasMaxLength(128);
    }
}
