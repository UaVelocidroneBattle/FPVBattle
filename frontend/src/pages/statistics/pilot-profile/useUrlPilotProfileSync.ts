import { useEffect } from "react";
import { useSearchParams } from "react-router";
import { useShallow } from "zustand/shallow";
import { usePilotProfileStore } from "@/store/pilotProfileStore";
import { usePilotsStore } from "@/store/pilotsStore";

/**
 * Custom hook for pilot profile selection with store-first architecture:
 *
 * 1. Store is the single source of truth for pilot selection
 * 2. URL param overrides store when present (for sharing/bookmarking)
 * 3. Store changes automatically update URL (for sharing)
 * 4. Store changes automatically trigger profile/data fetching
 * 5. No URL param = store state unchanged (navigation persistence)
 */
export const useUrlPilotProfileSync = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  const { currentPilot, choosePilot } = usePilotProfileStore(
    useShallow((state) => ({
      currentPilot: state.currentPilot,
      choosePilot: state.choosePilot,
    }))
  );

  const { pilots: availablePilots, state: pilotsState } = usePilotsStore(
    useShallow((state) => ({
      pilots: state.pilots,
      state: state.state,
    }))
  );

  // Effect 1: URL → Store (when URL has pilot)
  useEffect(() => {
    if (pilotsState !== "Loaded" || availablePilots.length === 0) return;

    const urlPilot = searchParams.get("pilot");

    // Only update store if URL has valid pilot and it's different from current
    if (urlPilot && availablePilots.includes(urlPilot)) {
      choosePilot(urlPilot);
    }
  }, [pilotsState, availablePilots, searchParams, choosePilot]);

  // Effect 2: Store → URL (for sharing/bookmarking)
  useEffect(() => {
    const params = new URLSearchParams();

    if (currentPilot) {
      params.set("pilot", currentPilot);
    }

    // Prevent clearing URL on initial render when store is empty but URL has pilot
    if (!currentPilot && searchParams.has("pilot")) return;

    setSearchParams(params);
  }, [currentPilot, setSearchParams, searchParams]);
};
