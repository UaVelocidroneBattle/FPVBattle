using FpvBattle.Domain;
using FpvBattle.Logic.Persistence;

namespace FpvBattle.Logic.Services.Pilots;

public class PilotPlatformsService
{
    private readonly IApplicationDbContext _db;

    public PilotPlatformsService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddOrUpdatePlatformAsync(Pilot pilot, PlatformNames platform, string username)
    {
        var account = _db.PilotPlatformAccounts.Where(pp => pp.PilotId == pilot.Id)
            .SingleOrDefault(pp => pp.PlatformName == platform);

        if (account is null)
        {
            var newAccount = new PilotPlatformAccount
            {
                PilotId = pilot.Id,
                PlatformName = platform,
                Username = username
            };

            _db.PilotPlatformAccounts.Add(newAccount);
            await _db.SaveChangesAsync();
            return;
        }

        account.Username = username;
        await _db.SaveChangesAsync();
    }
}
