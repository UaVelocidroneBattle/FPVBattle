using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Notifications;

public record BadTrack(Competition Competition, Track Track) : INotification;
