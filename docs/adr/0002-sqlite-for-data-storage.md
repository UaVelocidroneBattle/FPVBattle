# ADR-0002: SQLite for Data Storage

## Status
Accepted

## Context
VelocidroneBot requires persistent data storage for:

- **Pilot Information**: Names, streaks, achievements, race history
- **Competition Data**: Daily competitions, tracks, results, leaderboards
- **Bot State**: Configuration variables, Patreon supporter data
- **Background Jobs**: Hangfire job storage and execution history
- **Statistics**: Historical data for charts and analytics

The project operates under specific constraints:
- **Budget**: Free/open-source project funded personally by maintainers
- **Infrastructure**: Running on underpowered free/cheap hosting
- **Team Size**: Small development team with limited operational overhead
- **Scale**: Low-to-moderate load (Ukrainian Velocidrone community)
- **Deployment**: Simple deployment model with minimal dependencies

## Decision
Use **SQLite** as the primary database for all data storage needs.

## Rationale

### Deployment Simplicity
- **Zero Configuration**: No database server setup or management required
- **File-Based**: Database is a single file that deploys with the application
- **No External Dependencies**: Eliminates database server as deployment dependency
- **Backup Simplicity**: Database backup is just file copy operation

### Resource Efficiency
- **Low Memory Footprint**: Minimal RAM usage compared to database servers
- **CPU Efficient**: Optimized for single-application access patterns
- **Storage Efficient**: Compact file format, good compression
- **Perfect for Underpowered Servers**: Runs well on resource-constrained hosting

### Cost Considerations
- **Completely Free**: No licensing costs, no hosting fees for database servers
- **No Additional Infrastructure**: Eliminates need for separate database hosting
- **Operational Cost**: Zero ongoing database management overhead

### Development Experience
- **Simple Local Development**: No database server setup for developers
- **Easy Testing**: In-memory SQLite for unit tests
- **Familiar SQL**: Team's Microsoft SQL Server experience transfers directly
- **Entity Framework Support**: Full EF Core support with familiar patterns

### Scale Appropriateness
- **Current Load**: Handles hundreds of pilots without performance issues
- **Expected Growth**: Ukrainian drone racing community is limited in size
- **Read-Heavy Workload**: Most operations are leaderboard/statistics reads
- **Write Patterns**: Mostly periodic batch updates (daily results)

## Consequences

### Positive
- **Deployment Simplicity**: Single executable + database file deployment
- **Zero Operational Overhead**: No database administration required
- **Cost Effective**: Completely free, no ongoing database costs
- **Development Velocity**: No database setup barriers for new developers
- **Resource Efficient**: Runs on minimal hardware requirements
- **Backup/Restore Simplicity**: File-based operations
- **Version Control Friendly**: Can commit schema and test data

### Negative
- **Concurrency Limitations**: Single writer limitation (not an issue for our use case)
- **No Built-in Replication**: Manual backup strategies required
- **Scale Ceiling**: Will need migration if load grows significantly
- **Advanced Features**: Missing some enterprise database features

### Neutral
- **Different from Enterprise Patterns**: Not typical for web applications at scale
- **Migration Path**: Can migrate to PostgreSQL/SQL Server if needed in future

## Alternatives Considered

### Microsoft SQL Server
- **Pros**: Team has extensive experience, powerful features, excellent tooling
- **Cons**: 
  - **Resource Hungry**: High memory and CPU requirements
  - **Expensive**: Licensing costs for production use
  - **Overkill**: Advanced features not needed for current scale
  - **Complex Deployment**: Requires separate database server management
- **Verdict**: Too expensive and resource-intensive for project constraints

### PostgreSQL
- **Pros**: Free, powerful, good reputation, scalable
- **Cons**:
  - **No Team Experience**: Learning curve and potential issues
  - **Overkill**: Advanced features not needed for current requirements
  - **Additional Complexity**: Requires database server setup and management
  - **Resource Usage**: More resource-intensive than SQLite
- **Verdict**: Unnecessary complexity for current needs

### MySQL
- **Pros**: Free, widely used
- **Cons**:
  - **No Team Experience**: Same learning curve as PostgreSQL
  - **No Clear Benefits**: Doesn't offer advantages over PostgreSQL for our use case
  - **Additional Complexity**: Still requires database server management
- **Verdict**: No compelling reason to choose over PostgreSQL

### NoSQL Databases (MongoDB, Redis, etc.)
- **Pros**: Different data models, potentially simpler for some use cases
- **Cons**:
  - **No Team Experience**: Significant learning curve
  - **No Clear Benefits**: Relational data model fits our needs perfectly
  - **Additional Complexity**: New deployment and operational patterns
  - **Overkill**: Advanced scaling features not needed
- **Verdict**: No benefits for current relational data patterns

## Implementation Notes

### Database Configuration
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseLazyLoadingProxies()
        .UseSqlite(connectionString));
```

### File Organization
- **Main Database**: `DB/app.db` - Application data
- **Hangfire Database**: `DB/hangfire.db` - Background job data
- **Separation Rationale**: Isolate application data from job processing data

### Backup Strategy
- **Automated Backups**: File-based copying before deployments
- **Manual Backups**: Simple file download from server
- **Restore Process**: Replace database file and restart application

### Performance Optimizations
- **Indexing**: Proper indexes on frequently queried columns (pilot lookups, date ranges)
- **Connection Pooling**: EF Core handles connection management
- **WAL Mode**: Write-Ahead Logging for better concurrency
- **Lazy Loading**: Enabled for development simplicity

### Migration Path
If scale requirements change:
1. **SQLite → PostgreSQL**: EF Core supports database provider switching
2. **Data Migration**: Export/import tools available
3. **Code Changes**: Minimal - mostly connection string and provider configuration
4. **Timeline**: Can be done when concurrent users > 100 or data > 10GB

### Monitoring
- **Database Size**: Monitor file size growth
- **Query Performance**: Log slow queries in development
- **Lock Contention**: Monitor for write conflicts (unlikely in current usage)

## Date
2025-01-14

## Participants
- Development Team
- Project Maintainers

## Related Decisions
- ADR-0001: Hangfire for Background Jobs (shares database storage)
- Future ADR: Backup and Recovery Strategy
- Future ADR: Performance Monitoring and Alerting