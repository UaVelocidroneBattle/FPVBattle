using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record NewPilot(Pilot Pilot, string CupId) : INotification;
