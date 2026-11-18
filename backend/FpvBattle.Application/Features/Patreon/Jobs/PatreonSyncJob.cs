using FpvBattle.Application.Features.Patreon.Models;
using FpvBattle.Application.Features.Patreon.Notifications;
using FpvBattle.Application.Features.Patreon.Services;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace FpvBattle.Application.Features.Patreon.Jobs;

public class PatreonSyncJob
{
    private static readonly ILogger Log = Serilog.Log.ForContext<PatreonSyncJob>();
    private readonly IPublisher _publisher;
    private readonly PatreonOptions _options;
    private readonly IPatreonService _patreonService;
    private readonly IApplicationDbContext _db;
    private readonly PatreonUserMatcher _userMatcher;

    public PatreonSyncJob(
        IPatreonService patreonService,
        IApplicationDbContext db,
        PatreonUserMatcher userMatcher,
        IPublisher publisher,
        IOptions<PatreonOptions> options)
    {
        _patreonService = patreonService;
        _db = db;
        _userMatcher = userMatcher;
        _publisher = publisher;
        _options = options.Value;
    }

    [DisableConcurrentExecution("PatreonSync", 60)]
    public async Task Handle(CancellationToken ct)
    {
        if (!_options.EnableSync)
        {
            return;
        }

        // Get campaigns first
        var campaigns = await _patreonService.GetCampaignsAsync(ct);

        if (!campaigns.Any())
        {
            Log.Information("No campaigns found in Patreon API");
            return;
        }

        var campaignId = campaigns.First().Id;
        var supportersFromApi = await _patreonService.GetCampaignMembersAsync(campaignId, ct);

        if (!supportersFromApi.Any())
        {
            Log.Information("No supporters received from Patreon API");
            return;
        }

        Log.Information("Retrieved {Count} supporters from Patreon API", supportersFromApi.Length);

        var existingSupporters = await _db.PatreonSupporters
            .ToDictionaryAsync(s => s.PatreonId, ct);

        var newSupporters = new List<PatreonSupporter>();
        var updatedCount = 0;
        var firstTimePayers = new List<PatreonSupporter>();
        var now = DateTime.UtcNow;

        foreach (var supporter in supportersFromApi)
        {
            if (existingSupporters.TryGetValue(supporter.PatreonId, out var existingSupporter))
            {
                if (UpdateSupporterData(existingSupporter, supporter))
                {
                    updatedCount++;
                }

                if (existingSupporter.TryRecordFirstPayment(now))
                {
                    firstTimePayers.Add(existingSupporter);
                }
            }
            else
            {
                _db.PatreonSupporters.Add(supporter);
                newSupporters.Add(supporter);

                if (supporter.TryRecordFirstPayment(now))
                {
                    firstTimePayers.Add(supporter);
                }
            }
        }

        await LinkToPilotsByEmailAsync(newSupporters, existingSupporters.Values, ct);

        await _db.SaveChangesAsync(ct);

        if (firstTimePayers.Count > 0)
        {
            await _publisher.Publish(new NewPatreonSupportersNotification(firstTimePayers), ct);
        }

        Log.Information("Patreon sync completed successfully. New: {NewCount}, Updated: {UpdatedCount}",
            newSupporters.Count, updatedCount);
    }

    private async Task LinkToPilotsByEmailAsync(
        IReadOnlyCollection<PatreonSupporter> newSupporters,
        IEnumerable<PatreonSupporter> existingSupporters,
        CancellationToken ct)
    {
        var usersByEmail = await _userMatcher.FindUsersByEmailAsync(newSupporters, ct);

        var linkedPilotIds = existingSupporters
            .Select(s => s.PilotId)
            .OfType<int>()
            .ToHashSet();

        foreach (var supporter in newSupporters)
        {
            if (supporter.NormalizedEmail is not { } email
                || usersByEmail.GetValueOrDefault(email)?.Pilot is not { } pilot
                || !linkedPilotIds.Add(pilot.Id))
            {
                continue;
            }

            supporter.PilotId = pilot.Id;
            Log.Information("Linked new Patreon supporter {Supporter} to pilot {Pilot} by email", supporter.Name, pilot.Name);
        }
    }

    private static bool UpdateSupporterData(PatreonSupporter existing, PatreonSupporter updated)
    {
        var hasChanges = false;

        if (existing.Name != updated.Name)
        {
            existing.Name = updated.Name;
            hasChanges = true;
        }

        if (existing.Email != updated.Email)
        {
            existing.Email = updated.Email;
            hasChanges = true;
        }

        if (existing.TierName != updated.TierName)
        {
            existing.TierName = updated.TierName;
            hasChanges = true;
        }

        if (existing.Amount != updated.Amount)
        {
            existing.Amount = updated.Amount;
            hasChanges = true;
        }

        if (existing.Status != updated.Status)
        {
            existing.Status = updated.Status;
            hasChanges = true;
        }

        if (hasChanges)
        {
            existing.LastUpdated = DateTime.UtcNow;
        }

        return hasChanges;
    }
}
