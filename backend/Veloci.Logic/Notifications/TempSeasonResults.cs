using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record TempSeasonResults(string CupId, List<SeasonResult> Results) : INotification;
