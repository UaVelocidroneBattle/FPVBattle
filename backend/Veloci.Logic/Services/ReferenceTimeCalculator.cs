namespace Veloci.Logic.Services;

public static class ReferenceTimeCalculator
{
    public static double? GetTopPilotsAverageTime(IEnumerable<int> trackTimes, int topPilotsForReference, int minimumCount)
    {
        var topTimes = trackTimes
            .OrderBy(t => t)
            .Take(topPilotsForReference)
            .ToList();

        return topTimes.Count < minimumCount
            ? null
            : topTimes.Average();
    }

    public static double GapPercent(int pilotTime, double referenceTime)
        => (pilotTime - referenceTime) / referenceTime * 100.0;
}
