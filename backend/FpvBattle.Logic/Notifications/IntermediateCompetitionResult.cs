using MediatR;
using FpvBattle.Domain;

namespace FpvBattle.Logic.Notifications;

public record IntermediateCompetitionResult(List<LeagueLeaderboard> Leaderboard, Competition Competition) : INotification;
