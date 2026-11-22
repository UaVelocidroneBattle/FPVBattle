import { useEffect } from "react";
import { useAuthStore } from "@/store/authStore";
import { useTrackSchedulingStore } from "@/store/trackSchedulingStore";

/** Loads whether the current user may schedule tracks, again whenever they sign in or out. */
export function useTrackSchedulingSync() {
    const userId = useAuthStore((state) => state.user?.id ?? null);

    useEffect(() => {
        useTrackSchedulingStore.getState().fetchOverview(userId);
    }, [userId]);
}
