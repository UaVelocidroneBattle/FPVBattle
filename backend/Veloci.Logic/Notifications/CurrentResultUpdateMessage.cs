using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record CurrentResultUpdateMessage(Competition Competition, List<TrackTimeDelta> Deltas) : INotification;
