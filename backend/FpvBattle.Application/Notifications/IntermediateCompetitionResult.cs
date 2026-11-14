using MediatR;
using FpvBattle.Domain;

namespace FpvBattle.Application.Notifications;

public record IntermediateCompetitionResult(List<LeagueLeaderboard> Leaderboard, Competition Competition) : INotification;
