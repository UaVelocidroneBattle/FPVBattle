using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Logic.Persistence;

public interface IApplicationDbContext
{
    DbSet<Competition> Competitions { get; }
    DbSet<CompetitionResults> CompetitionResults { get; }
    DbSet<PatreonSupporter> PatreonSupporters { get; }
    DbSet<PatreonTokens> PatreonTokens { get; }
    DbSet<Pilot> Pilots { get; }
    DbSet<PilotAchievement> PilotAchievements { get; }
    DbSet<PilotClaim> PilotClaims { get; }
    DbSet<PilotLeague> PilotLeagues { get; }
    DbSet<PilotPaceRating> PilotPaceRatings { get; }
    DbSet<PilotPlatformAccount> PilotPlatformAccounts { get; }
    DbSet<QuadModel> QuadModels { get; }
    DbSet<QueuedTrack> QueuedTracks { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Team> Teams { get; }
    DbSet<TeamCompetitionResult> TeamCompetitionResults { get; }
    DbSet<TeamMember> TeamMembers { get; }
    DbSet<TeamMembershipRequest> TeamMembershipRequests { get; }
    DbSet<Track> Tracks { get; }
    DbSet<TrackMap> TrackMaps { get; }
    DbSet<TrackTimeDelta> TrackTimeDeltas { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<UserNotification> UserNotifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
