using System.Text.Json.Serialization;

namespace Veloci.Logic.Features.Leagues.Models;

[JsonConverter(typeof(JsonStringEnumConverter<RatingQualificationStatus>))]
public enum RatingQualificationStatus
{
    Qualified,
    Reachable,
    OutOfReach
}

public sealed record RatingQualification(int TracksFlown, RatingQualificationStatus Status);

public sealed class CupRatingQualifications
{
    private readonly IReadOnlyDictionary<int, RatingQualification> _byPilot;
    private readonly RatingQualification _notFlownYet;

    internal CupRatingQualifications(
        int requiredTracks,
        RatingQualification notFlownYet,
        IReadOnlyDictionary<int, RatingQualification> byPilot)
    {
        RequiredTracks = requiredTracks;
        _notFlownYet = notFlownYet;
        _byPilot = byPilot;
    }

    public int RequiredTracks { get; }

    public RatingQualification For(int pilotId) => _byPilot.GetValueOrDefault(pilotId, _notFlownYet);
}
