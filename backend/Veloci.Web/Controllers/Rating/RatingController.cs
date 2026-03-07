using Microsoft.AspNetCore.Mvc;
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

        return new RatingModel
        {
            CalculatedOn = ratings[0].CalculatedOn,
            Ratings = ratings.Select(r => new PilotRatingModel
            {
                PilotId = r.PilotId,
                PilotName = r.Pilot.Name,
                Country = r.Pilot.Country,
                AverageGapPercent = r.AverageGapPercent,
                AverageGapChange = r.AverageGapChange,
                Rank = r.Rank,
                RankChange = r.RankChange,
            }).ToList()
        };
    }
}
