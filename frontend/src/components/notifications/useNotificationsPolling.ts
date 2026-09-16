import { useEffect } from "react";
import { useAuthStore } from "@/store/authStore";
import { useNotificationsStore } from "@/store/notificationsStore";

const POLL_INTERVAL_MS = 60_000;

export function useNotificationsPolling() {
    const userId = useAuthStore((state) => state.user?.id ?? null);

    useEffect(() => {
        const { fetchUnreadCount, reset } = useNotificationsStore.getState();

        if (!userId) {
            reset();
            return;
        }

        fetchUnreadCount();
        const interval = setInterval(fetchUnreadCount, POLL_INTERVAL_MS);
        window.addEventListener("focus", fetchUnreadCount);

        return () => {
            clearInterval(interval);
            window.removeEventListener("focus", fetchUnreadCount);
        };
    }, [userId]);
}
