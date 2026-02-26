using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Services;

public class WhiteListService
{
    private readonly IRepository<WhiteListedPilot> _whitelist;

    public WhiteListService(IRepository<WhiteListedPilot> whitelist)
    {
        _whitelist = whitelist;
    }

    public async Task AddToWhiteListAsync(string pilotName)
    {
        var exists = await _whitelist.GetAll().AnyAsync(wl => wl.PilotName == pilotName);

        if (exists)
            throw new InvalidOperationException($"Pilot '{pilotName}' is already on the whitelist.");

        await _whitelist.AddAsync(new WhiteListedPilot(pilotName));
    }

    public async Task RemoveFromWhiteListAsync(string pilotName)
    {
        var record = await _whitelist.GetAll().FirstOrDefaultAsync(wl => wl.PilotName == pilotName);

        if (record is null)
            throw new ArgumentException("Pilot not found");

        await _whitelist.RemoveAsync(record.Id);
    }

    public async Task<HashSet<string>> GetWhitelistAsync()
    {
        return await _whitelist.GetAll().Select(wl => wl.PilotName).ToHashSetAsync();
    }
}
