# Patreon

FPV Battle is supported through Patreon. The integration recognises patrons in the chats and rewards them with [freezies](day-streaks.md#freezies).

## Sync

**Patreon sync** (daily 09:00 UTC, `PatreonSyncJob`) reads the campaign members through the Patreon API (`IPatreonApiClient`, OAuth tokens refreshed by `IPatreonTokenManager`) and stores them as `PatreonSupporter` rows: name, email, tier, amount and status.

- A supporter is **paying** when their status is `active_patron` and the amount is above zero.
- The first time a supporter is seen paying, `FirstPaidAt` is recorded and `NewPatreonSupportersNotification` thanks them in Telegram and Discord. Each patron is announced only once, and never before they have actually paid.
- New supporters are linked to a pilot automatically when their Patreon email matches a user account that has a linked pilot (`PatreonUserMatcher`). Admins can also link them by hand on the Patrons page.

## Rewards and announcements

| Job | When | What |
|---|---|---|
| Accrue freezies to patrons | 1st of the month, 10:08 UTC | Adds freezies to each paying patron's pilot by tier: Level 1 → 1, Level 2 → 3, Level 3 → 5, Level 4 → 10, Level 5 → 20. Then announces the list |
| Patreon supporters | Mondays 10:05 UTC | Posts the list of current supporters in the chats |

The guide's Support page links to the Patreon campaign. Patrons can also create [teams](teams.md) without meeting the race-days requirement, when `AllowPatronsToCreate` is on.
