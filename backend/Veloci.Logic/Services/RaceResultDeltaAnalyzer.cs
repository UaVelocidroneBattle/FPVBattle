using Veloci.Data.Domain;

namespace Veloci.Logic.Services;

public class RaceResultDeltaAnalyzer
{
    public List<TrackTimeDelta> CompareResults(TrackResults a, TrackResults b)
    {
        var deltas = new List<TrackTimeDelta>();

        foreach (var trackTime in b.Times)
        {
            var existingTime = a.Times.FirstOrDefault(t => t.UserId == trackTime.UserId);

            if (existingTime is null)
            {
                deltas.Add(new TrackTimeDelta
                {
                    PilotId = trackTime.UserId.Value,
                    LocalRank = trackTime.LocalRank,
                    Rank = trackTime.GlobalRank,
                    TrackTime = trackTime.Time,
                    ModelName = trackTime.ModelName
                });

                continue;
            }

            if (existingTime.Time == trackTime.Time)
                continue;

            deltas.Add(new TrackTimeDelta
            {
                PilotId = trackTime.UserId.Value,
                LocalRank = trackTime.LocalRank,
                LocalRankOld = existingTime.LocalRank,
                Rank = trackTime.GlobalRank,
                RankOld = existingTime.GlobalRank,
                TrackTime = trackTime.Time,
                TimeChange = trackTime.Time - existingTime.Time,
                ModelName = trackTime.ModelName
            });
        }

        return deltas;
    }
}
