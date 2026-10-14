import { useEffect, useState } from "react";
import { getApiTeamsDetails } from "@/api/client";
import { useTeamsStore } from "@/store/teamsStore";

/**
 * Names of the pilots in the signed-in user's teams, or null when the user is in
 * no team (or the members are not loaded yet). Teams that are not public yet
 * count too: their members can always see them.
 */
export function useMyTeamPilots(): Set<string> | null {
    const teamIdsKey = useTeamsStore((state) => state.overview?.myTeams.map((team) => team.id).join(",") ?? "");
    const [pilots, setPilots] = useState<Set<string> | null>(null);

    useEffect(() => {
        if (!teamIdsKey) {
            setPilots(null);
            return;
        }

        let cancelled = false;
        const teamIds = teamIdsKey.split(",").map(Number);

        Promise.all(teamIds.map((teamId) => getApiTeamsDetails({ query: { teamId } })))
            .then((responses) => {
                if (cancelled) return;

                const names = responses
                    .flatMap((response) => response.data?.members ?? [])
                    .map((member) => member.pilotName)
                    .filter((name): name is string => !!name);
                setPilots(new Set(names));
            })
            .catch(() => {
                if (!cancelled) setPilots(null);
            });

        return () => {
            cancelled = true;
        };
    }, [teamIdsKey]);

    return pilots;
}
