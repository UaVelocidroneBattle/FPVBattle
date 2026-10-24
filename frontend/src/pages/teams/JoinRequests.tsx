import type { TeamPendingInviteModel } from "@/api/client";
import { formatDate } from "@/lib/utils";
import { sectionTitleClass } from "./teamStyles";

interface JoinRequestsProps {
    requests: TeamPendingInviteModel[];
    onApprove: (request: TeamPendingInviteModel) => void;
    onReject: (request: TeamPendingInviteModel) => void;
}

function JoinRequests({ requests, onApprove, onReject }: JoinRequestsProps) {
    return (
        <div className="mt-7">
            <h3 className={`${sectionTitleClass} mb-2.5`}>Join requests · {requests.length}</h3>
            <ul>
                {requests.map((request) => (
                    <li
                        key={request.userId}
                        className="flex items-center gap-3 border-t border-slate-700 py-2 text-sm first:border-t-0"
                    >
                        <span className="flex-1">
                            <span className="font-semibold text-emerald-400">{request.pilotName ?? "-"}</span>{" "}
                            <span className="text-xs text-slate-500"><span className="mx-1.5">·</span>asked on {formatDate(request.invitedOn)}</span>
                        </span>
                        <button
                            onClick={() => onApprove(request)}
                            className="px-2.5 py-1 text-sm text-emerald-400 transition-colors hover:text-emerald-300"
                        >
                            Approve
                        </button>
                        <button
                            onClick={() => onReject(request)}
                            className="px-2.5 py-1 text-sm text-slate-400 transition-colors hover:text-emerald-400"
                        >
                            Reject
                        </button>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default JoinRequests;
