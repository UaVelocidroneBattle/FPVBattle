import type { TeamPendingInviteModel } from "@/api/client";
import { formatDate } from "@/lib/utils";
import { sectionTitleClass } from "./teamStyles";

interface PendingInvitesProps {
    invites: TeamPendingInviteModel[];
    onRevoke: (invite: TeamPendingInviteModel) => void;
}

function PendingInvites({ invites, onRevoke }: PendingInvitesProps) {
    return (
        <div className="mt-7">
            <h3 className={`${sectionTitleClass} mb-2.5`}>Pending invites · {invites.length}</h3>
            <ul>
                {invites.map((invite) => (
                    <li
                        key={invite.userId}
                        className="flex items-center gap-3 border-t border-slate-700 py-2 text-sm first:border-t-0"
                    >
                        <span className="flex-1">
                            <span className="font-semibold text-emerald-400">{invite.pilotName ?? "-"}</span>{" "}
                            <span className="text-xs text-slate-500">
                                · invited {invite.invitedBy && `by ${invite.invitedBy} `}on {formatDate(invite.invitedOn)}
                            </span>
                        </span>
                        <button
                            onClick={() => onRevoke(invite)}
                            className="px-2.5 py-1 text-sm text-slate-400 transition-colors hover:text-emerald-400"
                        >
                            Revoke
                        </button>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default PendingInvites;
