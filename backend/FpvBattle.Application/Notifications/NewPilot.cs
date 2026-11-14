using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record NewPilot(Pilot Pilot, string CupId) : INotification;
