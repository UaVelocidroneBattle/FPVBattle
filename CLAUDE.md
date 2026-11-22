# FPV Battle

FPV Battle runs daily community competitions for the [Velocidrone](https://www.velocidrone.com/) FPV simulator. It is made of a web dashboard with admin tools, plus Telegram and Discord bots.

Every day the service picks a track and announces it in the chat channels. Pilots fly it whenever they like. The bot tracks results, posts leaderboard updates during the day, and closes the competition at the end of the day with final standings and points by placement.

## Features

- **Daily competitions and cups**: a new track every day per cup (open class, whoop class), each cup with its own channels and settings. Tracks and scenes are mirrored nightly from Velocidrone.
- **Track scheduling**: patrons and admins schedule tracks into a cup's queue through one service with per-role rules. Patrons have a monthly limit per Patreon tier, and the announcement names the patron who picked the track.
- **Quad of the Day**: some days name a quad model; pilots flying it compete for full points.
- **Day streaks and freezies**: consecutive flying days. A freezie saves the streak on a missed day. Pilots earn one every 30 streak days, and patrons get them monthly.
- **Achievements, leagues and pace rating**: badges for milestones, plus per-cup leagues with promotions and relegations.
- **Statistics**: leaderboards, global rating, track history, pilot comparison and a results heatmap.
- **User accounts**: Google sign-in (JWT plus rotating refresh tokens). A fly-to-verify claim links an account to a Velocidrone pilot. Deleting an account removes personal data but keeps race history.
- **Teams**: pilots create and join teams. Public teams compete on the daily track with the average time of the members who flew. Team deletion is soft and memberships keep their history.
- **Notifications**: in-app notifications next to the chat announcements.
- **Patreon**: supporter sync, patron announcements, freezie rewards per tier and track scheduling.
- **MCP server**: admin tools for Claude at `/mcp`.

## Repository

| Folder | Contents |
| --- | --- |
| `backend/` | ASP.NET Core 10 solution `FpvBattle.sln`: Domain, Application, Infrastructure, Web, Mcp, Hangfire.Metrics, Tests |
| `frontend/` | React + TypeScript dashboard (Vite, Zustand, TailwindCSS) |
| `shared/api/` | OpenAPI spec `FpvBattle.Web.json`, used to generate the frontend client |
| `docs/` | Feature docs and architectural decision records (`docs/adr/`) |

## Where to read more

| Read | When |
| --- | --- |
| [.claude/conventions/code-style.md](.claude/conventions/code-style.md) | Always: quality bar, SOLID and DRY, comments |
| [.claude/conventions/workflow.md](.claude/conventions/workflow.md) | Always: branches, commits, PRs, docs, common commands |
| [.claude/conventions/backend.md](.claude/conventions/backend.md) | Backend work: layers, ports and adapters, vertical slices, data access, EF, jobs, bots, admin Razor pages |
| [.claude/conventions/frontend.md](.claude/conventions/frontend.md) | Frontend work: components, file layout, API client, Zustand rules |
| [.claude/conventions/testing.md](.claude/conventions/testing.md) | Writing or running tests, architecture tests, frontend build check |
| [docs/README.md](docs/README.md) | How each feature behaves: competitions, track scheduling, quad of the day, day streaks, leagues, achievements, teams, accounts, notifications, bots, Patreon. Read the feature's doc before changing it |
| [docs/adr/](docs/adr/) | Why Hangfire, SQLite and the React SPA were chosen |

`backend/CLAUDE.md` and `frontend/CLAUDE.md` load the backend and frontend conventions automatically when you work in those folders.

@.claude/conventions/code-style.md
@.claude/conventions/workflow.md
