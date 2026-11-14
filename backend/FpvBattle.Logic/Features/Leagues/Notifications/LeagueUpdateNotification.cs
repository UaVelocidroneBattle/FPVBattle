using FpvBattle.Logic.Features.Leagues.Models;
using MediatR;

namespace FpvBattle.Logic.Features.Leagues.Notifications;

public record LeagueUpdateNotification(string CupId, IList<LeagueUpdateModel> Updates) : INotification;

