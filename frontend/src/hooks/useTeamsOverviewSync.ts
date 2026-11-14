import { useEffect } from "react";
import { useAuthStore } from "@/store/authStore";
import { useTeamsStore } from "@/store/teamsStore";

/**
 * Keeps the signed-in user's teams overview (and so the invitations badge in the menu)
 * fresh, and drops it when the session ends.
 */
export function useTeamsOverviewSync() {
    const userId = useAuthStore((state) => state.user?.id ?? null);

    useEffect(() => {
        const { fetchOverview, reset } = useTeamsStore.getState();

        if (!userId) {
            reset();
            return;
        }

        fetchOverview();
        window.addEventListener("focus", fetchOverview);
        return () => window.removeEventListener("focus", fetchOverview);
    }, [userId]);
}
