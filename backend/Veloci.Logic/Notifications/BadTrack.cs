using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record BadTrack(Competition Competition, Track Track) : INotification;
