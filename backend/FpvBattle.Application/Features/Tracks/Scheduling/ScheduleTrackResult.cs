using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Tracks.Scheduling;

/// <param name="ScheduledOn">The day an admin wants the track raced. Without it the track joins the end of the line.</param>
public sealed record ScheduleTrackRequest(string CupId, string TrackId, int? QuadId, DateTime? ScheduledOn = null);

public sealed record ScheduleTrackResult(QueuedTrack? QueuedTrack, string? Error)
{
    public static ScheduleTrackResult Failed(string error) => new(null, error);
}

public enum CancelQueuedTrackResult
{
    Cancelled,
    AlreadyRaced
}

/// <summary>A cup tracks can be scheduled for, with the quads a scheduler may pick besides "any".</summary>
public sealed record SchedulingCup(string CupId, string Name, IReadOnlyList<QuadModel> Quads);
