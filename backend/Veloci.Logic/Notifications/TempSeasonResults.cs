using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Notifications;

public record TempSeasonResults(string CupId, List<LeagueSeasonLeaderboard> Results) : INotification;
