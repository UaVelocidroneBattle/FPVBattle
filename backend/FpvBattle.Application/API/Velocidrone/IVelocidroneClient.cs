using FpvBattle.Application.API.Velocidrone.Dto;

namespace FpvBattle.Application.API.Velocidrone;

public interface IVelocidroneClient
{
    Task<ICollection<TrackTimeDto>> LeaderboardAsync(int trackId, CancellationToken cancellationToken = default);

    Task<ICollection<ModelDto>> ModelsAsync(CancellationToken cancellationToken = default);

    Task<ICollection<TrackDto>> TracksAsync(CancellationToken cancellationToken = default);
}
