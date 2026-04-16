import { useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import { getDashboardStore } from "@/store/dashboardStore";

const DATE_PARAM = "date";

/**
 * Syncs the dashboard's selected date with the URL search param:
 *
 * 1. Store is the single source of truth for the selected date
 * 2. URL param overrides store on mount (for sharing/bookmarking)
 * 3. Store changes automatically update the URL (for sharing)
 * 4. No URL param = store state unchanged (navigation persistence)
 */
export function useUrlDateSync(cupId: string) {
  const [searchParams, setSearchParams] = useSearchParams();
  const useStore = getDashboardStore(cupId);

  const selectedDate = useStore((state) => state.selectedDate);

  // Effect 1: URL → Store (on mount / when URL date changes)
  useEffect(() => {
    const urlDate = searchParams.get(DATE_PARAM);
    const { selectDate, selectedDate: currentDate } = getDashboardStore(cupId).getState();

    if (urlDate && urlDate !== currentDate) {
      selectDate(urlDate);
    }
  }, [cupId, searchParams]);

  // Effect 2: Store → URL (for sharing/bookmarking)
  // Always replace — date is filter state, not page navigation.
  // Pushing would add history entries on every date change, requiring extra
  // back-button presses to leave the dashboard.
  useEffect(() => {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      if (selectedDate) {
        next.set(DATE_PARAM, selectedDate);
      } else {
        next.delete(DATE_PARAM);
      }
      return next;
    }, { replace: true });
  }, [selectedDate, setSearchParams]);
}
