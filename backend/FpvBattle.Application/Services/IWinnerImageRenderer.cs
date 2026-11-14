namespace FpvBattle.Application.Services;

public interface IWinnerImageRenderer
{
    Task<byte[]> CreateWinnerImageAsync(string season, string[] winners);
}
