using MediatR;
using Veloci.Logic.Services.Statistics.YearResults;

namespace Veloci.Logic.Notifications;

public record YearResults(YearResultsModel Results) : INotification;
