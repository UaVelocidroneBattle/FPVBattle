using MediatR;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Notifications;

namespace Veloci.Logic.Services;

public interface IWhiteListService
{
    Task AddToWhiteListAsync(string pilotName);
    Task RemoveFromWhiteListAsync(string pilotName);
    Task<IReadOnlySet<string>> GetWhitelistAsync();
}

public class WhiteListService : IWhiteListService
{
    private readonly IRepository<WhiteListedPilot> _whitelist;
    private readonly IMediator _mediator;

    public WhiteListService(IRepository<WhiteListedPilot> whitelist, IMediator mediator)
    {
        _whitelist = whitelist;
        _mediator = mediator;
    }

    public async Task AddToWhiteListAsync(string pilotName)
    {
        var exists = await _whitelist.GetAll().AnyAsync(wl => wl.PilotName == pilotName);

        if (exists)
            throw new InvalidOperationException($"Pilot '{pilotName}' is already on the whitelist.");

        await _whitelist.AddAsync(new WhiteListedPilot(pilotName));
        await _mediator.Publish(new AddedToWhitelist(pilotName));
    }

    public async Task RemoveFromWhiteListAsync(string pilotName)
    {
        var record = await _whitelist.GetAll().FirstOrDefaultAsync(wl => wl.PilotName == pilotName);

        if (record is null)
            throw new ArgumentException("Pilot not found");

        await _whitelist.RemoveAsync(record.Id);
    }

    public async Task<IReadOnlySet<string>> GetWhitelistAsync()
    {
        return await _whitelist.GetAll().Select(wl => wl.PilotName).ToHashSetAsync();
    }
}
