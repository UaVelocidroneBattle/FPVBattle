using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Notifications;

public record NewPilot(Pilot Pilot, string CupId) : INotification;
