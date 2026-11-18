# Workflow

## Branches

- Start every bug fix or feature on its own branch:
  - bug: `bugs/<github-issue-id>-<description>`
  - feature: `feature/<github-issue-id>-<description>`
- Create and check out the branch before making changes.

## Commits

- Commit only when the user asks, even when the feature is done. The `.claude/hooks/confirm-git-commit.js` hook asks for confirmation on every `git commit`.
- Keep commit messages concise and clean.

## GitHub issues

Keep issue descriptions concise: 2-3 sentences.

## Pull requests

- Lead with the business value, then the technical implementation.
- Describe what was actually built, not temporary workarounds or problems avoided.
- Make each point add something new. Don't restate the same thing in different words.

## Documentation

- Feature docs live in `docs/` (e.g. `docs/teams.md`). Update them together with the code they describe.
- Significant architectural decisions get an ADR in `docs/adr/`: copy `template.md` and number it sequentially (`0005-kebab-case-title.md`).
- Conventions for Claude live in `.claude/conventions/`. When a convention changes, update the one file that owns it rather than adding a copy elsewhere.

## Common commands

| Task | Command |
| --- | --- |
| Run backend | `dotnet run --project backend/FpvBattle.Web` |
| Backend tests | `dotnet test backend/FpvBattle.sln` |
| Add EF migration | `dotnet ef migrations add <Name> --project backend/FpvBattle.Infrastructure --startup-project backend/FpvBattle.Web` |
| Frontend dev server | `npm run dev` (in `frontend/`) |
| Frontend build check | `npm run build` (in `frontend/`) |
| Regenerate API client | `npm run gen` (in `frontend/`); `npm run gen:full` also rebuilds the backend's OpenAPI spec |

Dev server configurations for the preview pane are in `.claude/launch.json`.
