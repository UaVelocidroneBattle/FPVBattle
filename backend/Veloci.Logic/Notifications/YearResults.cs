using MediatR;
using Veloci.Logic.Services.YearResults;

namespace Veloci.Logic.Notifications;

public record YearResults(YearResultsModel Results) : INotification;
