using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Settings;

namespace Veloci.Logic.Services.Leagues;

public class LeagueService
{
    private readonly List<LeagueSettings> _leaguesSettings;
    private readonly IRepository<League> _leaguesRepository;

    public LeagueService(
        IOptions<List<LeagueSettings>> options,
        IRepository<League> leaguesRepository)
    {
        _leaguesRepository = leaguesRepository;
        _leaguesSettings = options.Value;
    }



    public async Task UpdateLeaguesAsync()
    {
        foreach (var league in _leaguesSettings)
        {
            var dbLeague = await _leaguesRepository
                .GetAll()
                .FirstOrDefaultAsync(l => l.Order == league.Order);

            if (dbLeague is null)
            {
                var newLeague = new League
                {
                    Order = league.Order,
                    Name = league.Name
                };

                await _leaguesRepository.AddAsync(newLeague);
                continue;
            }

            if (dbLeague.Name != league.Name)
            {
                dbLeague.Name = league.Name;
                await _leaguesRepository.SaveChangesAsync();
            }
        }
    }
}
