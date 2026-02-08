# Backend Development Guidelines

## Architecture Overview

VelocidroneBot backend follows a **Vertical Slices Architecture** within a **Modular Monolith** structure. This approach organizes code by business features rather than technical layers.

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
- Use extension methods for shared logic (follow the repository pattern shown below)
- Place extensions in the appropriate domain namespace (e.g., cup logic → `Features/Cups/CupServiceExtensions.cs`)

**Workflow when spotting violations:**
1. Complete the main task first
2. Use TaskCreate to add cleanup task: "Extract duplicated [MethodName] to extension method"
3. Refactor after main work is stable

### TKL Controller-Service Architecture Pattern

**Preferred approach for this project:**

#### Controller Responsibilities (Application Layer)
- Handle data access (repository calls, entity fetching)
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
var action = await _actions.FindAsync(model.ActionId);
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

### TKL Repository Pattern

**Prefer thin repositories with extension methods over fat repositories.**

#### Fat Repository (Avoid)
- Contains many specific methods like `FindLoansByDate()`, `CountActiveUsers()`
- Becomes unmanageable as codebase grows
- Code duplication across similar methods
- Hard to compose queries
- Example: `El.Business.Repositories.LoanRepository.cs`

#### Thin Repository (Preferred)
- Only contains basic CRUD operations (`GetAll()`, `FindAsync()`, `Add()`, etc.)
- Uses extension methods for composable query filters
- Each extension does ONE thing and returns `IQueryable<T>`
- Can chain extensions: `.Active().ByDate(now).ForUser(user).ToListAsync()`

#### Extension Method Rules:
- Create in `[Entity]Extensions.cs` or `[Entity]QueryExtensions.cs`
- Take `IQueryable<T>` as first parameter, return `IQueryable<T>`
- Use descriptive names: `Active()`, `ByDate()`, `ForLoan()`
- Keep simple and focused on single concern

#### Example:
```csharp
// Good: Extension methods (El.DomainEntities.Actions.Action.cs)
public static IQueryable<Action> ForLoan(this IQueryable<Action> query, Loan loan)
    => query.Where(a => a.LoanId == loan.Id);

// Usage
_actions.GetAll().ForLoan(loan).Where(a => a.Date < DateTime.Now).ToListAsync()
```

## Project Structure

### Veloci.Data (Persistence Layer)
- **Domain Models**: Clean POCOs representing business entities
- **DbContext**: Centralized EF configuration via Fluent API
- **Repositories**: Generic repository pattern for data access
- **Migrations**: Database schema evolution

### Veloci.Logic (Business Logic Layer)
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

### Veloci.Web (Presentation Layer)
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
All EF configuration in `ApplicationDbContext.OnModelCreating()`:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configure entities via Fluent API
    modelBuilder.Entity<Pilot>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        entity.HasIndex(e => e.UserId);
    });
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
- API specification auto-generated to `shared/api/Veloci.Web.json`
- Frontend consumes generated TypeScript client
- Keep API models separate from domain models

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
