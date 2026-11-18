# Code Style

These rules apply to every layer: backend, frontend and admin views.

## Principles

- Always favour the most readable and elegant solution. Developers should be proud of the code. It should be maintainable and literate.
- Readability beats brevity: clear code is better than clever code.
- Never settle for a quick fix. Refactor to a maintainable solution.
- Ask of every implementation: is this the most maintainable approach, will the next developer understand it, and how does it hold up with 10x more features?

If you would not be proud to show the code to a senior developer, it is not ready for this codebase.

## SOLID, especially SRP and DRY

- **Single responsibility**: each class, component, hook or service has one reason to change. Don't mix concerns: a Telegram handler does not hold cup configuration logic; it calls `CupServiceExtensions.GetTelegramChannelId()`.
- **Don't repeat yourself**: when the same logic appears in several places, extract it into one well-named shared component, service or extension method, and update every consumer to use it.

When you spot duplication (the same helper in 3+ files) during other work:
1. Finish the main task first.
2. Add a follow-up task: "Extract duplicated [MethodName] to extension method".
3. Refactor once the main work is stable, and test that behaviour is unchanged.

## Naming and comments

- Good names carry the meaning. Write few comments.
- Don't write XML doc comments or JSDoc that only restate a member's name.
- A comment explains *why* when the code cannot. Longer rationale belongs in `docs/` or an ADR, not in the code.
