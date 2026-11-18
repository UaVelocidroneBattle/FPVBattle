# Achievements

Achievements are badges pilots earn for milestones and memorable moments. New achievements are announced in Telegram and Discord and appear in the pilot's notification bell. All of a pilot's badges show on their profile, and the guide's Achievements page lists every one.

## How they work

Each achievement is a class in `Features/Achievements/Collection` implementing `IAchievement` (`Name`, `Title`, `Description` and an optional `CupId` for cup-specific ones). The interface it extends decides when it is checked:

| Interface | Checked on | Event |
|---|---|---|
| `IAchievementAfterTimeUpdate` | every new or improved time | `CurrentResultUpdated` |
| `IAchievementAfterCompetition` | the end of each competition | `CompetitionFinished` |
| `IAchievementAfterSeason` | the end of each monthly season | `SeasonFinished` |
| `IAchievementAfterLeagueUpdate` | the monthly league update | `LeagueUpdateNotification` |
| `IGlobalAchievement` | the end of each competition; held by one pilot at a time | `CompetitionFinished` |

`AchievementsEventHandler` routes the events to `AchievementService`, which awards each achievement once per pilot and publishes `GotAchievements`. The Telegram, Discord and in-app handlers announce it. Achievement classes are discovered by reflection (`AchievementsServiceExtensions`), so adding one is a matter of adding the class.

## Catalogue

**Day streak**: The Beginning (10 days), 20, Fifty (50), Fanatic (75), Fifty-Fifty (100), Addicted (150), Cardinal (200), Consistency (250), Tractor Driver (300), Year (365), Outstanding (500), Living Legend (1000). President goes to the pilot with the longest streak ever.

**Times**
- Beast: fly a track in 66.6s.
- Jackpot: a time with identical digits, e.g. 77.777.
- Nano Boost: improve a time by 0.01s or less.
- Flash: the first result of the day.
- Early Bird / Night Owl: post a time between 4–8 AM or 1–4 AM, in the pilot's local time based on their country.
- Universal Soldier: fly both cups on the same day.

**Races**
- GOAT: first place on the track's global Velocidrone leaderboard.
- Longest Ride: the slowest time in a race.
- Per-league race podiums: e.g. "Gold Track Winner", "Gold Runner-Up", "Gold Third Place".
- Whoop class race podiums.

**Seasons**
- Per-league season podiums: e.g. "Gold Champion".
- Whoop class season podiums.
- Haunter: didn't miss a day of the season, separately for open and whoop class.

**Leagues**
- Ranked: placed in a league for the first time.
- Level Up: promoted.
- Demoted: moved down a league.
- Dropped: lost the league place.
