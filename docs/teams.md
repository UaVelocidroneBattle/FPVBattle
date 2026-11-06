# Teams

Signed-in users with a linked pilot can create teams and invite other pilots to them from the Teams page (`/teams`). Anyone, including visitors, can browse teams and their members.

A user belongs to at most one team. This is a rule, not a setting: the unique index on `TeamMembers.UserId` enforces it, and the API and UI deal with a single team (`TeamsOverviewModel.MyTeam`, `PilotProfileModel.Team`).

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
- The invite dialog suggests only pilots that can be invited right now: linked to an account, in no team, and not already invited.
- A pilot can hold invites from several teams. Accepting one checks `MaxTeamSize` again, because an invite can wait for a long time.
- An invite also puts a notification in the invitee's bell (`TeamInviteSentNotification`, handled by `InAppTeamInviteHandler`), next to the badge on the Teams menu item.

## Join requests

A pilot can also ask to join a team with "Ask to join" on its page.

- Invitations and join requests share one table, `TeamMembershipRequests` (`TeamMembershipRequest`). A join request is a row without an inviter (`InvitedByUserId` is null). `TeamMembershipRequest.IsJoinRequest` and the `Invitations()` / `JoinRequests()` query extensions keep the two apart. Accept and Decline only find invitations, so a pilot cannot accept their own request. The table was called `TeamInvites` before; the `RenameTeamInvitesToMembershipRequests` migration copied its rows across (`TeamMembershipRequestsMigrationTests`).
- Allowed for a user with a linked pilot who is in no team, to a public team that is not full. The race days rule for creating a team does not apply. At most `MaxPendingJoinRequests` requests can wait at once; the team page shows the reason when the button is greyed out (`JoinRequestBlockedReason`).
- The owner and admins see the requests on the team page and approve or reject them. The pilot sees their requests in the banner on the Teams page and can cancel them.
- Notifications (`InAppTeamJoinRequestHandler`): the owner and admins get one for a new request, the pilot gets one when it is approved.
- An invite to a pilot who has asked to join approves the request. A pilot the team has already invited cannot ask; the button points them to the invite instead.
- Joining a team in any way (accepting, approval, being added by a site admin, creating a team) drops all the user's other invites and requests.

## Limits

The `Teams` section of `appsettings.json` (documented in `appsettings.schema.json`):

| Setting | Meaning |
|---|---|
| `MaxTeamSize` | Members per team. 0 means no limit. The UI shows "x / limit" only when a limit is set. |
| `MinPublicTeamSize` | Members a team needs to be public. Default 3; 0 makes teams public from the start. See Visibility. |
| `MinRaceDaysToCreate` | Race days a pilot needs before they can create a team. 0 means no requirement. |
| `AllowPatronsToCreate` | Lets a paying Patreon supporter create a team without the race days. |
| `MaxPendingJoinRequests` | Join requests a user can have waiting at once. Default 3. |

Creating a team requires a linked pilot, no team yet, and either `MinRaceDaysToCreate` race days or a paying Patreon membership (when `AllowPatronsToCreate` is on). `TeamCreationPolicy` decides it; the Teams overview returns its reason as `CreateBlockedReason`, which the greyed-out Create team button shows on hover. Site admins creating a team from the admin page skip the race days rule.

## Names

Team names are unique ignoring case (the SQLite column uses the `NOCASE` collation) and at most 32 characters. The description is optional, at most 500. `NOCASE` folds only ASCII letters, so "Дрони" and "дрони" are treated as different names.

## Tags

A tag is a short label for the leaderboard, where the full name would not fit. The creator enters it when creating the team: 2 to 5 letters or digits, stored in upper case and unique ignoring case (`NOCASE` collation plus a unique index). The form upper-cases and filters the input as you type; the server validates again.

- Teams created before tags existed have no tag until an admin sets one.
- The owner and team admins change it in the edit dialog, together with the name and description. Site admins change it on the admin team page, which also lets them reclaim a squatted tag.
- Editing a team always requires a valid tag, so a team without one gets it the first time it is edited.
- `Teams:BlacklistedTags` in `appsettings.json` lists tags nobody can pick (compared ignoring case). A team that already holds a tag blacklisted later keeps it. The admin page applies the same rules.

Leaderboards (today's and the season's) show the tag of the pilot's team in brackets before the pilot name (`TeamTagLookup`).

A member of a team gets an All / My team switch above today's leaderboard. "My team" shows only the team's pilots, ranked together (flat), with their positions on the full board in brackets, and hides the country and flat filters. It works for teams that are not public yet too (`useMyTeamPilots`).

## Visibility

A team with fewer than `MinPublicTeamSize` members is not public, so a one- or two-pilot team does not expose who flies together. It is hidden from the teams list, its team page is not found by visitors and other users (members and invited pilots still see it, with a notice), its tag is not shown on leaderboards, and it is not listed on its members' pilot profiles. It becomes public by itself once it reaches the size; nothing is stored. The admin pages show every team that is not deleted.

## Privacy

Teams are public, so responses must never contain account display names or emails: they come from the Google profile and are usually real names. Members and inviters are identified by pilot name only. `TeamsTests.Responses_NeverContainDisplayNamesOrEmails` guards this.

## Team competition

The daily leaderboard has a team mode next to the pilots' one (`GET /api/competitions/team-overview?cupId&date`).

- **Who takes part**: public teams only (see Visibility).
- **Team time**: the average time of every member who flew the daily track, in ms. Slower pilots count too, on purpose: the team's time is everyone's business.
- **Minimum**: a team needs `MinTeamFinishers` members with a time to be ranked.
- **Quad of the day**: on a quad-of-the-day day only times on that quad count, so a faster quad can't improve a team's time.
- **Ranking and points**: one flat list per cup, no leagues, ordered by average time; equal averages go to the team with more pilots. Points come from the same table as pilots' (`PointsCalculator`).
- **Stored at the end of the day**: when a competition closes, `CompetitionConductor.StopAsync` stores the ranking in `TeamCompetitionResults` with membership as it is then, in the same save as the pilots' results, so the two are never stored apart. During the day the ranking is computed live from current membership, so a pilot who switches teams counts only for the team they are in at the close.
- **Pilots of a team**: each row lists the team's pilots and their times. They come from current membership, so on an old day they may not add up to the stored average; nothing more is stored.
- **Season**: the month's points per team, like the pilots' season leaderboard. A team that drops below the public size leaves today's board but keeps its season row.
- **Names**: results store the team id only, so past leaderboards show the current name and tag, just as they show a pilot's current name.

On the website a Pilots / Teams switch above the leaderboards (kept in the URL as `?teams=1`) replaces both boards with the team ones. A team row opens to show its pilots and their times; the user's own team is highlighted. Frontend: `src/pages/competition/TeamBoards.tsx`, `src/store/teamCompetitionStore.ts`.

Code: `Veloci.Logic/Features/Teams/Leaderboard` (`TeamLeaderboardCalculator` does the maths, `TeamRosterLookup` maps pilots to public teams, `TeamLeaderboardService` reads the leaderboards). Tests: `TeamLeaderboardCalculatorTests`, `TeamCompetitionTests`.

## Deletion

Deleting a team is soft. `Team.MarkDeleted` sets `IsDeleted` and removes the members and pending requests, so former members can join or create another team at once. The row stays so the team's past competition results keep its name and tag; they show it with `IsDeleted`.

A global query filter hides deleted teams, and their members, requests and results, from every query. Only the team leaderboard queries opt out with `IgnoreQueryFilters()`. The unique indexes on name and tag cover non-deleted teams only, so a deleted team's name and tag can be taken again.

## Account deletion

Before an account is deleted, a team the user owns goes to its longest-serving admin, then to its longest-serving member, and is deleted (softly, see Deletion) when nobody else is left (`TeamService.RemoveUserFromTeamAsync`).

## Admin page

The admin area has a Teams page (`/TeamManagement`, sidebar: Pilots > Teams). Admins act outside the team roles, so `TeamPermissions` does not apply, but the rules do: one team per user and `MaxTeamSize`. Pilots are always entered by exact, case-sensitive pilot name, and pages show pilot names only, never emails.

- **List**: team, owner, members (`x / limit` when a limit is set), pending invites, created date. Searchable and sortable.
- **Create a team**: the admin picks a pilot as the owner, who must be linked to an account and in no team.
- **Team page**: edit name and description (same rules as users), delete the team.
- **Members**: add a pilot straight into the team (as a plain member; their pending invite to it is dropped), make a member admin or member again, remove a member. The owner can be neither removed nor demoted.
- **Pending invites**: revoke. Join requests are listed here too, marked "Asked to join".

Code: `Veloci.Web/Controllers/Admin/Teams` (`TeamManagementController`), `Views/TeamManagement`. It is not named `TeamsController` because the API controller has that name, and the shared route value made links on the admin pages point at `/api/teams/...`. Tests: `TeamManagementTests`, on top of `TeamsTestBase`.

## Code

- Backend: `Veloci.Logic/Features/Teams` (`TeamService` runs the operations, `TeamPermissions` decides who may run them and is asked by the API controller, `TeamSettings`), `Veloci.Web/Controllers/Api/Teams`, domain `Team`, `TeamMember`, `TeamMembershipRequest`.
- Frontend: `src/pages/teams`, `src/store/teamsStore.ts`.
- API: `/api/teams/[action]`. `Overview`, `Search` and `Details` are `[AllowAnonymous]` (a signed-in user is still recognised through the JWT). Every other action requires a signed-in user.
- Tests: `Veloci.Tests/TeamsTests.cs` runs the controller and service against in-memory SQLite.

## Not built

- No ownership transfer between members, from the site or from the admin page.
