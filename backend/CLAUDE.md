# Backend Development Guidelines

## Architecture Overview

FPV Battle backend follows a **Vertical Slices Architecture** within a **Modular Monolith** structure. This approach organizes code by business features rather than technical layers.

### Core Architecture Principles
- **Vertical Slices**: Each feature is self-contained with its own models, services, and handlers
- **Modular Monolith**: Single deployable unit with clear module boundaries
- **Feature-First Organization**: Code organized by business capability, not technical concern
- **Minimal Cross-Feature Dependencies**: Features communicate via well-defined interfaces

### Code Quality Principles

**CRITICAL: Always apply SOLID principles, especially SRP and DRY**

#### Single Responsibility Principle (SRP)
- Each class should have ONE reason to change
- Extract domain logic to appropriate service extensions (e.g., `CupServiceExtensions.GetTelegramChannelId()`)
- Don't mix concerns (e.g., telegram handlers should not contain cup configuration logic)

#### Don't Repeat Yourself (DRY)
- **If you spot duplicate code (same helper method in 3+ files), create a task to refactor it AFTER completing the main work**
- Use extension methods for shared logic (see the query extensions under "Data Access" below)
- Place extensions in the appropriate domain namespace (e.g., cup logic → `Features/Cups/CupServiceExtensions.cs`)

**Workflow when spotting violations:**
1. Complete the main task first
2. Use TaskCreate to add cleanup task: "Extract duplicated [MethodName] to extension method"
3. Refactor after main work is stable

### TKL Controller-Service Architecture Pattern

**Preferred approach for this project:**

#### Controller Responsibilities (Application Layer)
- Handle data access (queries through `IApplicationDbContext`, entity fetching)
- Converts DTO to domain classes and vice versa
- Coordinate between different services
- Handle HTTP concerns (model binding, routing, etc.)
- Pass already-fetched entities to services

#### Service Responsibilities (Domain Layer)
- Focus on pure business logic
- Accept entities as parameters, not IDs
- Return result objects when needed

#### Example Pattern:
```csharp
// Controller handles data access
var action = await _db.Actions.FindAsync(model.ActionId);
var user = await GetCurrentUser<ApplicationUser>();
var dictItem = FindDictItemById(model.DictItemId);

// Service handles business logic
var result = _actionService.EditAction(action, model.Date, model.Comment, dictItem, user);
```

#### Benefits:
- Clear separation of concerns
- Better testability (easy to mock entities)
- Services focused on business rules
- Controllers control data fetching optimization

### Data Access: IApplicationDbContext

`IApplicationDbContext` (`FpvBattle.Logic/Persistence`) is the single unit of work. There are no repositories.

- **Inject `IApplicationDbContext`**, never `ApplicationDbContext`. It exposes one `DbSet<T>` per entity plus `SaveChangesAsync`.
- **Stage changes on the sets** (`Add`, `Update`, `Remove`) and **call `SaveChangesAsync` once per use case** in new code.
- **Adding a set**: declare it on the interface and implement it explicitly in `ApplicationDbContext` (`DbSet<Pilot> IApplicationDbContext.Pilots => Set<Pilot>();`). A public `DbSet` property would be picked up by EF's convention and rename the table.
- **Query filters** stay as composable `IQueryable<T>` extension methods next to their feature: one concern each, descriptive names, chainable.

```csharp
public static IQueryable<UserNotification> ForUser(this IQueryable<UserNotification> query, string userId)
    => query.Where(n => n.UserId == userId);

// Usage
await _db.UserNotifications.ForUser(userId).Unread().CountAsync(ct);
```

## Project Structure

### FpvBattle.Domain (Domain Layer)
- **Entities**: Clean POCOs carrying their own business rules (day streak, freezies, leagues, teams)
- **Query Extensions**: Pure LINQ extensions only (e.g. `ByName`), nothing that needs EF Core to run
- **No Dependencies**: No EF Core and no project references

### FpvBattle.Infrastructure (Infrastructure Layer)
References Domain and Logic; implements Logic's `IApplicationDbContext`. Everything under `Persistence/`:
- **ApplicationDbContext**: Identity tables plus every configuration in `Configurations/`
- **Configurations**: One `IEntityTypeConfiguration<T>` per entity holding its Fluent API mapping
- **Migrations**: Database schema evolution

### FpvBattle.Logic (Business Logic Layer)
`Persistence/` holds `IApplicationDbContext` and query extensions that need EF Core (e.g. `PilotQueries`). Logic references Domain only, never Infrastructure.

Organized by **vertical slices** (business features):

```
Features/
├── Achievements/          # Achievement system slice
│   ├── Collection/        # Achievement implementations
│   ├── Jobs/             # Background job handlers
│   ├── NotificationHandlers/ # Event handlers
│   ├── Services/         # Business logic services
│   └── Notifications/    # Domain events
├── Patreon/              # Patreon integration slice
│   ├── Services/         # Patreon API integration
│   ├── Jobs/             # Sync and processing jobs
│   ├── Models/           # Patreon-specific DTOs
│   └── NotificationHandlers/ # Event handlers
└── Bot/                  # Bot communication slice
    ├── Discord/          # Discord-specific implementation
    ├── Telegram/         # Telegram-specific implementation
    └── Commands/         # Bot command handlers
```

### FpvBattle.Web (Presentation Layer)
- **Controllers**: Thin HTTP endpoints delegating to Logic layer
- **Infrastructure**: Cross-cutting concerns (DI, middleware, configuration)
- **API Surface**: RESTful endpoints with OpenAPI documentation

## Vertical Slices Guidelines

### Feature Organization
Each feature slice should contain:
- **Services**: Core business logic for the feature
- **Models/DTOs**: Feature-specific data structures
- **Jobs**: Background processing related to the feature
- **NotificationHandlers**: Handlers for domain events
- **ServiceExtensions**: DI registration for the feature

### Example: Achievements Feature Slice
```
Features/Achievements/
├── Base/
│   └── IAchievement.cs           # Achievement interface
├── Collection/
│   ├── DayStreakAchievements.cs  # Day streak implementations
│   └── PlacementAchievements.cs  # Race placement achievements
├── Services/
│   └── AchievementService.cs     # Core achievement logic
├── Jobs/
│   └── DayStreakMilestoneJob.cs  # Background processing
├── NotificationHandlers/
│   └── AchievementsEventHandler.cs # Domain event handling
└── AchievementsServiceExtensions.cs # DI registration
```

### Cross-Feature Communication
- **Domain Events**: Use notification pattern for loose coupling
- **Shared Services**: Place truly shared services in root `Services/` folder
- **Avoid Direct Dependencies**: Features should not directly reference each other

## Modular Monolith Benefits

### Development Benefits
- **Feature Team Alignment**: Each team can own complete vertical slices
- **Reduced Merge Conflicts**: Teams work in isolated feature folders
- **Easier Testing**: Test complete features in isolation
- **Clear Boundaries**: Well-defined module interfaces

### Deployment Benefits
- **Single Deployment Unit**: Simpler deployment and operations
- **Shared Infrastructure**: Database, logging, monitoring
- **Transaction Consistency**: ACID transactions across features
- **Performance**: No network calls between modules

## Entity Framework Guidelines

### Fluent API Configuration
Each entity's EF configuration lives in its own class under `FpvBattle.Infrastructure/Persistence/Configurations/`. `ApplicationDbContext.OnModelCreating()` picks them all up with `ApplyConfigurationsFromAssembly`, so a new entity only needs a new configuration class:
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

### Domain Model Purity
Keep domain models as clean POCOs:
```csharp
// ✅ CORRECT: Clean domain model
public class Pilot
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int UserId { get; set; }
    public DateTime FirstRace { get; set; }
}

// ❌ WRONG: Polluted with persistence concerns
public class Pilot
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }
}
```

## Background Jobs (Hangfire)

### Job Registration Pattern
Use `IJobRegistrar` for feature-specific job registration:
```csharp
public class AchievementsJobRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<DayStreakMilestoneJob>(
            "check-daystreak-milestones",
            job => job.Execute(),
            Cron.Hourly);
    }
}
```

### Job Organization
- Keep jobs within their feature slices
- Register jobs in feature's `ServiceExtensions.cs`
- Use descriptive job names and cron expressions

## API Development

### Controller Organization
Organize controllers by business domain:
```
Controllers/
├── Pilots/
│   ├── PilotsController.cs
│   ├── PilotProfileModel.cs
│   └── PilotProfileMapper.cs
├── Competitions/
│   ├── CompetitionsController.cs
│   ├── CompetitionModel.cs
│   └── CompetitionMapper.cs
└── Heatmap/
    └── PilotResultsController.cs
```

### OpenAPI Integration
- API specification auto-generated to `shared/api/FpvBattle.Web.json`
- Frontend consumes generated TypeScript client
- Keep API models separate from domain models

## Admin Pages (Razor Views)

The admin dashboard (`FpvBattle.Web/Views/**`, e.g. Users, Pilot Claims, Track Queue) is server-rendered Razor/Bootstrap 5, not React.

- **Date format**: always European `dd.MM.yyyy` (add `HH:mm` when time matters), e.g. `@entity.CreatedOn.ToString("dd.MM.yyyy HH:mm")`. Never `yyyy-MM-dd` or culture-default `ToString()`/`ToShortDateString()`.
- Shared styling lives in `wwwroot/css/site.css` (design tokens under `:root`); reuse existing classes (`.panel`, `.table`, `.btn-icon`, `.dropdown-menu`, etc.) instead of inline styles.
- Row-level actions beyond a single primary action go into a `.btn-icon` kebab-menu `.dropdown` (see `Views/UserPilots/Index.cshtml`) rather than a row of buttons.

## Service Registration

### Feature-Based Registration
Each feature registers its services:
```csharp
public static class AchievementsServiceExtensions
{
    public static IServiceCollection AddAchievements(this IServiceCollection services)
    {
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<DayStreakMilestoneJob>();

        // Register all achievement implementations
        services.AddTransient<IEnumerable<IAchievement>>(provider =>
            typeof(IAchievement).Assembly
                .GetTypes()
                .Where(t => t.IsClass && t.IsAssignableTo(typeof(IAchievement)))
                .Select(t => (IAchievement)provider.GetRequiredService(t)));

        return services;
    }
}
```

## Testing Strategy

### Feature-Level Testing
Test complete vertical slices:
```csharp
public class AchievementServiceTests
{
    [Test]
    public async Task ProcessDayStreak_WhenPilotReachesMillestone_ShouldAwardAchievement()
    {
        // Test the entire achievement feature slice
    }
}
```

### Architecture Tests
`FpvBattle.Tests/Architecture` guards the structure of the solution:
- `ModelSnapshotTests` fails when the EF model differs from the latest migration. Production data depends on it, so never change the model without a migration.
- `LayerDependencyTests` pins which project may reference which, and keeps EF Core out of FpvBattle.Domain. Tighten the rules as layers are separated; never loosen them to make a change pass.

### Integration Testing
Test cross-feature communication via events:
```csharp
public class CompetitionAchievementIntegrationTests
{
    [Test]
    public async Task CompletedRace_ShouldTriggerAchievementCheck()
    {
        // Test event flow between Competition and Achievement features
    }
}
```

## Migration Guidelines

### Adding New Features
1. Create new feature folder under `Features/`
2. Implement feature services and models
3. Add background jobs if needed
4. Create service extension for DI registration
5. Add controllers for API endpoints
6. Update OpenAPI spec generation

### Refactoring Existing Features
1. Maintain backward compatibility
2. Use feature flags for gradual rollout
3. Keep database migrations separate
4. Test cross-feature event contracts

## Bot Architecture

### Command Pattern Implementation
```csharp
public interface ITelegramCommand
{
    string Command { get; }
    Task HandleAsync(string[] args, long chatId);
}

public class AchievementsCommand : ITelegramCommand
{
    public string Command => "/achievements";

    public async Task HandleAsync(string[] args, long chatId)
    {
        // Command implementation within feature slice
    }
}
```

### Bot Message Composition
Separate message building from bot logic:
```csharp
public class TelegramAchievementMessageComposer
{
    public string ComposeAchievementMessage(PilotAchievement achievement)
    {
        // Feature-specific message composition
    }
}
```

## Performance Considerations

### Database Optimization
- Add indexes for feature-specific queries
- Use `Include()` for predictable data loading
- Implement caching at feature boundaries

### Cross-Feature Performance
- Minimize cross-feature database queries
- Use domain events for async communication
- Cache shared reference data

## Security Guidelines

### Feature Isolation
- Each feature validates its own inputs
- No feature should bypass another's validation
- Share security utilities, not security logic

### API Security
- Implement authentication at the Web layer
- Authorize access to feature endpoints
- Validate feature-specific business rules
