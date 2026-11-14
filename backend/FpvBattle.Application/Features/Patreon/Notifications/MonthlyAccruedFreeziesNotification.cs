using FpvBattle.Application.Features.Patreon.Models;
using MediatR;

namespace FpvBattle.Application.Features.Patreon.Notifications;

public record MonthlyAccruedFreeziesNotification(List<AccruedPatronFreezies> Accrued) : INotification;
