using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record CompetitionCancelled (Competition Competition) : INotification;
