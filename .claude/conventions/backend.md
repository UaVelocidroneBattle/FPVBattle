# Backend Conventions

ASP.NET Core 10 modular monolith: a clean-architecture split into projects, with vertical slices inside Application.

## Projects

Each project depends only on the projects above it.

| Project | Role | References |
| --- | --- | --- |
| `FpvBattle.Domain` | Entities as POCOs that carry their own business rules (day streak, freezies, leagues, teams) | none (Identity stores only) |
| `FpvBattle.Application` | Business logic, feature slices, and **ports** (interfaces for everything external) | Domain |
| `FpvBattle.Infrastructure` | **Adapters**: database, Telegram, Discord, Velocidrone, Patreon, Google auth, imaging | Domain, Application |
| `FpvBattle.Mcp` | MCP server tools (`Tools/`), mapped at `/mcp` | Domain, Application |
| `FpvBattle.Hangfire.Metrics` | OpenTelemetry metrics for Hangfire jobs | none |
| `FpvBattle.Web` | Composition root: controllers, admin Razor views, startup | everything |
| `FpvBattle.Tests` | xUnit tests, see [testing.md](testing.md) | everything |

`LayerDependencyTests` enforces these references.

### Ports and adapters

- Application declares an interface for anything external: `IVelocidroneClient`, `ITelegramMessenger`, `IDiscordBotFactory`, `IPatreonApiClient`, `IGoogleTokenValidator`, `IWinnerImageRenderer`. A port's signature uses no third-party SDK types.
- Infrastructure implements the ports, with one folder per external system (`Auth/`, `Discord/`, `Imaging/`, `Patreon/`, `Telegram/`, `Velocidrone/`). Each folder has a registration extension: `AddTelegramBot`, `AddDiscordBot`, `AddVelocidroneClient`, `AddPatreonApi`, `AddTokenAuthentication`, `AddImaging`.
- Telegram, Discord, Google, ImageSharp, RestSharp and ASP.NET Core MVC never appear in Application. `Hangfire.Core` and EF Core are the only infrastructure-flavoured packages allowed there.

## Application layout

```
FpvBattle.Application/
├── Features/          # Vertical slices: Achievements, Auth, Cups, Leagues, Notifications,
│   └── <Feature>/     #   Patreon, PilotBinding, QuadOfTheDay, Teams
│       ├── Services/              # business logic
│       ├── Models/                # feature DTOs
│       ├── Jobs/                  # Hangfire jobs and the feature's IJobRegistrar
│       ├── Notifications/         # domain events the feature publishes
│       ├── NotificationHandlers/  # handlers for events it reacts to
│       ├── <Entity>Queries.cs     # IQueryable filters for entities it owns
│       └── <Feature>ServiceExtensions.cs  # DI registration (AddTeams, AddAchievementsServices, ...)
├── Services/          # Core competition logic shared by features (CompetitionService, LeaderboardCalculator, ...)
├── Notifications/     # Core domain events (CompetitionStarted, CompetitionFinished, FreezieAdded, ...)
├── Bot/               # Telegram and Discord messaging: composers, cup messengers, Telegram commands
├── API/Velocidrone/   # IVelocidroneClient port and its DTOs
├── Persistence/       # IApplicationDbContext and query extensions for core entities
├── Jobs/              # IJobRegistrar
└── Mappings/          # Mapperly mappers
```

- Organize code by business capability, not by technical layer.
- Features don't reference each other directly. They communicate through domain events or well-defined interfaces.
- Put a service in the root `Services/` only when it is truly shared.

## Controllers and services

Controllers (Web) own data access and coordination; services (Application) own business rules.

- **Controllers** fetch entities through `IApplicationDbContext`, map DTOs to and from domain types, coordinate services, and handle HTTP concerns.
- **Services** take already-fetched entities (not IDs) and return result objects where needed.

```csharp
// Controller: data access
var action = await _db.Actions.FindAsync(model.ActionId);
var user = await GetCurrentUser<ApplicationUser>();

// Service: business logic
var result = _actionService.EditAction(action, model.Date, model.Comment, user);
```

Controller layout:
- `Controllers/Api/<Area>/`: the REST API for the React app, one folder per area with its controller, models and mapper (e.g. `Teams/TeamsController.cs`, `TeamsModels.cs`, `TeamModelMapper.cs`).
- `Controllers/Admin/<Area>/`: the admin dashboard.

Keep API models separate from domain models. The OpenAPI spec is generated to `shared/api/FpvBattle.Web.json`, and the frontend client is generated from it.

## Data access: IApplicationDbContext

`IApplicationDbContext` (`FpvBattle.Application/Persistence`) is the single unit of work. There are no repositories.

- Inject `IApplicationDbContext`, never `ApplicationDbContext`. It exposes one `DbSet<T>` per entity plus `SaveChangesAsync`.
- Stage changes on the sets (`Add`, `Update`, `Remove`) and call `SaveChangesAsync` once per use case.
- **Adding a set**: declare it on the interface and implement it explicitly in `ApplicationDbContext` (`DbSet<Pilot> IApplicationDbContext.Pilots => Set<Pilot>();`). A public `DbSet` property would be picked up by EF's convention and rename the table.
- **Query filters** are composable `IQueryable<T>` extension methods in Application, never in Domain. They go in `Features/<Feature>/<Entity>Queries.cs` for an entity a feature owns, and in `Persistence/Queries/<Entity>Queries.cs` for core entities (Pilot, Competition). Each has one concern and a descriptive name, and they chain.

```csharp
public static IQueryable<UserNotification> ForUser(this IQueryable<UserNotification> query, string userId)
    => query.Where(n => n.UserId == userId);

await _db.UserNotifications.ForUser(userId).Unread().CountAsync(ct);
```

- Global query filters hide soft-ended rows. For example, `CurrentMembers` hides ended team memberships, and history queries opt out with `IncludingFormer()`.
- Query only the slice the page shows (e.g. the default date range), never the whole history.

## Entity Framework

- All mapping is in Fluent API, with one `IEntityTypeConfiguration<T>` per entity in `FpvBattle.Infrastructure/Persistence/Configurations/`. `ApplyConfigurationsFromAssembly` picks them up.
- Domain entities stay clean POCOs, with no `[Key]`, `[MaxLength]` or other data annotations.
- Migrations live in `FpvBattle.Infrastructure/Persistence/Migrations/`. Every model change needs a migration: `ModelSnapshotTests` fails otherwise, and production data depends on it.
- Prefer soft endings over deletes when history matters: team deletion is soft, and memberships end with `LeftOn` (see `docs/teams.md`).

```csharp
public sealed class PilotConfiguration : IEntityTypeConfiguration<Pilot>
{
    public void Configure(EntityTypeBuilder<Pilot> builder)
    {
        builder.ToTable("Pilots");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(128);
    }
}
```

## Domain events

Domain events use MediatR: an `INotification` record in a `Notifications/` folder, and one `INotificationHandler<T>` per reaction in `NotificationHandlers/`. One event usually has a handler per channel. For example, `GotAchievements` is handled by the Telegram, Discord and in-app handlers.

## Background jobs (Hangfire)

Each feature registers its recurring jobs through an `IJobRegistrar` in its `Jobs/` folder, and registers that registrar in its `ServiceExtensions`.

```csharp
public class PatreonJobRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<PatreonSyncJob>("Patreon sync", x => x.Handle(CancellationToken.None), "0 9 * * *");
    }
}
```

Give jobs descriptive names. Before changing job or timing logic to fix a bug, confirm the root cause in production logs.

## Bots

- Telegram commands implement `ITelegramCommand` (`Bot/Telegram/Commands/Core`): `Keywords`, `Description`, `ExecuteAsync(TelegramCommandContext)`, `RemoveMessageAfterDelay` and `Public`. The context carries the cup, the chat and the parameters.
- Message text is built by composer classes, separate from bot logic. Examples are `TelegramAchievementMessageComposer` and `DiscordAchievementMessageComposer`.
- Per-cup channel configuration comes from cup service extensions, such as `CupServiceExtensions.GetTelegramChannelId()`.

## Admin pages (Razor)

The admin dashboard (`FpvBattle.Web/Views/**`: Users, Pilot Claims, Track Queue, Team Management, Patrons, ...) is server-rendered Razor with Bootstrap 5, not React.

- **Date format**: always European `dd.MM.yyyy`, adding `HH:mm` when time matters, e.g. `@entity.CreatedOn.ToString("dd.MM.yyyy HH:mm")`. Never use `yyyy-MM-dd` or the culture-default `ToString()` / `ToShortDateString()`.
- Shared styling lives in `wwwroot/css/site.css`, with design tokens under `:root`. Reuse existing classes (`.panel`, `.table`, `.btn-icon`, `.dropdown-menu`) instead of inline styles.
- Row actions beyond a single primary action go into a `.btn-icon` kebab-menu `.dropdown` (see `Views/Users/Index.cshtml`), not a row of buttons.

## Domain rules worth knowing

- Velocidrone pilot names are case sensitive ("Jack" ≠ "jack"): always match them ordinally.
- The Velocidrone leaderboard keeps one best time per pilot per quad class.
- The day streak is cup-agnostic: racing any one enabled cup keeps it alive.

## Adding a feature

1. Create `Features/<Feature>/` in Application with its services, models, events and handlers.
2. Add jobs and an `IJobRegistrar` if needed.
3. Register everything in `<Feature>ServiceExtensions` and call it from the Web composition root.
4. Add entities to Domain, a configuration and an `IApplicationDbContext` set in Infrastructure, and a migration.
5. Add controllers under `Controllers/Api/<Area>/`, then regenerate the OpenAPI spec and the frontend client.
