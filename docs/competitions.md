# Daily Competitions

FPV Battle runs one competition per cup each day. A competition is a Velocidrone track that everyone flies during the day. Its leaderboard is the track's own Velocidrone leaderboard, starting from the moment the competition opens.

## Cups

A cup is an independent competition class with its own track pool, chat channels and settings (`Cups:Definitions` in configuration, `CupOptions`). There are two today:

| Cup | Quads |
|---|---|
| `open-class` | any quad |
| `whoop-class` | whoop class quads only (`QuadClasses`) |

Each cup can turn [Quad of the Day](quad-of-the-day.md) and [leagues](leagues.md) on or off on its own. Disabling a cup stops its jobs. Most code takes a `cupId`, and `CupService` / `CupServiceExtensions` resolve its settings and channels.

## The daily cycle

`CupJobRegistrar` registers four recurring jobs per enabled cup, all in UTC and timed relative to the cup's `Schedule.StartTime` (currently `00:03`):

| Time | Job | What happens |
|---|---|---|
| start − 4 min | `StopPollAsync` | Closes the Discord poll for the day's track and stores its rating |
| start − 2 min | `StopAsync` | Closes the competition, stores final results and team results, publishes `CompetitionFinished` |
| start − 1 min | `SeasonResultsAsync` | On the 1st, finalizes last month's season. On other days, posts the season standings so far |
| start | `StartNewAsync` | Picks a track, opens a new competition, publishes `CompetitionStarted` |

Between start and stop, two global jobs (`HangfireInit`) drive the day:

- **Update results** runs every 10 minutes (when `ResultsUpdateEnabled`). It reads the Velocidrone leaderboard, finds new or improved times (`RaceResultDeltaAnalyzer`), registers new pilots and renames, and publishes `CurrentResultUpdated`. Achievements, pilot claims and the chat updates react to that event.
- **Publish current leaderboard** runs every 2 hours. It posts the leaderboard when something has changed since the last post. When nobody, or only one pilot, has flown, it posts a cheer-up message instead, outside quiet hours (23:00–07:00).

**Sync track lookups** (`TracksJobRegistrar`) runs daily at 23:05 UTC, about an hour before the cups start. See [Track and scene lookups](#track-and-scene-lookups).

The `CompetitionConductor` class owns the start, stop and season steps. `CompetitionService` owns the polling.

## Track selection

1. **The track queue first.** Admins can queue a specific track for a cup, optionally on a date and with a quad (Track Queue admin page, `TrackQueueService`).
2. **Otherwise random**, from the available tracks in the database that pass the cup's `TrackFilter`: allowed scenes (by Velocidrone scene id), allowed or blocked track types, and blocked name patterns. While a cup filters by type, a track of unknown type never passes. A track is skipped when:
   - it was used in the last 6 months, or
   - pilots rated it below 3 out of 5.
3. A track with no results on Velocidrone yet is skipped, with up to 10 attempts.

Queuing a track looks it up by name among the same available, cup-filtered tracks. An admin can change the day's track through the MCP server (`change_track`), which cancels the running competition and starts a new one (`TrackRestart`).

### Track and scene lookups

Tracks and scenes are stored in the database (`Track`, `Scene`), keyed by their Velocidrone ids (`Scene.SceneId`, `Track.TrackId`; `Track.SceneId` is the foreign key to `Scene.Id`). `TrackLookupSync` is the only code that creates them. Each run it reads Velocidrone's scene list and official track list and:

- adds new scenes and tracks, and updates changed names, track types and scenes;
- creates a placeholder scene named `Scene <id>` for a track whose scene is missing from the scene list;
- soft deletes (`IsDeleted`) scenes and tracks Velocidrone no longer returns, and restores them when they come back. Competitions and the track queue keep pointing at deleted tracks, so history still loads them, but `Available()` hides them from track selection;
- skips deletions of a kind when Velocidrone returns fewer than half of the available rows, which guards against a truncated response.

A failed Velocidrone call fails the job, and Hangfire retries it. The sync ignores cup filters: every official track is stored, and the filters apply only when a track is picked.

### Track rating

At the start of each competition, a Discord poll asks pilots to rate the track. When the poll closes, its weighted average becomes the track's rating (`TrackRating`). A rating below 3 publishes `BadTrack`, and the track is never picked at random again.

## Points

When a competition closes, `LeaderboardCalculator` ranks each pilot by their best time and gives points by position (`PointsCalculator`):

| Place | 1 | 2 | 3 | 4 | 5 | 10 | 20 | 30 | 39+ |
|---|---|---|---|---|---|---|---|---|---|
| Points | 100 | 85 | 75 | 67 | 60 | 38 | 20 | 10 | 1 |

- With [leagues](leagues.md) enabled, pilots are ranked and scored within their own league, so every league has its own winner.
- On a [Quad of the Day](quad-of-the-day.md), points go by rank among the pilots who flew that quad. Everyone else scores 1 point.
- Each result also stores a gap to the leader as a percentage of the top-3 average time (`ReferenceTimeCalculator`).

## Seasons

A season is a calendar month per cup. Season standings are the sum of daily points from non-cancelled competitions, grouped by the league each pilot was in when the season started. On the 1st, `SeasonFinished` announces the top three and the season achievements are awarded.

## Competition states

`Started` → `Closed` when the day ends. A competition replaced by a track change or a restart becomes `Cancelled` and doesn't count toward the season.
