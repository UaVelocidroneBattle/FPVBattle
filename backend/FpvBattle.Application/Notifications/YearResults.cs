using FpvBattle.Application.Services.Statistics.YearResults;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record YearResults(YearResultsModel Results) : INotification;
