using MediatR;
using Veloci.Logic.Services.Statistics;

namespace Veloci.Logic.Notifications;

public record EndOfSeasonStatisticsNotification(EndOfSeasonStatisticsDto Statistics) : INotification;
