using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Notifications;

public record IntermediateCompetitionResult(List<LeagueLeaderboard> Leaderboard, Competition Competition) : INotification;
