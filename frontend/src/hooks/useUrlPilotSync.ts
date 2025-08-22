import { useEffect } from "react";
import { useSearchParams } from "react-router";
import { useShallow } from "zustand/shallow";
import { useSelectedPilotsStore } from "@/store/selectedPilotsStore";
import { usePilotsStore } from "@/store/pilotsStore";

/**
 * Custom hook for pilot selection with store-first architecture:
 *
 * 1. Store is the single source of truth for pilot selection
 * 2. URL params override store when present (for sharing/bookmarking)
 * 3. Store changes automatically update URL (for sharing)
 * 4. Store changes automatically trigger data fetching
 * 5. No URL params = store state unchanged (navigation persistence)
 */
export const useUrlPilotSync = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  const { pilots: selectedPilots, setPilots } = useSelectedPilotsStore(
    useShallow((state) => ({
      pilots: state.pilots,
      setPilots: state.setPilots,
    }))
  );

  const { pilots: availablePilots, state: pilotsState } = usePilotsStore(
    useShallow((state) => ({
      pilots: state.pilots,
      state: state.state,
    }))
  );

  // Effect 1: URL → Store (when URL has pilots)
  useEffect(() => {
    if (pilotsState !== "Loaded" || availablePilots.length === 0) return;

    const urlPilot1 = searchParams.get("pilot1");
    const urlPilot2 = searchParams.get("pilot2");
    const urlPilots = [urlPilot1, urlPilot2].filter(
      (p): p is string => p !== null && availablePilots.includes(p)
    );

    // Only update store if URL has pilots
    if (urlPilots.length > 0) {
      const newPilots: (string | null)[] = [null, null];
      urlPilots.forEach((pilot, index) => {
        newPilots[index] = pilot;
      });
      const finalPilots = newPilots.slice(0, Math.max(urlPilots.length, 1));
      setPilots(finalPilots);
    }
  }, [pilotsState, availablePilots, searchParams, setPilots]);

  // Effect 2: Store → URL (for sharing/bookmarking)
  useEffect(() => {
    const params = new URLSearchParams();
    const validPilots = selectedPilots.filter((p) => p !== null);

    if (validPilots[0]) params.set("pilot1", validPilots[0]);
    if (validPilots[1]) params.set("pilot2", validPilots[1]);

    // if no pilots are selected, and URL params are present skip "reseting" update
    // because it is the first render, and store is not populated yet
    if (params.size === 0 && searchParams.size != 0) return;

    setSearchParams(params);
  }, [selectedPilots, setSearchParams, searchParams]);
};
