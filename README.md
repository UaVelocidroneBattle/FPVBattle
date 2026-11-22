# FPV Battle

**FPV Battle** is a daily racing league for pilots of the [Velocidrone](https://www.velocidrone.com/) FPV drone simulator. Live at [fpv-battle.fun](https://fpv-battle.fun).

Every day FPV Battle picks a new track and announces it in the community's Telegram and Discord channels. Pilots fly it whenever it suits them during the day; there is no fixed race time and no lobby. The service watches the track's Velocidrone leaderboard, posts new times and the leaderboard to the chats as the day goes on, and at the end of the day closes the race, awards points and crowns the winners. Points add up into monthly seasons.

The goal is simple: give pilots a reason to fly every day, a race they can win at their level, and a community to share it with.

## Features

### Daily racing
- **A new track every day** in each cup, chosen from Velocidrone's official tracks, mirrored nightly into the database. Recently used tracks and tracks the community rated poorly are skipped.
- **Track scheduling**: patrons pick tracks for the daily competition, up to a monthly limit per Patreon tier. Admins can schedule tracks too, optionally on a date.
- **Cups**: separate competitions for open class and whoop class, each with its own track pool and channels.
- **Points by placement**: 100 for first, 85 for second, and so on down the field. Every pilot who flies scores.
- **Monthly seasons** with running standings and a podium on the 1st.
- **Quad of the Day**: some days name a quad model; only pilots who fly it compete for full points.
- **Track rating polls**: pilots rate each track on Discord, and badly rated tracks leave the rotation.

### Keeping pilots coming back
- **Day streaks**: a counter of consecutive days flown, with warnings in the evening before a streak is lost.
- **Freezies** save a streak on a missed day. Pilots earn one every 30 streak days, and patrons get them monthly.
- **Achievements**: badges for streak milestones, podiums, perfect-digit times, night flights and more, announced in the chats.
- **Global Rating**: a pace rating that measures how far behind the fastest pilots someone is on average, recalculated weekly.
- **Leagues**: each month pilots are placed into Platinum, Gold, Silver and Bronze by their rating. Each league has its own daily leaderboard and season, with promotions and relegations announced.

### Community
- **Teams**: pilots form teams, invite friends or ask to join. Public teams compete daily on their members' average time.
- **Telegram and Discord bots** post every step of the day, and Telegram commands look up tracks, pilots and achievements.
- **Patreon**: supporters are thanked in the chats, receive freezies every month and can schedule daily tracks.

### Web dashboard
- **Live competition page** for each cup, with league and team leaderboards.
- **Statistics**: global rating, day streak standings, track history, pilot counts and participation, countries, and pilot comparison charts.
- **Pilot profiles** with race history, streaks, leagues, achievements and team.
- **Accounts**: sign in with Google, link your pilot by flying a race with it, get personal notifications. Accounts can be deleted at any time; race history stays on the leaderboards.
- **Guide** to the rules, in English and Ukrainian.
- **Admin tools**: track queue, users and pilot links, pilot claims, teams, patrons and notifications.

## Learn more

- [docs/](docs/README.md): how each feature works in detail
- [CLAUDE.md](CLAUDE.md): architecture and development conventions

| Folder | Contents |
| --- | --- |
| [`backend/`](backend/) | ASP.NET Core 10 modular monolith: Domain, Application, Infrastructure and Web, with Hangfire jobs, the bots and an MCP server |
| [`frontend/`](frontend/) | React + TypeScript dashboard (Vite, Zustand, TailwindCSS) |
| [`shared/`](shared/) | OpenAPI specification used to generate the frontend API client |
| [`docs/`](docs/) | Feature documentation and architectural decision records |
