using FpvBattle.Domain;

namespace FpvBattle.Mcp.Dto;

public class CompetitionDto
{
    public required string Id { get; set; }
    public required DateTime StartedOn { get; set; }
    public required CompetitionState State { get; set; }
    public required int SceneId { get; set; }
    public required string SceneName { get; set; }
    public required int TrackId { get; set; }
    public required string TrackName { get; set; }

    public static CompetitionDto FromEntity(Competition competition)
    {
        return new CompetitionDto
        {
            Id = competition.Id,
            StartedOn = competition.StartedOn,
            State = competition.State,
            SceneId = competition.Track.Scene.SceneId,
            SceneName = competition.Track.Scene.Name,
            TrackId = competition.Track.TrackId,
            TrackName = competition.Track.Name
        };
    }
}
