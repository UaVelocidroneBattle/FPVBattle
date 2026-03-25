using Veloci.Data.Domain;

namespace Veloci.Mcp.Dto;

public class CompetitionResultsDto
{
    public required string PilotName { get; set; }
    public required int TrackTime { get; set; }
    public required int LocalRank { get; set; }
    public required int GlobalRank { get; set; }
    public required int Points { get; set; }
    public string? ModelName { get; set; }

    public static CompetitionResultsDto FromEntity(CompetitionResults result) => new()
    {
        PilotName = result.Pilot.Name,
        TrackTime = result.TrackTime,
        LocalRank = result.LocalRank,
        GlobalRank = result.GlobalRank,
        Points = result.Points,
        ModelName = result.ModelName
    };
}