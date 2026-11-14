using FpvBattle.Application.Features.Leagues.Models;
using MediatR;

namespace FpvBattle.Application.Features.Leagues.Notifications;

public record LeagueUpdateNotification(string CupId, IList<LeagueUpdateModel> Updates) : INotification;

