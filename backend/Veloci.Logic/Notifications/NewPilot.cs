using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record NewPilot (Pilot Pilot) : INotification;
