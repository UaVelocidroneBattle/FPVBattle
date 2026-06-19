using Microsoft.AspNetCore.Mvc;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Leagues.Services;

namespace Veloci.Web.Controllers.Rating;

[ApiController]
[Route("/api/ratings/[action]")]
public class RatingController : ControllerBase
{
    private readonly RatingService _ratingService;

    public RatingController(RatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpGet("/api/ratings/get")]
    public async Task<ActionResult<RatingModel>> GetRating([FromQuery] string cupId)
    {
        var ratings = await _ratingService.GetRatingsForCupAsync(cupId);

        if (ratings.Count == 0)
            return NotFound();

        var previousRatings = await _ratingService.GetPreviousRatingsForCupAsync(cupId);
        var currentPilotIds = ratings.Select(r => r.PilotId).ToHashSet();

        return new RatingModel
        {
            CalculatedOn = ratings[0].CalculatedOn,
            Ratings = ratings.Select(ToPilotRatingModel).ToList(),
            DroppedOutPilots = previousRatings
                .Where(r => !currentPilotIds.Contains(r.PilotId))
                .Select(ToPilotRatingModel)
                .ToList()
        };
    }

    private static PilotRatingModel ToPilotRatingModel(PilotPaceRating r) => new()
    {
        PilotId = r.PilotId,
        PilotName = r.Pilot.Name,
        Country = r.Pilot.Country,
        AverageGapPercent = r.AverageGapPercent,
        AverageGapChange = r.AverageGapChange,
        Rank = r.Rank,
        RankChange = r.RankChange,
    };
}
