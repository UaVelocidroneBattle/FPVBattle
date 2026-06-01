using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record SeasonFinished(string CupId, List<LeagueSeasonLeaderboard> Results, string SeasonName, string[] Winners, byte[] Image, string ImageName) : INotification;
