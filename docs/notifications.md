# Notifications

Signed-in users get personal notifications in the bell in the dashboard header. The bell adds to the chat announcements: it tells one user about things that concern them.

## What sends one

| Event | Handler |
|---|---|
| The user's pilot earned an [achievement](achievements.md) | `InAppAchievementsHandler` |
| The user's pilot changed [league](leagues.md) | `InAppLeagueHandler` |
| The user was invited to a [team](teams.md) | `InAppTeamInviteHandler` |
| A join request arrived at the user's team, or the user's request was approved | `InAppTeamJoinRequestHandler` |

Pilot-based events reach only pilots linked to an account (see [accounts.md](accounts.md)).

## How it works

- Handlers build `UserNotificationMessage`s, or `PilotNotificationMessage`s that resolve to the linked user, and hand them to `UserNotificationService`.
- The service delivers them through every `INotificationChannel`. Today that is only `InAppNotificationChannel`, which stores `UserNotification` rows. A new channel, such as email or a Telegram DM, plugs in as another implementation.
- The API (`/api/notifications`) lists notifications, returns the unread count, and marks one or all as read.
- **Cleanup old notifications** (daily 05:01 UTC) deletes read notifications after 30 days and unread ones after 90 days.
- Admins can browse all users' notifications on the User Notifications page.
