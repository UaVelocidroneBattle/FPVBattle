using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Bot.Telegram.Commands.Core;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class CurrentTrackCommand : ITelegramCommand
{
    private readonly IRepository<Competition> _competitions;

    public CurrentTrackCommand(IRepository<Competition> competitions)
    {
        _competitions = competitions;
    }

    public string[] Keywords => ["/current-track", "/ct"];
    public string Description => "`/current-track` або `/ct` - Current track";

    public async Task<string> ExecuteAsync(TelegramCommandContext context)
    {
        var activeCompetitions = _competitions
            .GetAll(c => c.State == CompetitionState.Started)
            .Include(c => c.Track)
            .ThenInclude(t => t.Map);

        var openCompetition = await activeCompetitions
            .ForCup(CupIds.OpenClass)
            .FirstOrDefaultAsync();

        var openTrack = openCompetition is null
            ? "(No track)"
            : $"*{openCompetition.Track.Map.Name} - `{openCompetition.Track.Name}`*";

        var whoopCompetition = await activeCompetitions
            .ForCup(CupIds.WhoopClass)
            .FirstOrDefaultAsync();

        var whoopTrack = whoopCompetition is null
            ? "(No track)"
            : $"*{whoopCompetition.Track.Map.Name} - `{whoopCompetition.Track.Name}`*";

        return $"Open class:{Environment.NewLine}{openTrack}" +
               $"{Environment.NewLine}{Environment.NewLine}" +
               $"Whoop class:{Environment.NewLine}{whoopTrack}";
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => true;
}
