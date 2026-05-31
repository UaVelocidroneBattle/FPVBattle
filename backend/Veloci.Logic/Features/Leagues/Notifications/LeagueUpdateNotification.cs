using MediatR;
using Veloci.Logic.Features.Leagues.Models;

namespace Veloci.Logic.Features.Leagues.Notifications;

public record LeagueUpdateNotification(string CupId, IList<LeagueUpdateModel> Updates) : INotification;

