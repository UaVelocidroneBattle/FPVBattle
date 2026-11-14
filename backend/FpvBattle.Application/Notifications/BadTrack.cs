using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record BadTrack(Competition Competition, Track Track) : INotification;
