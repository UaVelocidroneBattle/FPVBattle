# ADR-0001: Hangfire for Background Job Processing

## Status
Accepted

## Context
VelocidroneBot requires several background processing capabilities:

- **Daily Competition Management**: Starting new competitions at 17:00 UTC daily
- **Results Processing**: Periodic fetching of race results from Velocidrone API
- **Achievement Processing**: Checking for day streak milestones and other achievements
- **Patreon Synchronization**: Monthly syncing of Patreon supporters
- **Bot Notifications**: Sending periodic updates to Discord/Telegram channels
- **Data Cleanup**: Periodic maintenance tasks

The system needs a reliable background job processing solution that can handle:
- Scheduled/recurring jobs (daily, hourly, monthly)
- One-time delayed jobs
- Job persistence across application restarts
- Job monitoring and failure handling
- Integration with existing ASP.NET Core application

## Decision
Use **Hangfire** as the background job processing framework.

## Rationale

### Team Experience
- Development team has prior experience with Hangfire
- Familiar with its configuration, patterns, and troubleshooting
- Reduces learning curve and implementation time

### Technical Requirements Match
- **Recurring Jobs**: Perfect for daily competition scheduling
- **Persistence**: Jobs survive application restarts
- **Web Dashboard**: Built-in monitoring and management UI
- **ASP.NET Core Integration**: Seamless integration with existing stack
- **SQLite Support**: Matches our lightweight database approach

### Operational Benefits
- **Reliability**: Automatic retry mechanisms for failed jobs
- **Monitoring**: Built-in dashboard for job status and history
- **Scalability**: Can handle current load and future growth
- **Simplicity**: Minimal configuration and maintenance overhead

## Consequences

### Positive
- Quick implementation due to team familiarity
- Reliable job execution with built-in retry logic
- Excellent monitoring and debugging capabilities via web dashboard
- Jobs persist across application restarts and deployments
- Easy to add new background jobs as features grow
- No additional infrastructure required (uses existing SQLite database)

### Negative
- Additional dependency in the application
- Hangfire dashboard adds attack surface (mitigated by authorization)
- SQLite storage may become bottleneck at very high scale (not current concern)
- License considerations for commercial use (using free version)

### Neutral
- Learning curve for team members unfamiliar with Hangfire
- Background job state stored in database increases database size

## Alternatives Considered

### .NET Background Services (IHostedService)
- **Pros**: Built into .NET, no additional dependencies
- **Cons**: No persistence, no web UI, manual retry logic, complex scheduling
- **Verdict**: Too basic for our needs

### Quartz.NET
- **Pros**: Mature, feature-rich scheduling framework
- **Cons**: More complex configuration, no team experience, no built-in web UI
- **Verdict**: Overkill for current requirements

### Azure Functions / AWS Lambda
- **Pros**: Serverless, managed scaling
- **Cons**: Additional cloud dependencies, different deployment model, overkill for current scale
- **Verdict**: Unnecessary complexity for current needs

### Custom Timer-based Solution
- **Pros**: Full control, minimal dependencies
- **Cons**: Complex to implement correctly, no persistence, no monitoring
- **Verdict**: High development cost, low reliability

## Implementation Notes

### Database Configuration
```csharp
services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(hangfireConnectionString));
```

### Job Organization Pattern
- Use `IJobRegistrar` interface for feature-specific job registration
- Keep job classes within their respective feature slices
- Register recurring jobs in `DefaultInit.cs` during application startup

### Security Considerations
- Hangfire dashboard protected by authorization filter
- Job parameters sanitized to prevent injection attacks
- Sensitive data (tokens, passwords) not passed directly to jobs

### Example Job Implementation
```csharp
public class DayStreakMilestoneJob
{
    private readonly IAchievementService _achievementService;

    public DayStreakMilestoneJob(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    public async Task Execute()
    {
        await _achievementService.CheckDayStreakMilestones();
    }
}

// Registration
RecurringJob.AddOrUpdate<DayStreakMilestoneJob>(
    "check-daystreak-milestones",
    job => job.Execute(),
    Cron.Hourly);
```

### Monitoring and Alerting
- Use Hangfire dashboard for job monitoring
- Implement custom logging for job execution
- Set up alerts for critical job failures

## Date
2025-01-14

## Participants
- Development Team
- DevOps Team

## Related Decisions
- Future ADR: Vertical Slices Architecture (affects job organization)
- Future ADR: SQLite vs PostgreSQL (affects Hangfire storage backend)