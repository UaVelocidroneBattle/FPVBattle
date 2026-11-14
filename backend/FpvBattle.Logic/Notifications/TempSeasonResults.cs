using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record TempSeasonResults(string CupId, List<LeagueSeasonLeaderboard> Results) : INotification;
