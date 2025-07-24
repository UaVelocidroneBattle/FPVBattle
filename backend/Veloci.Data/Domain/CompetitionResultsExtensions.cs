namespace Veloci.Data.Domain;

public static class CompetitionResultsExtensions
{
    public static CompetitionResults? GetByLocalRank(this IEnumerable<CompetitionResults> competitionResults, int rank)
    {
        return competitionResults.SingleOrDefault(res => res.LocalRank == rank);
    }

    public static CompetitionResults? GetByPilotName(this IEnumerable<CompetitionResults> competitionResults, string name)
    {
        return competitionResults.FirstOrDefault(res => res.PlayerName == name);
    }
}
