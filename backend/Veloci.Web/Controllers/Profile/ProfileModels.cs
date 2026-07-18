namespace Veloci.Web.Controllers.Profile;

public class ProfileModel
{
    public required string DisplayName { get; set; }

    public required string Email { get; set; }

    /// <summary>The pilot linked to this account, when verification has completed.</summary>
    public LinkedPilotModel? Pilot { get; set; }

    /// <summary>The claim awaiting its verification race, when one is open.</summary>
    public PendingClaimModel? PendingClaim { get; set; }
}

public class LinkedPilotModel
{
    public required int Id { get; set; }

    public required string Name { get; set; }

    public required string Country { get; set; }

    public required int DayStreak { get; set; }

    public required int TotalRaceDays { get; set; }

    public DateTime? LastRaceDate { get; set; }
}

public class PendingClaimModel
{
    public required string PilotName { get; set; }

    public required DateTime ExpiresAt { get; set; }

    public required bool IsExpired { get; set; }
}

public class PilotLookupModel
{
    public required bool Found { get; set; }

    /// <summary>True when the found pilot is already linked to another account.</summary>
    public bool AlreadyLinked { get; set; }

    public string? Name { get; set; }

    public string? Country { get; set; }

    public int TotalRaceDays { get; set; }

    public DateTime? LastRaceDate { get; set; }
}

public class ClaimPilotRequest
{
    public required string PilotName { get; set; }
}
