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