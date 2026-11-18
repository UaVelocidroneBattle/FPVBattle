# User Accounts and Pilot Linking

Visitors can browse everything on the dashboard without an account. An account adds a profile, the notification bell, and [teams](teams.md), and it ties the person to their Velocidrone pilot.

## Sign-in

- Google is the only sign-in method. The frontend gets a Google ID token, and `GoogleAuthService` validates it (`IGoogleTokenValidator`) and finds or creates the `ApplicationUser`.
- The API then issues a short-lived JWT access token (15 minutes) and a refresh token (60 days) (`TokenService`, `Auth` settings).
- Refresh tokens rotate: each refresh revokes the old token and issues a new pair. Signing out revokes the current token.
- The admin dashboard has its own ASP.NET Identity login, separate from the dashboard's Google sign-in.

## Linking a pilot: fly-to-verify

A user proves they own a Velocidrone pilot by flying with it (`PilotBindingService`):

1. On the profile page the user claims a pilot name. Names are case sensitive. The claim is refused when:
   - the user already has a pilot,
   - the pilot is linked to someone else, or
   - another user has a pending claim on the same name.
2. The claim stays open for 48 hours (`PilotClaim.ExpiresOn`). The pilot doesn't need to exist in FPV Battle yet.
3. When that pilot posts a time on any daily track, `PilotClaimResolutionHandler` (on `CurrentResultUpdated`) links the pilot to the user and removes the claim.
4. Expired claims are removed nightly (04:30 UTC, `ExpiredClaimCleanupJob`).

Admins can see pending claims on the Pilot Claims page. On the Users page they can link or unlink a pilot directly, bypassing verification.

## Deleting an account

Users can permanently delete their account from the profile page (`DELETE /api/profile/account`). This:
- removes their email, profile, sign-in methods, sessions and pending claim,
- ends their team membership, and
- **keeps** the pilot and its race history, which are community competition data that stay on the leaderboards.
