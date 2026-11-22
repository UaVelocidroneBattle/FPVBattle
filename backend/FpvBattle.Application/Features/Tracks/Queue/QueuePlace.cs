using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Tracks.Queue;

/// <summary>A track waiting in a cup's queue, with its place among the undated tracks of that cup.</summary>
public sealed record QueuePlace(QueuedTrack QueuedTrack, int Position);
