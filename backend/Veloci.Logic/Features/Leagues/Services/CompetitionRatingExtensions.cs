using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Leagues.Services;

public static class CompetitionRatingExtensions
{
    extension(Competition competition)
    {
        public IEnumerable<CompetitionResults> RatingEligibleResults =>
            competition.QuadOfTheDay is null
                ? competition.CompetitionResults
                : competition.CompetitionResults.Where(r => r.ModelName == competition.QuadOfTheDay.Name);
    }
}
