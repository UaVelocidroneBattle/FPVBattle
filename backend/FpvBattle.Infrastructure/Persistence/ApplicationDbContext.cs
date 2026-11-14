using FpvBattle.Domain;
using FpvBattle.Logic.Persistence;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Explicit implementations keep these sets out of EF's DbSet property discovery,
    // which would otherwise rename tables to the property names.
    DbSet<Competition> IApplicationDbContext.Competitions => Set<Competition>();
    DbSet<CompetitionResults> IApplicationDbContext.CompetitionResults => Set<CompetitionResults>();
    DbSet<PatreonSupporter> IApplicationDbContext.PatreonSupporters => Set<PatreonSupporter>();
    DbSet<PatreonTokens> IApplicationDbContext.PatreonTokens => Set<PatreonTokens>();
    DbSet<Pilot> IApplicationDbContext.Pilots => Set<Pilot>();
    DbSet<PilotAchievement> IApplicationDbContext.PilotAchievements => Set<PilotAchievement>();
    DbSet<PilotClaim> IApplicationDbContext.PilotClaims => Set<PilotClaim>();
    DbSet<PilotLeague> IApplicationDbContext.PilotLeagues => Set<PilotLeague>();
    DbSet<PilotPaceRating> IApplicationDbContext.PilotPaceRatings => Set<PilotPaceRating>();
    DbSet<PilotPlatformAccount> IApplicationDbContext.PilotPlatformAccounts => Set<PilotPlatformAccount>();
    DbSet<QuadModel> IApplicationDbContext.QuadModels => Set<QuadModel>();
    DbSet<QueuedTrack> IApplicationDbContext.QueuedTracks => Set<QueuedTrack>();
    DbSet<RefreshToken> IApplicationDbContext.RefreshTokens => Set<RefreshToken>();
    DbSet<Team> IApplicationDbContext.Teams => Set<Team>();
    DbSet<TeamCompetitionResult> IApplicationDbContext.TeamCompetitionResults => Set<TeamCompetitionResult>();
    DbSet<TeamMember> IApplicationDbContext.TeamMembers => Set<TeamMember>();
    DbSet<TeamMembershipRequest> IApplicationDbContext.TeamMembershipRequests => Set<TeamMembershipRequest>();
    DbSet<Track> IApplicationDbContext.Tracks => Set<Track>();
    DbSet<TrackMap> IApplicationDbContext.TrackMaps => Set<TrackMap>();
    DbSet<TrackTimeDelta> IApplicationDbContext.TrackTimeDeltas => Set<TrackTimeDelta>();
    DbSet<ApplicationUser> IApplicationDbContext.Users => Users;
    DbSet<UserNotification> IApplicationDbContext.UserNotifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
