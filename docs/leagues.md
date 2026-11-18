# Pace Rating and Leagues

Leagues group pilots of similar speed so that everyone has a race to win, not only the fastest few. A pilot's league comes from their pace rating.

## Pace rating (Global Rating)

The pace rating measures how far behind the best pilots someone is on average, as a percentage (`PaceRatingCalculator`, settings in `PaceRating`).

1. For every closed competition in the look-back window (`LookBackPeriod`, currently 1 month), the reference time is the average of the top 3 times (`TopPilotsForReference`).
2. Each pilot's gap that day is `(time − reference) / reference`. A pilot faster than the top-3 average has a negative gap.
3. A pilot's rating is the average of their daily gaps, after dropping their worst day (`DropWorstDaysCount`).
4. Only pilots who flew at least `MinDaysForRelevance` days in the window (currently 15) get a rating. Ratings are ranked, and each stores its change since the previous calculation.

It runs weekly (Mondays 03:00 UTC) and again just before the monthly league update. On a [Quad of the Day](quad-of-the-day.md), only times on the named quad count.

The Global Rating page shows the ratings. During the month, it also shows how close each pilot is to qualifying (`RatingQualificationService`): Qualified, Reachable (still possible with the days left) or Out of reach.

## Leagues

On the 1st of each month (00:07 UTC, `LeagueService.UpdatePilotLeaguesAsync`), every cup with leagues enabled recalculates the ratings and fills its leagues in rating order. Leagues have different sizes. For instance:

| League | Size |
|---|---|
| Platinum | 7 |
| Gold | 12 |
| Silver | 17 |
| Bronze | everyone else with a rating (`Size: 0`) |

Pilots without a rating are in the "others" group (`OthersName`, shown as "Unranked"). League names, sizes, order and colours are configured per cup in `CupOptions.Leagues`.

- A league change adds a new `PilotLeague` row and marks the old one historical, so each past competition can still resolve the league a pilot was in on that day (`Pilot.GetCurrentLeague`).
- A pilot who loses their rating gets a dated "no league" row and drops out.
- Changes publish `LeagueUpdateNotification`, which is announced in the chats and the pilot's notification bell. The changes also award the Ranked, Level Up, Demoted and Dropped [achievements](achievements.md).

## Effect on competitions

With leagues enabled, daily points and season standings are calculated **per league**. Each league has its own leaderboard, winner and full points table (see [competitions.md](competitions.md#points)). Season standings use the league a pilot was in when the season started.
