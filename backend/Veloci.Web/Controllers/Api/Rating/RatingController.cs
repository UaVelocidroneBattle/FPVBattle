using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Features.Leagues.Models;
using Veloci.Logic.Features.Leagues.Services;

namespace Veloci.Web.Controllers.Api.Rating;

[ApiController]
[Route("/api/ratings/[action]")]
public class RatingController : ControllerBase
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private readonly RatingService _ratingService;
    private readonly RatingQualificationService _qualificationService;
    private readonly ICupService _cupService;
    private readonly IMemoryCache _cache;

    public RatingController(
        RatingService ratingService,
        RatingQualificationService qualificationService,
        ICupService cupService,
        IMemoryCache cache)
    {
        _ratingService = ratingService;
        _qualificationService = qualificationService;
        _cupService = cupService;
        _cache = cache;
    }

    [HttpGet("/api/ratings/get")]
    public async Task<ActionResult<RatingModel>> GetRating([FromQuery] string cupId)
    {
        var cacheKey = $"rating-{cupId}";

        if (_cache.TryGetValue(cacheKey, out RatingModel? cached))
            return cached!;

        var model = await BuildRatingModelAsync(cupId);

        if (model is null)
            return NotFound();

        _cache.Set(cacheKey, model, CacheDuration);

        return model;
    }

    private async Task<RatingModel?> BuildRatingModelAsync(string cupId)
    {
        var ratings = await _ratingService.GetRatingsForCupAsync(cupId);

        if (ratings.Count == 0)
            return null;

        var leagueOptions = _cupService.GetCupOptions(cupId).Leagues;
        var previousRatings = await _ratingService.GetPreviousRatingsForCupAsync(cupId);
        var currentPilotIds = ratings.Select(r => r.PilotId).ToHashSet();

        var qualifications = leagueOptions.Enabled
            ? await _qualificationService.GetForCupAsync(cupId)
            : null;

        return new RatingModel
        {
            CalculatedOn = ratings[0].CalculatedOn,
            RequiredTracks = qualifications?.RequiredTracks,
            Ratings = ratings.Select(r => ToPilotRatingModel(r, cupId, qualifications)).ToList(),
            DroppedOutPilots = previousRatings
                .Where(r => !currentPilotIds.Contains(r.PilotId))
                .Select(r => ToPilotRatingModel(r, cupId, qualifications))
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

    private static PilotRatingModel ToPilotRatingModel(PilotPaceRating r, string cupId, CupRatingQualifications? qualifications) => new()
    {
        PilotId = r.PilotId,
        PilotName = r.Pilot.Name,
        Country = r.Pilot.Country,
        AverageGapPercent = r.AverageGapPercent,
        AverageGapChange = r.AverageGapChange,
        Rank = r.Rank,
        RankChange = r.RankChange,
        League = r.Pilot.GetCurrentLeague(cupId),
        Qualification = qualifications?.For(r.PilotId)
    };
}
