# Testing

## Backend

All backend tests are in `backend/FpvBattle.Tests` and run with `dotnet test backend/FpvBattle.sln`.

- **Stack**: xUnit (`[Fact]`, `[Theory]` + `[InlineData]`) with FluentAssertions.
- **Naming**: `Method_ShouldOutcome_WhenCondition`, e.g. `GetVariable_ShouldReturnNull_WhenVariableDoesNotExist`.
- **Domain rules** (day streak, points, leagues, Patreon supporters) are tested directly on entities and calculators, with no database.
- **Database tests** use in-memory SQLite (`DataSource=:memory:` with `EnsureCreated`) through the real `ApplicationDbContext`. `TeamsTestBase` is the example: it seeds users and pilots and drives the controller and service together.
- **Events**: pass `RecordingPublisher` as the MediatR `IPublisher` to assert which domain events were published without running their handlers.
- **Migrations that move data** get a test for both directions (see `TeamMembershipHistoryMigrationTests`).
- Test a whole feature slice, and test cross-feature behaviour through the events it publishes.

### Architecture tests

`FpvBattle.Tests/Architecture` guards the structure of the solution:

- `ModelSnapshotTests` fails when the EF model differs from the latest migration. Production data depends on it, so never change the model without a migration.
- `LayerDependencyTests` pins which project may reference which. It keeps EF Core out of Domain, `IQueryable` extensions out of Domain, and external SDKs (Telegram, Discord, SixLabors, Google, RestSharp, MVC) out of Application. Tighten the rules as layers separate; never loosen them to make a change pass.

## Frontend

There is no frontend test suite. After every frontend change, run `npm run build` in `frontend/` (it runs `tsc -b` and `vite build`) and fix every error before you consider the task done. `npm run lint` runs ESLint.

UI changes are checked by the user on their own dev server. Don't drive the browser pane to verify small UI tweaks.
