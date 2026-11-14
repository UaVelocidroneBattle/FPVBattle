using FpvBattle.Logic.Services.Statistics.YearResults;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record YearResults(YearResultsModel Results) : INotification;
