using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Tracks.Scheduling;

/// <summary>A suggested track, when it was last raced, and its issues under the scheduler's rules.</summary>
public sealed record SchedulableTrack(Track Track, LastRace? LastRace, IReadOnlyList<TrackIssue> Issues)
{
    public TrackIssue? BlockingIssue => Issues.FirstOrDefault(issue => issue.Blocking);
}

public sealed record LastRace(DateTime StartedOn, string CupId, bool IsInReuseCooldown);
