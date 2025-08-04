using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Patreon.Models;
using Veloci.Logic.Features.Patreon.Notifications;
using Veloci.Logic.Features.Patreon.Services;

namespace Veloci.Logic.Features.Patreon.Commands;

public class PatreonSyncJob
{
    private static readonly ILogger _log = Log.ForContext<PatreonSyncJob>();
    private readonly IMediator _mediator;
    private readonly PatreonOptions _options;

    private readonly IPatreonService _patreonService;
    private readonly IRepository<PatreonSupporter> _supportersRepository;

    public PatreonSyncJob(
        IPatreonService patreonService,
        IRepository<PatreonSupporter> supportersRepository,
        IMediator mediator,
        IOptions<PatreonOptions> options)
    {
        _patreonService = patreonService;
        _supportersRepository = supportersRepository;
        _mediator = mediator;
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
                _log.Information("No campaigns found in Patreon API");
                return;
            }

            var campaignId = campaigns.First().Id;
            var supportersFromApi = await _patreonService.GetCampaignMembersAsync(campaignId, ct);

            if (!supportersFromApi.Any())
            {
                _log.Information("No supporters received from Patreon API");
                return;
            }

            _log.Information("Retrieved {Count} supporters from Patreon API", supportersFromApi.Length);

            var existingSupporters = await _supportersRepository
                .GetAll()
                .ToDictionaryAsync(s => s.PatreonId, ct);

            var newSupporters = new List<PatreonSupporter>();
            var updatedSupporters = new List<PatreonSupporter>();

            foreach (var supporter in supportersFromApi)
            {
                if (existingSupporters.TryGetValue(supporter.PatreonId, out var existingSupporter))
                {
                    // Update existing supporter
                    var hasChanges = UpdateSupporterData(existingSupporter, supporter);
                    if (hasChanges)
                    {
                        updatedSupporters.Add(existingSupporter);
                    }
                }
                else
                {
                    // New supporter
                    newSupporters.Add(supporter);
                }
            }

            // Save new supporters
            if (newSupporters.Any())
            {
                await _supportersRepository.AddRangeAsync(newSupporters);
                _log.Information("Added {Count} new supporters", newSupporters.Count);

                // Send notifications for new supporters
                foreach (var newSupporter in newSupporters)
                {
                    await _mediator.Publish(new NewPatreonSupporterNotification(newSupporter), ct);
                }
            }

            // Update existing supporters
            if (updatedSupporters.Any())
            {
                foreach (var updatedSupporter in updatedSupporters)
                {
                    await _supportersRepository.UpdateAsync(updatedSupporter);
                }

                _log.Information("Updated {Count} existing supporters", updatedSupporters.Count);
            }

            await _supportersRepository.SaveChangesAsync();

            _log.Information("Patreon sync completed successfully. New: {NewCount}, Updated: {UpdatedCount}",
                newSupporters.Count, updatedSupporters.Count);
    }

    private bool UpdateSupporterData(PatreonSupporter existing, PatreonSupporter updated)
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
