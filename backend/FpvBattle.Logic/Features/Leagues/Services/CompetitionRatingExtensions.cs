using FpvBattle.Domain;

namespace FpvBattle.Logic.Features.Leagues.Services;

public static class CompetitionRatingExtensions
{
    extension(Competition competition)
    {
        public IEnumerable<CompetitionResults> RatingEligibleResults =>
            competition.CompetitionResults.ForQuadOfTheDay(competition.QuadOfTheDay);
    }
}
