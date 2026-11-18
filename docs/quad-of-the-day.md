# Quad of the Day

Some days a competition names one quad model. Pilots who fly that quad compete for full points; everyone else still appears on the leaderboard but scores 1 point. The challenge pushes pilots onto quads they don't usually fly.

## How the quad is chosen

When a competition starts (`QuadOfTheDayService`), in this order:

1. **Queued track**: an admin who queues a track can attach a quad to it.
2. **From the track name**: if the track's name contains one of the cup's quad names (e.g. a track built for "TBS Spec"), that quad is used.
3. **At random**: with the cup's `QuadOfTheDay.Probability` (in percent), one quad from the cup's `QuadOfTheDay.Quads` pool. This never happens two competitions in a row in the same cup.

The feature is configured per cup (`CupOptions.QuadOfTheDay`). It is currently on for open class and off for whoop class.

## Scoring

- Each pilot's result is their best time on the named quad. If they never flew it, their overall best time counts instead (`LeaderboardCalculator.SelectBestDelta`).
- Points go by rank among the pilots who flew the quad, so a faster pilot on another quad can't push them down a points slot. Pilots on other quads score 1 point. See [competitions.md](competitions.md#points).
- The gap-to-leader reference and the [pace rating](leagues.md) use only the results on the named quad (`ForQuadOfTheDay`, `RatingEligibleResults`). That keeps the day's results comparable to other days.
