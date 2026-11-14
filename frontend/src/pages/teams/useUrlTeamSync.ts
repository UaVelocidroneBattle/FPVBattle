import { useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTeamsStore } from "@/store/teamsStore";

function parseTeamId(value: string | undefined): number | null {
    const teamId = Number(value);
    return value && Number.isInteger(teamId) && teamId > 0 ? teamId : null;
}

export function teamPath(teamId: number | null): string {
    return teamId === null ? "/teams" : `/teams/${teamId}`;
}

/**
 * Keeps the selected team in the URL (/teams/12), so a link opens that team.
 * Call it before the page loads its data: a team in the URL then wins over the
 * one the store remembers from an earlier visit.
 */
export function useUrlTeamSync() {
    const { teamId: teamIdParam } = useParams();
    const navigate = useNavigate();
    const urlTeamId = parseTeamId(teamIdParam);
    const selectedTeamId = useTeamsStore((state) => state.selectedTeamId);

    // URL → store: a shared link, or going back and forward in history
    useEffect(() => {
        const { selectedTeamId, selectTeam } = useTeamsStore.getState();
        if (urlTeamId !== null && urlTeamId !== selectedTeamId) selectTeam(urlTeamId);
    }, [urlTeamId]);

    // Store → URL. Reads the store at run time rather than the rendered value: on the
    // first render the effect above has just selected the team from the URL.
    useEffect(() => {
        const teamId = useTeamsStore.getState().selectedTeamId;
        if (teamId !== urlTeamId) navigate(teamPath(teamId), { replace: true });
    }, [selectedTeamId, navigate]);
}
