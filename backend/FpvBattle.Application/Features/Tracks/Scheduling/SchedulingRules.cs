using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Tracks.Scheduling;

/// <summary>
/// What a scheduler may get past. Every track is checked the same way; the rules decide which issues block
/// and which are only shown. A patron's track also counts against their monthly quota.
/// </summary>
public sealed class SchedulingRules
{
    private static readonly IReadOnlySet<TrackIssueKind> EveryIssue = Enum.GetValues<TrackIssueKind>().ToHashSet();

    // Admins may pick tracks outside the filter, badly rated or raced recently: they see why, and decide
    public static SchedulingRules Admin { get; } = new(patron: null, blockingIssues: new HashSet<TrackIssueKind> { TrackIssueKind.AlreadyQueued });

    public static SchedulingRules ForPatron(ApplicationUser patron) => new(patron, EveryIssue);

    private readonly IReadOnlySet<TrackIssueKind> _blockingIssues;

    private SchedulingRules(ApplicationUser? patron, IReadOnlySet<TrackIssueKind> blockingIssues)
    {
        Patron = patron;
        _blockingIssues = blockingIssues;
    }

    /// <summary>The patron whose quota the track counts against, or null for an admin.</summary>
    public ApplicationUser? Patron { get; }

    public bool Blocks(TrackIssueKind kind) => _blockingIssues.Contains(kind);

    /// <summary>A track that can never pass isn't worth suggesting.</summary>
    public bool HidesTracksOutsideCupFilter => Blocks(TrackIssueKind.OutsideCupFilter);
}
