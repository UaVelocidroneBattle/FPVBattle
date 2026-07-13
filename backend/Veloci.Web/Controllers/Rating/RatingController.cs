using Microsoft.AspNetCore.Mvc;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Features.Leagues.Services;

namespace Veloci.Web.Controllers.Rating;

[ApiController]
[Route("/api/ratings/[action]")]
public class RatingController : ControllerBase
{
    private readonly RatingService _ratingService;
    private readonly ICupService _cupService;

    public RatingController(RatingService ratingService, ICupService cupService)
    {
        _ratingService = ratingService;
        _cupService = cupService;
    }

    [HttpGet("/api/ratings/get")]
    public async Task<ActionResult<RatingModel>> GetRating([FromQuery] string cupId)
    {
        var ratings = await _ratingService.GetRatingsForCupAsync(cupId);

        if (ratings.Count == 0)
            return NotFound();

        var leagueOptions = _cupService.GetCupOptions(cupId).Leagues;
        var previousRatings = await _ratingService.GetPreviousRatingsForCupAsync(cupId);
        var currentPilotIds = ratings.Select(r => r.PilotId).ToHashSet();

        return new RatingModel
        {
            CalculatedOn = ratings[0].CalculatedOn,
            Ratings = ratings.Select(r => ToPilotRatingModel(r, cupId)).ToList(),
            DroppedOutPilots = previousRatings
                .Where(r => !currentPilotIds.Contains(r.PilotId))
                .Select(r => ToPilotRatingModel(r, cupId))
                .ToList(),

            LeagueSettings = new LeagueSettingsModel
            {
                Enabled = leagueOptions.Enabled,
                OthersName = leagueOptions.OthersName,
                Descriptors = leagueOptions.Definitions.Select(x => new LeagueDescriptorModel
                {
                    Name = x.Name,
                    Size = x.Size,
                    Order = x.Order,
                    Color = x.Color
                })
                .ToList()
            }
        };
    }

    private static PilotRatingModel ToPilotRatingModel(PilotPaceRating r, string cupId) => new()
    {
        PilotId = r.PilotId,
        PilotName = r.Pilot.Name,
        Country = r.Pilot.Country,
        AverageGapPercent = r.AverageGapPercent,
        AverageGapChange = r.AverageGapChange,
        Rank = r.Rank,
        RankChange = r.RankChange,
        League = r.Pilot.GetCurrentLeague(cupId)
    };
}
