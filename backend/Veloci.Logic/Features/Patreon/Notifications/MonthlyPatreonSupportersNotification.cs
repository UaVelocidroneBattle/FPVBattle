using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Features.Patreon.Notifications;

public record MonthlyPatreonSupportersNotification(List<PatreonSupporter> Supporters) : INotification;
