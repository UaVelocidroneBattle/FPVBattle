namespace Veloci.Data.Domain;

public static class CompetitionResultsExtensions
{
    public static CompetitionResults? GetByPilotId(this IEnumerable<CompetitionResults> competitionResults, int pilotId)
    {
        return competitionResults.FirstOrDefault(res => res.Pilot.Id == pilotId);
    }
}
