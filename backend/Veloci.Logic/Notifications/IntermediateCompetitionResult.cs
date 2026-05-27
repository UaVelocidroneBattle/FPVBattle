using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record IntermediateCompetitionResult(List<LeagueLeaderboard> Leaderboard, Competition Competition) : INotification;
