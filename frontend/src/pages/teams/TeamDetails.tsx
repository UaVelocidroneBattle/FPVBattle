import { useState } from "react";
import type { TeamMemberModel, TeamPendingInviteModel, TeamRole } from "@/api/client";
import ActionMenu, { type ActionMenuItem } from "@/components/ui/ActionMenu";
import ConfirmDialog from "@/components/ui/ConfirmDialog";
import { Spinner } from "@/components/ui/spinner";
import { useAuthStore } from "@/store/authStore";
import { type ActionResult, useTeamsStore } from "@/store/teamsStore";
import InviteDialog from "./InviteDialog";
import MembersTable from "./MembersTable";
import PendingInvites from "./PendingInvites";
import TeamFormDialog from "./TeamFormDialog";
import { panelClass, sectionTitleClass } from "./teamStyles";

type PendingConfirmation =
    | { kind: "delete" }
    | { kind: "leave" }
    | { kind: "remove"; member: TeamMemberModel };

function TeamDetails() {
    const details = useTeamsStore((state) => state.details);
    const detailsState = useTeamsStore((state) => state.detailsState);
    const maxTeamSize = useTeamsStore((state) => state.overview?.maxTeamSize ?? 0);
    const minPublicTeamSize = useTeamsStore((state) => state.overview?.minPublicTeamSize ?? 0);
    const currentUserId =useAuthStore((state) => state.user?.id);
    const [inviting, setInviting] = useState(false);
    const [editing, setEditing] = useState(false);
    const [confirmation, setConfirmation] = useState<PendingConfirmation | null>(null);
    const [error, setError] = useState<string | null>(null);

    if (detailsState === "Error") {
        return <p className="text-sm text-red-400">Failed to load the team.</p>;
    }

    if (!details) return <Spinner />;

    const { myRole } = details;
    const canManage = myRole === "Owner" || myRole === "Admin";
    const isFull = maxTeamSize > 0 && details.memberCount >= maxTeamSize;
    const isHidden = details.memberCount < minPublicTeamSize;
    const teamId = details.id;

    const run = async (action: Promise<ActionResult>) => {
        setError(null);
        const result = await action;
        if (!result.ok) setError(result.error ?? null);
    };

    const confirmed = async (action: Promise<ActionResult>): Promise<string | null> => {
        const result = await action;
        return result.ok ? null : (result.error ?? "Something went wrong. Please try again.");
    };

    const store = () => useTeamsStore.getState();

    const teamMenu: ActionMenuItem[] = [];
    if (canManage) {
        teamMenu.push({ label: "Edit team", onSelect: () => setEditing(true) });
        teamMenu.push({
            label: "Invite a pilot",
            onSelect: () => setInviting(true),
            disabled: isFull,
            title: isFull ? "The team is full" : undefined,
        });
    }
    if (myRole === "Owner") {
        teamMenu.push({ label: "Delete team", danger: true, onSelect: () => setConfirmation({ kind: "delete" }) });
    } else if (myRole) {
        teamMenu.push({ label: "Leave team", danger: true, onSelect: () => setConfirmation({ kind: "leave" }) });
    }

    const confirmationDialog = () => {
        switch (confirmation?.kind) {
            case "delete":
                return {
                    title: `Delete “${details.name}”?`,
                    description:
                        "This removes the team, all memberships and pending invites. This can’t be undone.",
                    confirmLabel: "Delete team",
                    onConfirm: () => confirmed(store().removeTeam(teamId)),
                };
            case "leave":
                return {
                    title: `Leave “${details.name}”?`,
                    description: "You will need a new invitation to join again.",
                    confirmLabel: "Leave team",
                    onConfirm: () => confirmed(store().leaveTeam(teamId)),
                };
            case "remove":
                return {
                    title: `Remove ${confirmation.member.pilotName ?? "this member"}?`,
                    description: "They will lose access to the team and need a new invitation to return.",
                    confirmLabel: "Remove",
                    onConfirm: () => confirmed(store().removeMember(teamId, confirmation.member.userId)),
                };
            default:
                return null;
        }
    };
    const dialog = confirmationDialog();

    return (
        <section className={`${panelClass} p-6`}>
            <div className="flex items-start justify-between gap-4">
                <h2 className="flex flex-wrap items-center gap-3 text-[22px] font-bold text-white">
                    {details.name}
                    {details.tag && (
                        <span className="border border-emerald-500 px-2 py-0.5 font-mono text-sm font-semibold tracking-wider text-emerald-400">
                            {details.tag}
                        </span>
                    )}
                </h2>
                {teamMenu.length > 0 && <ActionMenu label="Team actions" items={teamMenu} />}
            </div>

            {details.description && <p className="mt-3.5 max-w-prose text-sm text-slate-400">{details.description}</p>}

            {isHidden && (
                <p className="mt-3.5 max-w-prose border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-300">
                    Only its members and invited pilots can see this team until it has {minPublicTeamSize} members.
                    It is not listed for others, and its tag is not shown on leaderboards or pilot profiles.
                </p>
            )}

            {!myRole && (
                <p className="mt-3.5 text-xs text-slate-500">
                    You can join a team only through an invitation from its owner or an admin.
                </p>
            )}

            <div className="mt-7">
                <h3 className={`${sectionTitleClass} mb-3 flex items-center gap-3.5`}>
                    <span>Members · {maxTeamSize > 0 ? `${details.memberCount} / ${maxTeamSize}` : details.memberCount}</span>
                    {isFull && <span className="text-[13px] font-medium normal-case tracking-normal text-amber-400">Team is full</span>}
                </h3>

                {error && <p className="mb-3 text-sm text-red-400">{error}</p>}

                <MembersTable
                    members={details.members}
                    myRole={myRole}
                    currentUserId={currentUserId}
                    onSetRole={(member: TeamMemberModel, role: TeamRole) =>
                        run(store().setMemberRole(teamId, member.userId, role))
                    }
                    onRemove={(member) => setConfirmation({ kind: "remove", member })}
                />
            </div>

            {canManage && details.pendingInvites.length > 0 && (
                <PendingInvites
                    invites={details.pendingInvites}
                    onRevoke={(invite: TeamPendingInviteModel) => run(store().revokeInvitation(teamId, invite.userId))}
                />
            )}

            <InviteDialog open={inviting} teamId={teamId} onClose={() => setInviting(false)} />

            <TeamFormDialog open={editing} team={details} onClose={() => setEditing(false)} />

            <ConfirmDialog
                open={dialog !== null}
                title={dialog?.title ?? ""}
                description={dialog?.description ?? ""}
                confirmLabel={dialog?.confirmLabel ?? ""}
                onConfirm={dialog?.onConfirm ?? (async () => null)}
                onClose={() => setConfirmation(null)}
            />
        </section>
    );
}

export default TeamDetails;
