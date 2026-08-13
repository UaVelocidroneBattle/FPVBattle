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

        var openCompetition = await activeCompetitions.ForCup(CupIds.OpenClass).FirstOrDefaultAsync();
        var whoopCompetition = await activeCompetitions.ForCup(CupIds.WhoopClass).FirstOrDefaultAsync();

        return $"Open class:{Environment.NewLine}{FormatTrack(openCompetition)}" +
               $"{Environment.NewLine}{Environment.NewLine}" +
               $"Whoop class:{Environment.NewLine}{FormatTrack(whoopCompetition)}";
    }

    private static string FormatTrack(Competition? competition)
    {
        if (competition is null)
            return "(No track)";

        var trackName = $"*{TelegramMarkdown.EscapeUserText(competition.Track.Map.Name)} - " +
                        $"`{TelegramMarkdown.EscapeCodeText(competition.Track.Name)}`*";

        if (competition.QuadOfTheDay is null)
            return trackName;

        return $"{trackName}{Environment.NewLine}{Environment.NewLine}" +
               $"⚠️ Квад дня: *{TelegramMarkdown.EscapeUserText(competition.QuadOfTheDay.Name)}*";
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => true;
}
