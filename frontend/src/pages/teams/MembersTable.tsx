import type { TeamMemberModel, TeamRole } from "@/api/client";
import PilotName from "@/components/PilotName";
import ActionMenu, { type ActionMenuItem } from "@/components/ui/ActionMenu";
import CountryFlag from "@/components/ui/CountryFlag";
import { formatDate } from "@/lib/utils";
import { roleNameClass } from "./teamStyles";

interface MembersTableProps {
    members: TeamMemberModel[];
    myRole: TeamRole | null | undefined;
    currentUserId: string | undefined;
    onSetRole: (member: TeamMemberModel, role: TeamRole) => void;
    onRemove: (member: TeamMemberModel) => void;
}

const headerCellClass = "px-2 pb-2 text-left text-xs font-medium uppercase tracking-wide text-slate-500";

function MembersTable({ members, myRole, currentUserId, onSetRole, onRemove }: MembersTableProps) {
    const actionsFor = (member: TeamMemberModel): ActionMenuItem[] => {
        if (member.userId === currentUserId || member.role === "Owner") return [];

        const items: ActionMenuItem[] = [];

        if (myRole === "Owner") {
            const promoted = member.role === "Member";
            items.push({
                label: promoted ? "Make admin" : "Demote",
                onSelect: () => onSetRole(member, promoted ? "Admin" : "Member"),
            });
        }

        if (myRole === "Owner" || (myRole === "Admin" && member.role === "Member")) {
            items.push({ label: "Remove", danger: true, onSelect: () => onRemove(member) });
        }

        return items;
    };

    const hasActions = members.some((member) => actionsFor(member).length > 0);

    // The pilot column takes whatever the fixed columns leave, so no gap opens up after them.
    return (
        <table className="w-full table-fixed text-sm">
            <colgroup>
                <col />
                <col className="w-20" />
                <col className="hidden w-28 sm:table-column" />
                {hasActions && <col className="w-10" />}
            </colgroup>
            <thead>
                <tr>
                    <th className={headerCellClass}>Pilot</th>
                    <th className={headerCellClass}>Role</th>
                    <th className={`${headerCellClass} hidden text-right sm:table-cell`}>Joined</th>
                    {hasActions && <th className={headerCellClass} />}
                </tr>
            </thead>
            <tbody>
                {members.map((member) => {
                    const actions = actionsFor(member);

                    return (
                        <tr
                            key={member.userId}
                            className={`border-t border-slate-700 ${member.userId === currentUserId ? "bg-emerald-500/5" : ""}`}
                        >
                            <td className={`px-2 py-2.5 font-semibold ${roleNameClass[member.role]}`}>
                                {/* The flag slot keeps its width without a country, so the names stay aligned. */}
                                <div className="flex min-w-0 items-center gap-2">
                                    <span className="w-5 shrink-0">
                                        {member.country && <CountryFlag countryCode={member.country} />}
                                    </span>
                                    {member.pilotName ? <PilotName name={member.pilotName} className="truncate" /> : "-"}
                                </div>
                            </td>
                            <td className="truncate px-2 py-2.5 text-slate-400">{member.role}</td>
                            <td className="hidden px-2 py-2.5 text-right text-slate-400 sm:table-cell">
                                {formatDate(member.joinedOn)}
                            </td>
                            {hasActions && (
                                <td className="py-2.5 text-right">
                                    {actions.length > 0 && <ActionMenu label={`Actions for ${member.pilotName ?? "this member"}`} items={actions} />}
                                </td>
                            )}
                        </tr>
                    );
                })}
            </tbody>
        </table>
    );
}

export default MembersTable;
