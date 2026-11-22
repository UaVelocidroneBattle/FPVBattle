import { useEffect, useState } from "react";
import { getApiTrackSchedulingSuggestions } from "@/api/client";
import type { SchedulableTrackModel } from "@/api/client";

const DEBOUNCE_MS = 200;
const MIN_TERM_LENGTH = 2;

/**
 * Tracks of the cup whose name contains the typed text, each with the reason it can't be scheduled, if any.
 * Nothing is fetched while `enabled` is false or the text is too short.
 */
export function useTrackSuggestions(cupId: string, text: string, enabled: boolean): SchedulableTrackModel[] {
    const [suggestions, setSuggestions] = useState<SchedulableTrackModel[]>([]);

    useEffect(() => {
        const term = text.trim();

        if (!enabled || term.length < MIN_TERM_LENGTH) {
            setSuggestions([]);
            return;
        }

        // Cleanup marks the request outdated, so an answer to older text never replaces a newer one
        let outdated = false;
        const timer = setTimeout(async () => {
            try {
                const response = await getApiTrackSchedulingSuggestions({ query: { cupId, term } });
                if (!outdated) setSuggestions(response.data ?? []);
            } catch {
                if (!outdated) setSuggestions([]);
            }
        }, DEBOUNCE_MS);

        return () => {
            outdated = true;
            clearTimeout(timer);
        };
    }, [cupId, text, enabled]);

    return suggestions;
}
