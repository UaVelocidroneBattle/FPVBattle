using MediatR;
using Veloci.Domain;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Notifications;

public record CompetitionFinished(
    Competition Competition,
    List<LeagueLeaderboard> Leaderboard
) : INotification;
