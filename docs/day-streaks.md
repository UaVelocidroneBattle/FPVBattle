# Day Streaks and Freezies

A day streak counts how many days in a row a pilot has flown. It exists to bring pilots back every day.

## Counting

- A day counts when the pilot posts a new or improved time in **any** enabled cup. The streak is shared across cups by design: flying open class or whoop class is enough.
- The nightly job **Update pilots day streaks** (00:05 UTC, `PilotService.UpdatePilotDayStreaksAsync`) runs `Pilot.OnRaceFlown` for everyone who flew the previous day: the streak grows, `MaxDayStreak` and `TotalRaceDays` follow. The same job resets every pilot who missed the day (`ResetDayStreaksAsync`).
- The streak logic lives on the `Pilot` entity and is covered by `DayStreakTests`.

## Freezies

A freezie (`DayStreakFreeze`) saves the streak on a missed day. When a pilot misses a day and has a spare freezie, the oldest one is spent on that day (`SpentOn`) instead of resetting the streak. Several missed days use one freezie each.

Pilots get freezies in two ways:
- **One for every 30 streak days** (`Pilot.AddFreezie`), whenever the streak reaches a multiple of 30.
- **Monthly for patrons**, according to their Patreon tier (see [patreon.md](patreon.md)).

## Admin freeze

An admin can freeze a pilot's streak until a date (`Pilot.DayStreakFrozenUntil`, set on the Users admin page), for example while the pilot is travelling. Until that date, missed days neither reset the streak nor spend freezies.

## Warnings and records

- **Day streak lose warning** (22:03 UTC) reminds pilots with a streak above 10 who haven't flown today, unless their streak is frozen (`DayStreakPotentialLose`).
- Streak milestones award [achievements](achievements.md): 10, 20, 50, 75, 100, 150, 200, 250, 300, 365, 500 and 1000 days. The pilot with the longest streak ever (`MaxDayStreak`) holds the "President" achievement.
- The Day Streaks statistics page shows the standings. Admins can correct a streak with the MCP tools `set_day_streak`, `recalculate_day_streak` and `add_freezie`.
