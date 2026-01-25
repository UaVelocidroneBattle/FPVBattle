using Veloci.Data.Domain;

namespace Veloci.Mcp.Dto;

public class CompetitionDto
{
    public required string Id { get; set; }
    public required DateTime StartedOn { get; set; }
    public required CompetitionState State { get; set; }
    public required string MapName { get; set; }
    public required string TrackName { get; set; }
    public required int TrackId { get; set; }
    public required int MapId { get; set; }

    public static CompetitionDto FromEntity(Competition competition)
    {
        return new CompetitionDto
        {
            Id = competition.Id,
            StartedOn = competition.StartedOn,
            State = competition.State,
            MapName = competition.Track.Map.Name,
            TrackName = competition.Track.Name,
            TrackId = competition.Track.TrackId,
            MapId = competition.Track.Map.MapId
        };
    }
}
