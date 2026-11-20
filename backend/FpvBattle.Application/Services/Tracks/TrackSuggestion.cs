using FpvBattle.Domain;

namespace FpvBattle.Application.Services.Tracks;

public sealed record TrackSuggestion(Track Track, bool MatchesCupFilter, LastRace? LastRace);

public sealed record LastRace(DateTime StartedOn, string CupId, bool IsInReuseCooldown);
