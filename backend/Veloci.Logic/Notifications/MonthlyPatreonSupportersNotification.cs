using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record MonthlyPatreonSupportersNotification(List<PatreonSupporter> Supporters) : INotification;