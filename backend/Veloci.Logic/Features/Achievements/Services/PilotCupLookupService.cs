using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Achievements.Notifications;

namespace Veloci.Logic.Features.Achievements.Services;

/// <summary>
/// Service for looking up which cups pilots participated in
/// </summary>
public interface IPilotCupLookupService
{
    /// <summary>
    /// Gets pilot cup participation data for a specific date
    /// </summary>
    /// <param name="pilots">List of pilots to check</param>
    /// <param name="date">Target date (defaults to current UTC date)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pilot participation records with their cup IDs</returns>
    Task<List<PilotCupParticipation>> GetPilotCupsAsync(
        List<Pilot> pilots,
        DateTime? date = null,
        CancellationToken cancellationToken = default);
}

public class PilotCupLookupService : IPilotCupLookupService
{
    private static readonly ILogger _log = Log.ForContext<PilotCupLookupService>();
    private readonly IRepository<Competition> _competitions;

    public PilotCupLookupService(IRepository<Competition> competitions)
    {
        _competitions = competitions;
    }

    public async Task<List<PilotCupParticipation>> GetPilotCupsAsync(
        List<Pilot> pilots,
        DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var pilotIds = pilots.Select(p => p.Id).ToHashSet();

        _log.Debug("Looking up cups for {PilotCount} pilots on {Date}",
            pilots.Count, targetDate.Date);

        // Query returns flat list of (PilotId, CupId) pairs
        var participations = await _competitions
            .GetAll()
            .OnDate(targetDate)
            .NotCancelled()
            .SelectMany(comp => comp.CompetitionResults
                .Where(r => pilotIds.Contains(r.PilotId))
                .Select(r => new { r.PilotId, comp.CupId }))
            .ToListAsync(cancellationToken);

        // Group by pilot and extract distinct cup IDs
        var pilotCupMap = participations
            .GroupBy(p => p.PilotId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.CupId).Distinct().ToList());

        // Build result list matching pilot order, including pilots with no cups
        var result = pilots
            .Select(pilot => new PilotCupParticipation(
                pilot,
                pilotCupMap.GetValueOrDefault(pilot.Id) ?? []))
            .ToList();

        foreach (var participation in result)
        {
            _log.Debug("Pilot {PilotName} (ID: {PilotId}) flew in {CupCount} cups: {CupIds}",
                participation.Pilot.Name, participation.Pilot.Id,
                participation.CupIds.Count, string.Join(", ", participation.CupIds));
        }

        return result;
    }
}
