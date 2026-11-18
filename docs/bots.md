# Telegram and Discord Bots

The chats are where the competition lives. Each cup posts to its own Telegram channel and Discord channel (`CupOptions.Channels`).

## What gets posted

`TelegramMessageEventHandler` and `DiscordMessageEventHandler` turn domain events into chat messages. Each platform has its own composer for formatting (`TelegramMessageComposer`, `DiscordMessageComposer`).

| Event | Message |
|---|---|
| `CompetitionStarted` | The new track. On Discord, also the track rating poll |
| `CurrentResultUpdated` | New and improved times as they come in |
| `IntermediateCompetitionResult` | The current leaderboard, every 2 hours when it changed |
| `CheerUp` | A nudge when nobody, or only one pilot, has flown yet |
| `CompetitionFinished` | Final results and points |
| `TempSeasonResults` / `SeasonFinished` | Season standings so far / the final podium |
| `NewPilot`, `PilotRenamed` | Welcome and rename notices |
| `DayStreakPotentialLose` | Evening reminder for pilots about to lose a streak |
| `FreezieAdded` | Freezies granted |
| `BadTrack`, `TrackRestart`, `CompetitionCancelled` | Track problems and replacements |

Achievements, league changes and Patreon events are posted by their own feature handlers (e.g. `TelegramAchievementsHandler`, `DiscordLeagueHandler`, `TelegramPatreonHandler`).

The platform SDKs stay in Infrastructure, behind the `ITelegramMessenger` and `IDiscordBotFactory` ports.

## Telegram commands

Commands implement `ITelegramCommand` (`Bot/Telegram/Commands`). Each command has keywords, a description shown by `/help`, and flags for whether it is public and whether its reply deletes itself after a delay.

| Command | Does |
|---|---|
| `/help`, `/?` | Lists the commands |
| `/current-track`, `/ct` | The current track |
| `/pilot {name}`, `/p` | A pilot's profile |
| `/achievements {name}`, `/ach` | A pilot's achievements |
| `/connect {platform} ...`, `/list-pilot-platforms` | Link a pilot to their Telegram or Discord username (`PilotPlatformAccount`) |
