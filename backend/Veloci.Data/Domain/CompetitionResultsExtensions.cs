namespace Veloci.Data.Domain;

public static class CompetitionResultsExtensions
{
    public static CompetitionResults? GetByLocalRank(this IEnumerable<CompetitionResults> competitionResults, int rank)
    {
        return competitionResults.SingleOrDefault(res => res.LocalRank == rank);
    }

    public static CompetitionResults? GetByPilotId(this IEnumerable<CompetitionResults> competitionResults, int pilotId)
    {
        return competitionResults.FirstOrDefault(res => res.Pilot.Id == pilotId);
    }
}
