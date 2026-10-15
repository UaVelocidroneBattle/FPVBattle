import { useEffect, useState } from "react";
import type { TeamSummaryModel } from "@/api/client";
import { useAuthStore } from "@/store/authStore";
import { useTeamsStore } from "@/store/teamsStore";
import MemberCount from "./MemberCount";
import { compactPrimaryButtonClass, inputClass, panelClass, sectionTitleClass } from "./teamStyles";

const SEARCH_DEBOUNCE_MS = 250;

interface TeamListItemProps {
    team: TeamSummaryModel;
    maxTeamSize: number;
}

function TeamListItem({ team, maxTeamSize }: TeamListItemProps) {
    const selected = useTeamsStore((state) => state.selectedTeamId === team.id);

    return (
        <button
            onClick={() => useTeamsStore.getState().selectTeam(team.id)}
            className={`flex w-full items-baseline justify-between gap-2 border-l-[3px] px-4 py-3 text-left transition-colors hover:bg-slate-700/40 ${
                selected ? "border-emerald-400 bg-slate-700/50" : "border-transparent"
            }`}
        >
            <span className="min-w-0 truncate font-semibold text-white">{team.name}</span>
            <span className="text-sm text-slate-400">
                <MemberCount count={team.memberCount} maxTeamSize={maxTeamSize} />
            </span>
        </button>
    );
}

interface OtherTeamsProps {
    title: string;
    /** Draws a line above the block, to set it apart from the user's own team. */
    separated: boolean;
    maxTeamSize: number;
}

function OtherTeams({ title, separated, maxTeamSize }: OtherTeamsProps) {
    const otherTeams = useTeamsStore((state) => state.otherTeams);
    const appliedQuery = useTeamsStore((state) => state.searchQuery);
    const [query, setQuery] = useState(appliedQuery);

    useEffect(() => {
        const timer = setTimeout(() => {
            const trimmed = query.trim();
            if (trimmed !== useTeamsStore.getState().searchQuery) useTeamsStore.getState().search(trimmed);
        }, SEARCH_DEBOUNCE_MS);

        return () => clearTimeout(timer);
    }, [query]);

    return (
        <div className={`pt-4 ${separated ? "border-t border-slate-700" : ""}`}>
            <h2 className={`${sectionTitleClass} px-4`}>{title}</h2>

            <div className="px-4 pb-5 pt-3">
                <input
                    type="search"
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    placeholder="Search teams"
                    className={inputClass}
                />
            </div>

            {otherTeams.map((team) => (
                <TeamListItem key={team.id} team={team} maxTeamSize={maxTeamSize} />
            ))}

            {appliedQuery && otherTeams.length === 0 && (
                <p className="px-4 pb-4 text-xs text-slate-500">No team matches “{appliedQuery}”</p>
            )}
        </div>
    );
}

interface TeamsSidebarProps {
    /** Present only when the user can create a team right now. */
    onCreateTeam?: () => void;
}

function TeamsSidebar({ onCreateTeam }: TeamsSidebarProps) {
    const signedIn = useAuthStore((state) => state.user !== null);
    const myTeam = useTeamsStore((state) => state.overview?.myTeam);
    const maxTeamSize = useTeamsStore((state) => state.overview?.maxTeamSize ?? 0);

    return (
        <aside className={panelClass}>
            {signedIn && (
                <div className={`pt-4 ${myTeam ? "" : "pb-4"}`}>
                    <div className={`flex min-h-[30px] items-center justify-between gap-2 px-4 ${myTeam ? "mb-4" : ""}`}>
                        <h2 className={sectionTitleClass}>Your team</h2>
                        {onCreateTeam && (
                            <button onClick={onCreateTeam} className={compactPrimaryButtonClass}>
                                Create team
                            </button>
                        )}
                    </div>
                    {myTeam && <TeamListItem team={myTeam} maxTeamSize={maxTeamSize} />}
                </div>
            )}

            <OtherTeams title={signedIn ? "Other teams" : "Teams"} separated={signedIn} maxTeamSize={maxTeamSize} />
        </aside>
    );
}

export default TeamsSidebar;
