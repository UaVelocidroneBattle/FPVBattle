using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Bot.Telegram.Commands.Core;

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
        // Check if chat is bound to a cup
        if (context.CupId is null)
        {
            return "This chat is not bound to any cup 🤷";
        }

        var activeCompetition = await _competitions
            .GetAll(c => c.State == CompetitionState.Started)
            .ForCup(context.CupId)
            .FirstOrDefaultAsync();

        return activeCompetition is null
            ? $"No active competition in {context.CupId} cup 😕"
            : $"*{activeCompetition.Track.Map.Name} - `{activeCompetition.Track.Name}`*";
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => true;
}
