using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record BadTrack(Competition Competition, Track Track) : INotification;
