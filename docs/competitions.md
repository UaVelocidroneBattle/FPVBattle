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

The `CompetitionConductor` class owns the start, stop and season steps. `ResultsUpdater` runs Update results and `LeaderboardPublisher` runs Publish current leaderboard. They share the `Competition` concurrency lock, so a leaderboard is never built from a half-saved update.

## Track selection

1. **The track queue first.** Admins and patrons schedule tracks into a cup's queue, optionally with a quad, and admins optionally on a date (see [track-scheduling.md](track-scheduling.md)). A track dated for today goes first, then undated tracks in the order they were added (`TrackQueueService`).
2. **Otherwise random**, from the available tracks in the database that pass the cup's `TrackFilter`: allowed scenes (by Velocidrone scene id), allowed or blocked track types, and blocked name patterns. While a cup filters by type, a track of unknown type never passes. A track is skipped when:
   - it was raced in the last 6 months (cancelled competitions don't count),
   - pilots rated it below 3 out of 5, or
   - it is waiting in the track queue.

   These reuse rules live in `TrackReuseRules`.
3. A track with no results on Velocidrone yet is skipped, with up to 10 attempts.

Scheduling takes the track picked from the suggestions, by its id, so tracks that share a name stay distinct. An admin may schedule a track outside the cup filter, badly rated or raced in the last 6 months: the Track Queue page suggests tracks as you type and shows those issues as warnings, along with when the track was last raced. An admin can change the day's track through the MCP server (`change_track`), see [Restarting the track](#restarting-the-track).

### Restarting the track

A conductor can restart the running track from "Restart track" in the user menu (the mobile menu too): a confirmation picks the cup, starting with the one on screen. It works in every cup. The menu item, the MCP `change_track` tool and the Telegram restart phrase all go through `TrackRestartService`: it announces the restart and queues `CompetitionConductor.StartNewAsync`, which cancels the running competition (its times earn no points, so pilots fly the new track to keep their day) and picks the next track the same way the daily start does, the queue first. When no competition is running, MCP and Telegram simply start one, which is how a failed start is recovered; the conductors' endpoint answers 409 instead, so a conductor can only replace a running track. The new-track job is queued before the announcement, so a chat that fails to post can't stop the restart. A disabled cup is refused. The Telegram and Discord messages add "Restarted by" with the conductor's pilot name, never the account's own name; MCP and Telegram restarts name nobody. A second restart of the same cup within 5 minutes (a double click, or another conductor first) is refused and changes nothing: the API answers 409, and Telegram replies that the track was just changed.

A conductor is a user with the `Conductor` role. The `CanRestartTrack` policy guards `POST /api/competitions/restart-track`, and it checks the role in the database (`StoredRoleRequirement`), so removing the role works at once rather than when the access token expires. The current-user response carries `canRestartTrack`, which decides whether the menu item shows. Admins give and remove roles from each user's menu on the admin Users page, which lists every role that exists. The `AddConductorRole` migration creates the role.

### Track and scene lookups

Tracks and scenes are stored in the database (`Track`, `Scene`), keyed by their Velocidrone ids (`Scene.SceneId`, `Track.TrackId`; `Track.SceneId` is the foreign key to `Scene.Id`). `TrackLookupSync` is the only code that creates them. Each run it reads Velocidrone's scene list and official track list and:

- adds new scenes and tracks, and updates changed names, track types and scenes;
- creates a placeholder scene named `Scene <id>` for a track whose scene is missing from the scene list;
- soft deletes (`IsDeleted`) scenes and tracks Velocidrone no longer returns, and restores them when they come back. Competitions and raced queue entries keep pointing at deleted tracks, so history still loads them, but `Available()` hides them from track selection. A deleted track that is still waiting in the queue is removed from it;
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
