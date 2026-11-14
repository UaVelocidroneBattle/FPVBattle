using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record SeasonFinished(string CupId, List<LeagueSeasonLeaderboard> Results, string SeasonName, string[] Winners, byte[]? Image, string ImageName) : INotification;
