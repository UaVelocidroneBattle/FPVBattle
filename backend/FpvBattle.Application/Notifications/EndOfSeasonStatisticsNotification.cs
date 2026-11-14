using FpvBattle.Application.Services.Statistics;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record EndOfSeasonStatisticsNotification(EndOfSeasonStatisticsDto Statistics) : INotification;
