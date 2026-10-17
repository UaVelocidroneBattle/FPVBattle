import { useState } from "react";
import { useTeamsStore } from "@/store/teamsStore";
import MemberCount from "./MemberCount";
import { compactPrimaryButtonClass, compactSecondaryButtonClass } from "./teamStyles";

function InvitationsBanner() {
    const invitations = useTeamsStore((state) => state.overview?.invitations);
    const maxTeamSize = useTeamsStore((state) => state.overview?.maxTeamSize ?? 0);
    const [busyTeamId, setBusyTeamId] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);

    if (!invitations?.length) return null;

    const respond = async (teamId: number, accept: boolean) => {
        const { acceptInvitation, declineInvitation } = useTeamsStore.getState();

        setBusyTeamId(teamId);
        setError(null);
        const result = await (accept ? acceptInvitation(teamId) : declineInvitation(teamId));
        setBusyTeamId(null);

        if (!result.ok) setError(result.error ?? null);
    };

    return (
        <section className="mb-6 border border-emerald-700/50 bg-emerald-950/30">
            {invitations.map((invitation) => (
                <div
                    key={invitation.teamId}
                    className="flex flex-wrap items-center gap-4 border-b border-emerald-700/30 px-6 py-3.5 last:border-b-0"
                >
                    <div className="min-w-56 flex-1">
                        <div className="flex items-baseline gap-3">
                            <span className="font-semibold text-emerald-400">{invitation.teamName}</span>
                            <span className="text-slate-400">
                                <MemberCount count={invitation.memberCount} maxTeamSize={maxTeamSize} />
                            </span>
                        </div>
                        <div className="text-xs text-slate-400">
                            {invitation.invitedBy ? `Invited by ${invitation.invitedBy}` : "Invited"}
                        </div>
                    </div>

                    <div className="flex gap-2">
                        <button
                            onClick={() => respond(invitation.teamId, true)}
                            disabled={busyTeamId !== null}
                            className={compactPrimaryButtonClass}
                        >
                            Accept
                        </button>
                        <button
                            onClick={() => respond(invitation.teamId, false)}
                            disabled={busyTeamId !== null}
                            className={compactSecondaryButtonClass}
                        >
                            Decline
                        </button>
                    </div>
                </div>
            ))}

            {error && <p className="px-6 pb-3 text-sm text-red-400">{error}</p>}
        </section>
    );
}

export default InvitationsBanner;
