using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record TempSeasonResults(List<SeasonResult> Results) : INotification;
