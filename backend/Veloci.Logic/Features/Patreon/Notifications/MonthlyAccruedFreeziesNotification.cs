using MediatR;
using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Notifications;

public record MonthlyAccruedFreeziesNotification(List<AccruedPatronFreezies> Accrued) : INotification;
