using System.Globalization;

namespace FpvBattle.Application.Helpers;

public static class TrackTimeConverter
{
    public static string MsToSec(int ms) => (ms / 1000.0).ToString(CultureInfo.InvariantCulture);
}
