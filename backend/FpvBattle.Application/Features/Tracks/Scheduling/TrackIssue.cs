namespace FpvBattle.Application.Features.Tracks.Scheduling;

public enum TrackIssueKind
{
    OutsideCupFilter,
    AlreadyQueued,
    BadlyRated,
    RacedRecently
}

/// <summary>A reason not to schedule a track. Whether it stops the scheduler depends on their <see cref="SchedulingRules"/>.</summary>
public sealed record TrackIssue(TrackIssueKind Kind, string Message, bool Blocking);
