# FPV Battle

FPV Battle is a competition service for [Velocidrone](https://www.velocidrone.com/) simulator: a web dashboard with statistics, plus Telegram and Discord bots running daily community competitions.

Every day the service picks a new track and announces it in the chat channels. Pilots fly it whenever they like during the day; FPV Battle watches the results, posts leaderboard updates throughout the day, and closes the competition at midnight with final standings and points.

## Features

### Competitions
- **Daily track** — a new track every day, automatic result tracking, periodic leaderboard updates in the chats, and final results with points based on placement.
- **Cups** — independent competition classes (e.g. open class, whoop), each with its own tracks, channels, and configuration.
- **Quad of the Day** — an occasional challenge where the daily competition designates a specific quad model, picked at random or dictated by the track itself. Pilots flying the designated quad compete for full points; anyone on a different quad scores a single point.

### Pilot motivation
- **Daystreaks** — the number of consecutive days a pilot has flown. Every 30 days of streak earns a "freezie", which automatically saves the streak on a missed day.
- **Achievements** — badges for milestones like streak lengths and race placements, announced in the chats.
- **Leagues & pace rating** — pilots are ranked by their pace and grouped into leagues per cup, with promotions and relegations announced as they happen.

### Web dashboard
- **Statistics** — leaderboards, global rating, daystreak standings, track history, pilot comparison charts, and a results heatmap.
- **Pilot profiles** — per-pilot pages with race history, streaks, and achievements.
- **User accounts** — sign in with Google and link your account to your Velocidrone pilot via a fly-to-verify claim: claim your pilot name, fly the daily track, and the link completes automatically. Accounts can be permanently deleted from the profile page; personal data is removed while pilot race history stays on the leaderboards.
- **Admin tools** — track queue management, user-pilot links, and claim oversight.

### Integrations
- **Discord & Telegram** — announcements and leaderboards on both platforms, plus bot commands in Telegram.
- **Patreon** — supporter sync so patrons get recognized.

## Repository layout

| Folder | Contents |
| --- | --- |
| [`backend/`](backend/) | ASP.NET Core modular monolith (vertical slices), Entity Framework, Hangfire jobs, bot services |
| [`frontend/`](frontend/) | React + TypeScript dashboard (Vite, Zustand, TailwindCSS) |
| [`shared/`](shared/) | OpenAPI specification used to generate the type-safe frontend client |
| [`docs/`](docs/) | Project documentation and architectural decision records |
