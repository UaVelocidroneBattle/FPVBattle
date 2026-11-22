import { create } from "zustand";
import { getApiTrackSchedulingOverview, postApiTrackSchedulingCancel, postApiTrackSchedulingSchedule } from "@/api/client";
import type { QueuedTrackModel, TrackSchedulingOverviewModel } from "@/api/client";
import { LoadingStates } from "@/lib/loadingStates";

const NETWORK_ERROR = "Network error. Please try again.";

export type ScheduleResult =
    | { ok: true; queuedTrack: QueuedTrackModel }
    | { ok: false; error: string };

export interface CancelResult {
    ok: boolean;
    error?: string;
}

interface TrackSchedulingState {
    overview: TrackSchedulingOverviewModel | null;
    overviewState: LoadingStates;
    /** The user the latest overview request was made for; null for a visitor. */
    overviewUserId: string | null | undefined;
}

interface TrackSchedulingActions {
    /** Loads the overview for the given user, skipping a request already running for the same user. */
    fetchOverview: (userId: string | null) => Promise<void>;
    schedule: (cupId: string, trackId: string, quadId: number | null) => Promise<ScheduleResult>;
    cancel: (queuedTrackId: string) => Promise<CancelResult>;
}

type TrackSchedulingStore = TrackSchedulingState & TrackSchedulingActions;

function serverMessage(error: unknown, fallback: string): string {
    return typeof error === "string" && error ? error : fallback;
}

export const useTrackSchedulingStore = create<TrackSchedulingStore>()((set, get) => {
    const refresh = () => get().fetchOverview(get().overviewUserId ?? null);

    return {
        overview: null,
        overviewState: "Idle",
        overviewUserId: undefined,

        fetchOverview: async (userId) => {
            if (get().overviewState === "Loading" && get().overviewUserId === userId) return;

            set({ overviewState: "Loading", overviewUserId: userId });
            try {
                const response = await getApiTrackSchedulingOverview();
                // The user signed in or out while this request was running: a newer one owns the state
                if (get().overviewUserId !== userId) return;

                set(response.data ? { overview: response.data, overviewState: "Loaded" } : { overviewState: "Error" });
            } catch {
                if (get().overviewUserId === userId) set({ overviewState: "Error" });
            }
        },

        schedule: async (cupId, trackId, quadId) => {
            try {
                const response = await postApiTrackSchedulingSchedule({ body: { cupId, trackId, quadId: quadId ?? undefined } });
                if (!response.data) {
                    return { ok: false, error: serverMessage(response.error, "Could not schedule the track. Please try again.") };
                }

                await refresh();
                return { ok: true, queuedTrack: response.data };
            } catch {
                return { ok: false, error: NETWORK_ERROR };
            }
        },

        cancel: async (queuedTrackId) => {
            try {
                const response = await postApiTrackSchedulingCancel({ body: { id: queuedTrackId } });
                if (!response.response.ok) {
                    return { ok: false, error: serverMessage(response.error, "Could not cancel the track. Please try again.") };
                }

                await refresh();
                return { ok: true };
            } catch {
                return { ok: false, error: NETWORK_ERROR };
            }
        },
    };
});
