using FpvBattle.Logic.Services.Statistics;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record EndOfSeasonStatisticsNotification(EndOfSeasonStatisticsDto Statistics) : INotification;
