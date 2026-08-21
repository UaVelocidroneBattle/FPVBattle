import { useEffect } from "react";
import { useCupsStore, CupModel } from "@/store/cupsStore";

interface UseCupsResult {
    cups: CupModel[];
    loadingState: ReturnType<typeof useCupsStore.getState>["loadingState"];
    /** The cup to fall back on when none is selected, e.g. when redirecting. */
    defaultCup: CupModel | undefined;
    findCup: (cupId: string | undefined) => CupModel | undefined;
}

/**
 * Cups as the backend configures them. Any component that needs cup ids or
 * names can call this — the underlying fetch happens once per session.
 */
export function useCups(): UseCupsResult {
    const cups = useCupsStore((state) => state.cups);
    const loadingState = useCupsStore((state) => state.loadingState);

    useEffect(() => {
        useCupsStore.getState().fetchCups();
    }, []);

    return {
        cups,
        loadingState,
        defaultCup: cups[0],
        findCup: (cupId) => cups.find((cup) => cup.id === cupId),
    };
}
