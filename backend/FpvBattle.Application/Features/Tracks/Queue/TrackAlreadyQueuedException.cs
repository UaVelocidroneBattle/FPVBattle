namespace FpvBattle.Application.Features.Tracks.Queue;

/// <summary>A track may wait in the queue only once: the queue of any cup already holds it.</summary>
public class TrackAlreadyQueuedException() : Exception(DefaultMessage)
{
    public const string DefaultMessage = "This track is already scheduled";
}
