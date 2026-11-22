namespace FpvBattle.Web.Controllers.Api.TrackScheduling;

public class TrackSchedulingOverviewModel
{
    /// <summary>False hides the feature: the button isn't shown at all.</summary>
    public required bool Enabled { get; set; }

    /// <summary>Why the user can't schedule a track now, or null when they can.</summary>
    public string? BlockedReason { get; set; }

    public required int MonthlyLimit { get; set; }

    public required int UsedThisMonth { get; set; }

    public required int Remaining { get; set; }

    public required List<SchedulingCupModel> Cups { get; set; }

    /// <summary>The user's tracks waiting in a queue, oldest first.</summary>
    public required List<QueuedTrackModel> MyTracks { get; set; }
}

public class SchedulingCupModel
{
    public required string CupId { get; set; }

    public required string Name { get; set; }

    /// <summary>Quads a patron may pick besides "any". Empty when the cup offers only "any".</summary>
    public required List<SchedulingQuadModel> Quads { get; set; }
}

public class SchedulingQuadModel
{
    public required int Id { get; set; }

    public required string Name { get; set; }
}

public class SchedulableTrackModel
{
    public required string TrackId { get; set; }

    public required string Name { get; set; }

    public required string SceneName { get; set; }

    /// <summary>Why the track can't be scheduled now, or null when it can.</summary>
    public string? Restriction { get; set; }
}

public class QueuedTrackModel
{
    public required Guid Id { get; set; }

    public required string CupId { get; set; }

    public required string TrackName { get; set; }

    public required string SceneName { get; set; }

    public string? QuadName { get; set; }

    public required DateTime AddedOn { get; set; }

    /// <summary>Place among the cup's undated queued tracks; 1 is raced next unless an admin dated a track for that day.</summary>
    public required int QueuePosition { get; set; }
}

public class ScheduleTrackModel
{
    public required string CupId { get; set; }

    public required string TrackId { get; set; }

    /// <summary>Null means any quad.</summary>
    public int? QuadId { get; set; }
}

public class QueuedTrackReferenceModel
{
    public required Guid Id { get; set; }
}
