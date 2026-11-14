using FpvBattle.Domain;
using FpvBattle.Logic.Features.Cups;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record CompetitionFinished(
    Competition Competition,
    List<LeagueLeaderboard> Leaderboard
) : INotification;
