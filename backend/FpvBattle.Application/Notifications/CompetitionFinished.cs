using FpvBattle.Application.Features.Cups;
using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record CompetitionFinished(
    Competition Competition,
    List<LeagueLeaderboard> Leaderboard
) : INotification;
