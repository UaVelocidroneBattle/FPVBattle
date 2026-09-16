namespace Veloci.Logic.Features.Leagues.Services;

public class InAppLeagueMessageComposer
{
    public string LeagueUpdateMessage(string? oldLeague, string? newLeague)
    {
        if (newLeague is null)
            return $"You dropped out of the {oldLeague?.ToUpperInvariant()} league";

        if (oldLeague is null)
            return $"You joined the {newLeague.ToUpperInvariant()} league";

        return $"You moved from the {oldLeague.ToUpperInvariant()} to the {newLeague.ToUpperInvariant()} league";
    }
}
