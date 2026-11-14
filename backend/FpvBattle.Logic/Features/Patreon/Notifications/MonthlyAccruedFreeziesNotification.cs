using FpvBattle.Logic.Features.Patreon.Models;
using MediatR;

namespace FpvBattle.Logic.Features.Patreon.Notifications;

public record MonthlyAccruedFreeziesNotification(List<AccruedPatronFreezies> Accrued) : INotification;
