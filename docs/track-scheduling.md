# Track Scheduling

Admins and paying patrons can pick tracks for the daily competition. A scheduled track joins the cup's [track queue](competitions.md#track-selection) and becomes the daily track when its turn comes. Admins schedule on the Track Queue admin page, patrons with the "Schedule a track" button on a cup's page. The guide's Track Scheduling page (`/guide/track-scheduling`, in English and Ukrainian) explains it to pilots.

## One flow, two sets of rules

`TrackSchedulingService` suggests, schedules and cancels the same way for everyone. Every track gets the same checks, and the scheduler's `SchedulingRules` decide which of them block:

| Check | Rule | Admin | Patron |
|---|---|---|---|
| Cup filter | The track passes the cup's `TrackFilter` | Warning | Blocks, hidden from suggestions |
| Already scheduled | The track is waiting in the queue of any cup | Blocks | Blocks |
| Rating | Pilots rated it below 3 | Warning | Blocks |
| Raced recently | Raced in any cup in the last 6 months. Cancelled competitions don't count | Warning | Blocks |

Suggestions start after 2 letters and show at most 40 tracks, names starting with the typed text first (`TrackSearch`, run in the database). A blocked track stays in the suggestions with its reason, but can't be picked. The server checks everything again when a track is scheduled. The rating and 6-month rules are in `TrackReuseRules`, which the random pick uses too.

The same for everyone:

- Only enabled cups can be scheduled.
- The quad is "any", or one of the cup's [Quad of the Day](quad-of-the-day.md) pool (`QuadOfTheDay.Quads`). A cup with Quad of the Day turned off offers only "any". Picking a quad makes that day a Quad of the Day.
- A date is optional and must be free in that cup. The patron dialog sends none, so a patron's track joins the end of the line.
- Cancelling works on a track that hasn't been raced yet. Patrons see and cancel only their own tracks, on the Profile page after a confirmation; their API checks the owner before cancelling.

## Patron quota

A patron needs a linked pilot (see [accounts.md](accounts.md)) with a paying Patreon supporter: status `active_patron` and an amount above zero (see [patreon.md](patreon.md)). Free-tier members can't schedule. Each patron has a monthly limit, set per Patreon tier in configuration:

```json
"TrackScheduling": {
  "Enabled": true,
  "MonthlyLimitByTier": { "Level 1": 1, "Level 2": 2, "Level 3": 3, "Level 4": 4, "Level 5": 5 }
}
```

- A tier missing from the list gets no tracks.
- The limit counts the tracks a patron added in the current UTC calendar month, across all cups, including ones already raced.
- Cancelling a track before it's raced frees its slot. So does an admin removing it from the queue, or the sync removing a track Velocidrone dropped.
- `Enabled: false` hides the feature.

`PatronQuotaPolicy` works out the quota and, when the user can't schedule, the one reason why. The site shows that reason on the disabled button.

## In the queue

A scheduled track is an ordinary `QueuedTrack`. A patron's track records the patron in `ScheduledByUserId`; an admin's has none. `TrackQueueService` keeps the line: at the cup's start it takes a track dated for today first, otherwise the oldest undated track. A track can wait in the queue only once, across all cups: a unique index on waiting entries enforces it, even for two requests at the same moment.

- The random pick skips tracks waiting in the queue, so a scheduled track isn't raced before its turn.
- When Velocidrone stops returning a waiting track, the nightly sync removes it from the queue, and the patron gets the slot back.
- The queue position shown to the patron counts the undated tracks ahead in that cup. Dated tracks can push it back a day.
- The queue isn't public. A patron sees only their own tracks, on the Profile page.
- The admin Track Queue page shows who scheduled each track.
- When a patron's track becomes the daily track, the Telegram and Discord announcements add "Picked by" and the patron's pilot name. Admin and random picks don't get the line.

Deleting an account keeps the user's waiting tracks in the queue, as if an admin had queued them.
