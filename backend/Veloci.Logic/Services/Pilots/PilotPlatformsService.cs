using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Services.Pilots;

public class PilotPlatformsService
{
    private readonly IRepository<PilotPlatformAccount> _pilotPlatforms;

    public PilotPlatformsService(IRepository<PilotPlatformAccount> pilotPlatforms)
    {
        _pilotPlatforms = pilotPlatforms;
    }

    public async Task AddOrUpdatePlatformAsync(Pilot pilot, PlatformNames platform, string username)
    {
        var account = _pilotPlatforms.GetAll(pp => pp.PilotId == pilot.Id)
            .SingleOrDefault(pp => pp.PlatformName == platform);

        if (account is null)
        {
            var newAccount = new PilotPlatformAccount
            {
                PilotId = pilot.Id,
                PlatformName = platform,
                Username = username
            };

            await _pilotPlatforms.AddAsync(newAccount);
            return;
        }

        account.Username = username;
        await _pilotPlatforms.SaveChangesAsync();
    }
}
