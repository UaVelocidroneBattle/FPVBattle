import { useEffect, useState } from "react";
import { getApiTeamsInviteCandidates } from "@/api/client";
import type { PilotSuggestionModel } from "@/api/client";

const DEBOUNCE_MS = 200;
const MIN_QUERY_LENGTH = 2;

/**
 * Pilots that can be invited to the team and whose name contains the typed text.
 * Nothing is fetched while `enabled` is false or the text is too short.
 */
export function usePilotSuggestions(teamId: number, text: string, enabled: boolean): PilotSuggestionModel[] {
    const [suggestions, setSuggestions] = useState<PilotSuggestionModel[]>([]);

    useEffect(() => {
        const query = text.trim();

        if (!enabled || query.length < MIN_QUERY_LENGTH) {
            setSuggestions([]);
            return;
        }

        let outdated = false;
        const timer = setTimeout(async () => {
            try {
                const response = await getApiTeamsInviteCandidates({ query: { teamId, query } });
                if (!outdated) setSuggestions(response.data ?? []);
            } catch {
                if (!outdated) setSuggestions([]);
            }
        }, DEBOUNCE_MS);

        return () => {
            outdated = true;
            clearTimeout(timer);
        };
    }, [teamId, text, enabled]);

    return suggestions;
}
