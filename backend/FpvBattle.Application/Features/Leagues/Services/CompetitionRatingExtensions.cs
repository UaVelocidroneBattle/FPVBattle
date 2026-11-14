using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Leagues.Services;

public static class CompetitionRatingExtensions
{
    extension(Competition competition)
    {
        public IEnumerable<CompetitionResults> RatingEligibleResults =>
            competition.CompetitionResults.ForQuadOfTheDay(competition.QuadOfTheDay);
    }
}
