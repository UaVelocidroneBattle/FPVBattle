using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record IntermediateCompetitionResult(List<CompetitionResults> Leaderboard, Competition Competition) : INotification;
