# Patreon Integration Requirements & Implementation Plan

## Requirements Overview
- Implement Patreon integration to get list of supporters daily via Hangfire background job
- Store supporter data in database (no relationship to Pilots)
- Add notifications for new supporters and monthly supporter lists
- Manual OAuth flow for initial token setup (one-time process)
- Backend-only implementation (no React changes needed)

## User Requirements & Clarifications
- No ASP.NET Core OAuth integration needed - just display access/refresh tokens
- One-time manual process for token retrieval
- Main page available only in dev environment (no auth restrictions needed)
- During initial flow, don't store tokens to database - just display them
- No relationship between PatreonSupporter and Pilot models
- Create page to display received access and refresh tokens for secure manual storage
- Connect Patreon button should be on ASP.NET Core side, not React
- Use Context7 to check best practices and updated docs
- No tests initially needed
- This is a hobby project (no GDPR considerations)

## Implementation Plan

### Phase 1: Manual OAuth Token Retrieval
1. **Add Patreon settings to `appsettings.json`**
   - ClientId, ClientSecret, RedirectUri (dev environment only)

2. **Create simple `PatreonController`**
   - Manual OAuth flow without ASP.NET Core OAuth middleware
   - Generate authorization URL with scopes: `identity`, `campaigns.members`

3. **Add "Connect Patreon" link on main page**
   - Simple link/button visible in dev environment only
   - Links to Patreon OAuth authorization

4. **Implement OAuth callback endpoint**
   - Handle callback from Patreon with authorization code
   - Exchange code for access/refresh tokens via direct HTTP call

5. **Create token display page**
   - Simple view showing access token and refresh token
   - Allow manual copying for secure storage
   - No database persistence during initial setup

### Phase 2: Database Models
1. **Create `PatreonSupporter` entity**
   - Properties: PatreonId, Name, Email, TierName, Amount, Status, FirstSupportedAt, LastUpdated
   - No relationship to Pilot model (as requested)

2. **Create Entity Framework migration**
   - Add PatreonSupporter table following existing patterns

### Phase 3: Patreon API Service
1. **Create `IPatreonService` interface and implementation**
   - Methods for fetching campaign members/supporters
   - Direct HTTP client calls to Patreon API endpoints
   - Use manually configured access/refresh tokens

2. **Implement token refresh handling**
   - Automatic refresh of expired access tokens
   - Handle API rate limits (100 requests/2 seconds)

### Phase 4: Background Job Implementation
1. **Create `PatreonSyncJob`**
   - Daily Hangfire job following existing `CompetitionService` patterns
   - Use `JobExecutionLoggingAttribute` for consistent logging

2. **Implement supporter sync logic**
   - Fetch supporter data from Patreon API
   - Compare with existing database records
   - Update/insert supporter information
   - Detect new supporters for notifications

### Phase 5: Notification System
1. **Extend MediatR notification system**
   - Create `NewPatreonSupporterNotification` for new supporters
   - Create `MonthlyPatreonSupportersNotification` for monthly lists

2. **Integrate with existing bot services**
   - Use existing Discord/Telegram notification infrastructure
   - Follow patterns similar to `DayStreakAchievements` notifications

## Technical Architecture

### OAuth Flow (Simplified)
```
1. User clicks "Connect Patreon" on main page
2. Redirect to Patreon authorization URL with scopes
3. User authorizes on Patreon
4. Patreon redirects to callback with authorization code
5. Exchange code for access/refresh tokens via HTTP POST
6. Display tokens on page for manual storage
```

### API Integration
- **Authorization URL**: `https://patreon.com/oauth2/authorize`
- **Token URL**: `https://patreon.com/api/oauth2/token`
- **API Base**: `https://www.patreon.com/api/oauth2/v2`
- **Required Scopes**: `identity`, `campaigns.members`
- **Rate Limits**: 100 requests per 2 seconds

### Database Schema
```sql
PatreonSupporter {
    PatreonId: string (PK)
    Name: string
    Email: string
    TierName: string
    Amount: decimal
    Status: string
    FirstSupportedAt: DateTime
    LastUpdated: DateTime
}
```

### File Structure
```
backend/Veloci.Data/Domain/PatreonSupporter.cs
backend/Veloci.Logic/Services/IPatreonService.cs
backend/Veloci.Logic/Services/PatreonService.cs
backend/Veloci.Logic/Services/PatreonSyncJob.cs
backend/Veloci.Logic/Notifications/NewPatreonSupporterNotification.cs
backend/Veloci.Logic/Notifications/MonthlyPatreonSupportersNotification.cs
backend/Veloci.Web/Controllers/PatreonController.cs
backend/Veloci.Web/Views/Patreon/Tokens.cshtml
```

## Implementation Notes
- Follow existing architectural patterns (Clean Architecture, MediatR, Hangfire)
- Use existing code style and conventions from the codebase
- Implement proper error handling and logging using Serilog
- No ASP.NET Core OAuth middleware - use direct HTTP client calls
- Store sensitive configuration in User Secrets for development
- Consider encryption for stored tokens in production (future enhancement)

## Questions Resolved
- ✅ Can we map supporters to players? - No, keep separate
- ✅ Use ASP.NET Core OAuth system? - No, manual OAuth flow
- ✅ Store tokens in database initially? - No, display for manual storage
- ✅ Add authentication to main page? - No, dev environment only
- ✅ Add tests? - Not initially, focus on implementation
- ✅ GDPR compliance? - No, hobby project