namespace Veloci.Data.Domain;

public static class SeasonResultExtensions
{
    public static SeasonResult? GetByPlace(this IEnumerable<SeasonResult> seasonResults, int rank)
    {
        return seasonResults.SingleOrDefault(res => res.Rank == rank);
    }
}