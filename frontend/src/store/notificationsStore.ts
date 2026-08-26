import { create } from "zustand";
import {
    getApiNotifications,
    getApiNotificationsUnreadCount,
    postApiNotificationsRead,
    postApiNotificationsReadAll,
} from "@/api/client";
import type { NotificationModel } from "@/api/client";
import { LoadingStates } from "@/lib/loadingStates";

interface NotificationsState {
    items: NotificationModel[];
    unreadCount: number;
    state: LoadingStates;
}

interface NotificationsActions {
    fetchUnreadCount: () => Promise<void>;
    fetchItems: () => Promise<void>;
    markRead: (ids: number[]) => Promise<void>;
    markAllRead: () => Promise<void>;
    /** Drops another user's notifications when the session ends. */
    reset: () => void;
}

type NotificationsStore = NotificationsState & NotificationsActions;

const initialState: NotificationsState = {
    items: [],
    unreadCount: 0,
    state: "Idle",
};

export const useNotificationsStore = create<NotificationsStore>()((set, get) => ({
    ...initialState,

    fetchUnreadCount: async () => {
        try {
            const response = await getApiNotificationsUnreadCount();
            if (response.data) {
                set({ unreadCount: response.data.unreadCount });
            }
        } catch {
            // Keep the current badge; the next poll will catch up
        }
    },

    fetchItems: async () => {
        if (get().state === "Loading") return;

        set({ state: "Loading" });
        try {
            const response = await getApiNotifications();
            set(response.data
                ? { items: response.data.items, unreadCount: response.data.unreadCount, state: "Loaded" }
                : { state: "Error" });
        } catch {
            set({ state: "Error" });
        }
    },

    markRead: async (ids: number[]) => {
        if (ids.length === 0) return;

        try {
            const response = await postApiNotificationsRead({ body: { ids } });
            const readOn = new Date().toISOString();

            set((state) => ({
                items: state.items.map((item) =>
                    item.readOn ? item : ids.includes(item.id) ? { ...item, readOn } : item),
                unreadCount: response.data?.unreadCount ?? state.unreadCount,
            }));
        } catch {
            // Leave the item unread; the next poll will show the real state
        }
    },

    markAllRead: async () => {
        try {
            const response = await postApiNotificationsReadAll();
            const readOn = new Date().toISOString();

            set((state) => ({
                items: state.items.map((item) => item.readOn ? item : { ...item, readOn }),
                unreadCount: response.data?.unreadCount ?? 0,
            }));
        } catch {
            // Leave the badge as is; the next poll will show the real state
        }
    },

    reset: () => set(initialState),
}));
