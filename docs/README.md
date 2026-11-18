# FPV Battle Documentation

How FPV Battle works, one feature per file. For what the project is, see the [README](../README.md); for architecture and coding conventions, see [CLAUDE.md](../CLAUDE.md) and [`.claude/conventions/`](../.claude/conventions/).

## Features

| Doc | Covers |
|---|---|
| [competitions.md](competitions.md) | Cups, the daily cycle, track selection and rating, points, seasons |
| [quad-of-the-day.md](quad-of-the-day.md) | How the day's quad is chosen and how it changes scoring |
| [day-streaks.md](day-streaks.md) | Day streaks, freezies, admin freezes and warnings |
| [leagues.md](leagues.md) | Pace rating (Global Rating) and monthly leagues |
| [achievements.md](achievements.md) | How achievements are checked, and the full catalogue |
| [teams.md](teams.md) | Teams, roles, invites, join requests, membership history and team competition |
| [accounts.md](accounts.md) | Google sign-in, fly-to-verify pilot linking, account deletion |
| [notifications.md](notifications.md) | The in-app notification bell |
| [bots.md](bots.md) | What the Telegram and Discord bots post, and the Telegram commands |
| [patreon.md](patreon.md) | Patreon sync, patron announcements and freezie rewards |

## Architectural decision records

| ADR | Decision |
|---|---|
| [0001](adr/0001-hangfire-for-background-jobs.md) | Hangfire for background jobs |
| [0002](adr/0002-sqlite-for-data-storage.md) | SQLite for data storage |
| [0003](adr/0003-react-spa-with-zustand-and-openapi-client.md) | React SPA with Zustand and a generated OpenAPI client |
| [0004](adr/0004-removed-vote-reminder.md) | Removed the vote reminder |

To record a new decision, copy [adr/template.md](adr/template.md) to the next number (`0005-kebab-case-title.md`).

## Keeping docs current

- Update a feature's doc together with the code that changes its behaviour.
- Describe rules and behaviour, and name the classes that own them, so a reader can jump to the code.
- Add a row to the table above when adding a feature doc.
