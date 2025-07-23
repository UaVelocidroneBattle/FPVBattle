using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record CompetitionStarted(Competition Competition, Track Track, IList<string> PilotsFlownOnTrack) : INotification;
