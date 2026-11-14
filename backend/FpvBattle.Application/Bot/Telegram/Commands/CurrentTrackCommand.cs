using FpvBattle.Application.Bot.Telegram.Commands.Core;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Bot.Telegram.Commands;

public class CurrentTrackCommand : ITelegramCommand
{
    private readonly IApplicationDbContext _db;

    public CurrentTrackCommand(IApplicationDbContext db)
    {
        _db = db;
    }

    public string[] Keywords => ["/current-track", "/ct"];
    public string Description => "`/current-track` або `/ct` - Current track";

    public async Task<string> ExecuteAsync(TelegramCommandContext context)
    {
        var activeCompetitions = _db.Competitions.Where(c => c.State == CompetitionState.Started)
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

        var trackName = $"*{TelegramMarkdown.Escape(competition.Track.Map.Name)} - " +
                        $"`{TelegramMarkdown.Escape(competition.Track.Name)}`*";

        if (competition.QuadOfTheDay is null)
            return trackName;

        return $"{trackName}{Environment.NewLine}{Environment.NewLine}" +
               $"⚠️ Квад дня: *{TelegramMarkdown.Escape(competition.QuadOfTheDay.Name)}*";
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => true;
}
