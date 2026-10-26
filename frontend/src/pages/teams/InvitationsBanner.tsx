import { useState, type ReactNode } from "react";
import { type ActionResult, useTeamsStore } from "@/store/teamsStore";
import MemberCount from "./MemberCount";
import { compactPrimaryButtonClass, compactSecondaryButtonClass } from "./teamStyles";
import { bleedOnPhones } from "@/lib/layout";

interface BannerRowProps {
    teamName: string;
    memberCount: number;
    maxTeamSize: number;
    note: string;
    children: ReactNode;
}

function BannerRow({ teamName, memberCount, maxTeamSize, note, children }: BannerRowProps) {
    return (
        <div className="flex flex-wrap items-center gap-4 border-b border-emerald-700/30 px-4 sm:px-6 py-3.5 last:border-b-0">
            <div className="min-w-56 flex-1">
                <div className="flex items-baseline gap-3">
                    <span className="font-semibold text-emerald-400">{teamName}</span>
                    <span className="text-slate-400">
                        <MemberCount count={memberCount} maxTeamSize={maxTeamSize} />
                    </span>
                </div>
                <div className="text-xs text-slate-400">{note}</div>
            </div>

            <div className="flex gap-2">{children}</div>
        </div>
    );
}

/** Invitations the user can answer, and the join requests they are still waiting on. */
function InvitationsBanner() {
    const invitations = useTeamsStore((state) => state.overview?.invitations);
    const joinRequests = useTeamsStore((state) => state.overview?.joinRequests);
    const maxTeamSize = useTeamsStore((state) => state.overview?.maxTeamSize ?? 0);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    if (!invitations?.length && !joinRequests?.length) return null;

    const run = async (action: () => Promise<ActionResult>) => {
        setBusy(true);
        setError(null);
        const result = await action();
        setBusy(false);

        if (!result.ok) setError(result.error ?? null);
    };

    const store = () => useTeamsStore.getState();

    return (
        <section className={`${bleedOnPhones} mb-4 sm:mb-6 border-y sm:border border-emerald-700/50 bg-emerald-950/30`}>
            {invitations?.map((invitation) => (
                <BannerRow
                    key={`invite-${invitation.teamId}`}
                    teamName={invitation.teamName}
                    memberCount={invitation.memberCount}
                    maxTeamSize={maxTeamSize}
                    note={invitation.invitedBy ? `Invited by ${invitation.invitedBy}` : "Invited"}
                >
                    <button
                        onClick={() => run(() => store().acceptInvitation(invitation.teamId))}
                        disabled={busy}
                        className={compactPrimaryButtonClass}
                    >
                        Accept
                    </button>
                    <button
                        onClick={() => run(() => store().declineInvitation(invitation.teamId))}
                        disabled={busy}
                        className={compactSecondaryButtonClass}
                    >
                        Decline
                    </button>
                </BannerRow>
            ))}

            {joinRequests?.map((request) => (
                <BannerRow
                    key={`request-${request.teamId}`}
                    teamName={request.teamName}
                    memberCount={request.memberCount}
                    maxTeamSize={maxTeamSize}
                    note="You asked to join"
                >
                    <button
                        onClick={() => run(() => store().cancelJoinRequest(request.teamId))}
                        disabled={busy}
                        className={compactSecondaryButtonClass}
                    >
                        Cancel request
                    </button>
                </BannerRow>
            ))}

            {error && <p className="px-4 sm:px-6 pb-3 text-sm text-red-400">{error}</p>}
        </section>
    );
}

export default InvitationsBanner;
