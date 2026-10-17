import { useEffect, useState } from "react";
import { getApiTeamsDetails } from "@/api/client";
import { useTeamsStore } from "@/store/teamsStore";

/**
 * Names of the pilots in the signed-in user's team, or null when the user is in
 * no team (or the members are not loaded yet). A team that is not public yet
 * counts too: its members can always see it.
 */
export function useMyTeamPilots(): Set<string> | null {
    const teamId = useTeamsStore((state) => state.overview?.myTeam?.id);
    const [pilots, setPilots] = useState<Set<string> | null>(null);

    useEffect(() => {
        if (teamId === undefined) {
            setPilots(null);
            return;
        }

        let cancelled = false;

        getApiTeamsDetails({ query: { teamId } })
            .then((response) => {
                if (cancelled) return;

                const names = (response.data?.members ?? [])
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
    }, [teamId]);

    return pilots;
}
