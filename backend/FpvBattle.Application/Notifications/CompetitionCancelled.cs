using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record CompetitionCancelled (Competition Competition) : INotification;
