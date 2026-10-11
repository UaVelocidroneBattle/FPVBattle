# Teams

Signed-in users with a linked pilot can create teams and invite other pilots to them from the Teams page (`/teams`). Anyone, including visitors, can browse teams and their members.

## Roles

| Role | Can do |
|---|---|
| Owner | Everything an admin can, plus delete the team, change member roles, remove admins. Exactly one per team. |
| Admin | Edit the name and description, invite pilots, revoke invites, remove plain members. |
| Member | Leave the team. |

The owner cannot leave; they delete the team instead.

## Invites

- Sent by pilot name (case sensitive) to the user linked to that pilot, so an invitee always has a linked pilot.
- They never expire. They disappear when accepted, declined or revoked.
- The invite dialog suggests only pilots that can be invited right now: linked to an account, not in the team, not already invited, and not at the team limit.
- Accepting checks the limits again, because an invite can wait for a long time.
- Joining is by invitation only.

## Limits

The `Teams` section of `appsettings.json` (documented in `appsettings.schema.json`):

| Setting | Meaning |
|---|---|
| `TeamsPerUser` | Teams a user can belong to. Default 1, so one user - one team. 0 means no limit. |
| `MaxOwnedTeamsPerUser` | Teams a user can create. |
| `MaxTeamSize` | Members per team. 0 means no limit. The UI shows "x / limit" only when a limit is set. |

Creating a team requires a linked pilot.

## Names

Team names are unique ignoring case (the SQLite column uses the `NOCASE` collation) and at most 32 characters. The description is optional, at most 500. `NOCASE` folds only ASCII letters, so "Дрони" and "дрони" are treated as different names.

## Tags

A tag is a short label for the leaderboard, where the full name would not fit. The creator enters it when creating the team: 2 to 5 letters or digits, stored in upper case and unique ignoring case (`NOCASE` collation plus a unique index). The form upper-cases and filters the input as you type; the server validates again.

- Teams created before tags existed have no tag until an admin sets one.
- The owner and team admins change it in the edit dialog, together with the name and description. Site admins change it on the admin team page, which also lets them reclaim a squatted tag.
- Editing a team always requires a valid tag, so a team without one gets it the first time it is edited.
- `Teams:BlacklistedTags` in `appsettings.json` lists tags nobody can pick (compared ignoring case). A team that already holds a tag blacklisted later keeps it. The admin page applies the same rules.

## Privacy

Teams are public, so responses must never contain account display names or emails: they come from the Google profile and are usually real names. Members and inviters are identified by pilot name only. `TeamsTests.Responses_NeverContainDisplayNamesOrEmails` guards this.

## Account deletion

Before an account is deleted, a team the user owns goes to its longest-serving admin, then to its longest-serving member, and is deleted when nobody else is left (`TeamService.RemoveUserFromTeamsAsync`).

## Admin page

The admin area has a Teams page (`/TeamManagement`, sidebar: Pilots > Teams). Admins act outside the team roles, so `TeamPermissions` does not apply, but the limits do (`TeamsPerUser`, `MaxOwnedTeamsPerUser`, `MaxTeamSize`). Pilots are always entered by exact, case-sensitive pilot name, and pages show pilot names only, never emails.

- **List**: team, owner, members (`x / limit` when a limit is set), pending invites, created date. Searchable and sortable.
- **Create a team**: the admin picks a pilot as the owner, who must be linked to an account.
- **Team page**: edit name and description (same rules as users), delete the team.
- **Members**: add a pilot straight into the team (as a plain member; their pending invite to it is dropped), make a member admin or member again, remove a member. The owner can be neither removed nor demoted.
- **Pending invites**: revoke.

Code: `Veloci.Web/Controllers/Admin/Teams` (`TeamManagementController`), `Views/TeamManagement`. It is not named `TeamsController` because the API controller has that name, and the shared route value made links on the admin pages point at `/api/teams/...`. Tests: `TeamManagementTests`, on top of `TeamsTestBase`.

## Code

- Backend: `Veloci.Logic/Features/Teams` (`TeamService` runs the operations, `TeamPermissions` decides who may run them and is asked by the API controller, `TeamSettings`), `Veloci.Web/Controllers/Api/Teams`, domain `Team`, `TeamMember`, `TeamInvite`.
- Frontend: `src/pages/teams`, `src/store/teamsStore.ts`.
- API: `/api/teams/[action]`. `Overview`, `Search` and `Details` are `[AllowAnonymous]` (a signed-in user is still recognised through the JWT). Every other action requires a signed-in user.
- Tests: `Veloci.Tests/TeamsTests.cs` runs the controller and service against in-memory SQLite.

## Not built

- No bell notification when someone is invited; the badge in the menu is the only signal.
- No ownership transfer between members, from the site or from the admin page.
