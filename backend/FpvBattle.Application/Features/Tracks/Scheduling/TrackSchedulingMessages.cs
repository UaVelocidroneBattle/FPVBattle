using FpvBattle.Application.Features.Tracks.Queue;
using FpvBattle.Application.Features.Tracks.Selection;
using static System.FormattableString;

namespace FpvBattle.Application.Features.Tracks.Scheduling;

public static class TrackSchedulingMessages
{
    public const string PatronsOnly = "Scheduling tracks is a perk for Patreon supporters";
    public const string SignIn = PatronsOnly + ". Sign in to use it";
    public const string PilotNotLinked = "Link your Velocidrone pilot on your profile first";
    public const string TierNotIncluded = "Your Patreon tier doesn't include track scheduling";
    public const string CupNotAvailable = "This competition is not available";
    public const string TrackNotFound = "The track was not found or is no longer available";
    public const string OutsideCupFilter = "Not allowed by the track filter";
    public const string QuadNotAllowed = "This quad can't be picked for this competition";
    public const string AlreadyScheduled = TrackAlreadyQueuedException.DefaultMessage;

    public static string MonthlyLimitReached(int limit, DateTime nextMonth) =>
        Invariant($"You've scheduled all {limit} tracks for this month. More from {nextMonth:d MMMM}");

    public static string DateTaken(DateTime date) => Invariant($"There is already a track scheduled on {date:dd.MM.yyyy}");

    public static string BadlyRated(double rating) => Invariant($"Low rating ({rating:0.0})");

    public static string RacedRecently(DateTime racedOn) =>
        Invariant($"Raced {racedOn:dd.MM.yyyy}, available from {TrackReuseRules.AvailableAgainOn(racedOn):dd.MM.yyyy}");
}
