# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

VelocidroneBot is a Discord and Telegram bot for Velocidrone drone racing competitions. The project consists of:

- **Backend**: ASP.NET Core 10.0 modular monolith using vertical slices architecture, with Entity Framework, Hangfire background jobs, and bot services
- **Frontend**: React + TypeScript dashboard with Vite, Zustand state management, and TailwindCSS
- **Shared**: OpenAPI specification for type-safe client generation

The purpose of the bot is to run daily flight competitions. Each day, the bot selects a new track and posts its name in the chat channels. When pilots see the announcement, they can start racing.

Throughout the day, the bot checks results, tracks everyone's progress, and sends periodic updates to the chats, including the current leaderboard. At the end of the day, the competition for that track is closed, and the bot posts the final results. Pilots earn points based on their performance and their final ranking.

To encourage people to fly every day, the bot also calculates "daystreaks." A daystreak is the number of consecutive days a pilot has flown without missing a day. Occasionally, a pilot can earn a "freezie," which is an item used to save their daystreak if they are unable to fly.

## Development Principles

- Always favour most readable and elegant solutions. Developers should be proud of the code. It should be maintainable and literate.

## Code Quality Standards

VelocidroneBot maintains **enterprise-grade quality standards** across all code layers. When implementing features or fixing issues:

### Quality-First Approach
- **Never settle for "quick fixes"** - always refactor to maintainable, professional solutions
- **Extract reusable components** when duplicate logic appears across multiple files
- **Follow DRY principle** - eliminate code duplication through proper abstraction
- **Apply single responsibility principle** - each component/service should have one clear purpose

### Code Review Mindset
- **Question initial implementations** - is this the most maintainable approach?
- **Consider future developers** - will this code be easy to understand and modify?
- **Think about scalability** - how will this pattern work with 10x more features?
- **Prioritize readability** over brevity - clear code is better than clever code

### Refactoring Standards
When code duplication or quality issues are identified:
1. **Stop and refactor immediately** - technical debt compounds quickly
2. **Extract shared components/services** with clear, descriptive names
3. **Centralize logic** in single locations with well-defined interfaces  
4. **Update all consumers** to use the new shared implementation
5. **Test thoroughly** to ensure refactoring maintains functionality

**Remember**: If you're not proud to show the code to senior developers, it's not ready for this codebase.

## Architecture Guidelines

### Backend Architecture (Vertical Slices)
- **Modular Monolith**: Single deployable unit with clear feature boundaries
- **Vertical Slices**: Features organized by business capability (Achievements, Patreon, Bot) rather than technical layers
- **Feature Isolation**: Each feature slice contains its own services, models, jobs, and event handlers
- **Cross-Feature Communication**: Use domain events and well-defined interfaces, avoid direct dependencies
- **See @backend/CLAUDE.md for detailed backend guidelines**

### Entity Framework Configuration
- All Entity Framework configuration (MaxLength, Key, indexes, etc.) is done via Fluent API in ApplicationDbContext.OnModelCreating()
- Domain models are kept as clean POCOs without data annotation attributes like [MaxLength], [Key], etc.
- This approach provides better separation of concerns and keeps domain models independent of persistence concerns

### Frontend Architecture (Component-Based)
- **React** as ui framework
- **Zustand State Management**: Clean separation of state and actions
- **Component Organization**: UI components in `src/components/ui/`, business components in `src/components/`, page-specific in `src/pages/`
- **API Integration**: Generated TypeScript client from OpenAPI specification
- **See @frontend/CLAUDE.md for detailed frontend guidelines**

## Version Control Guidelines

- When you asked to fix the issue, or implement a feature create separate branch for that. If it is a bug, branch should be `bugs\github-issue-id-description` or for feature - `feature\github-issue-id-describpiton`
- When you are asked to work a new feature or bug, create and checkout appropriate branch
- When asked to create github issues, be concise in description. It should be 2-3 sentences
- When you are asked to commit, create commit messages concise and clean

## Technical Communication Guidelines

### Pull Request Documentation
- **Focus on actual implementation**, not temporary solutions or workarounds
- **Avoid repetition** - each point should add unique value to the description
- **Lead with business value**, follow with technical implementation details
- **Be concise** - eliminate redundant statements that say the same thing differently
- **Use concrete language** - describe what was built, not what problems were avoided