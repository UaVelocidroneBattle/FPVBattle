import { create } from "zustand";
import { client } from "../api/client";
import { LoadingStates } from "../lib/loadingStates";

export interface PilotRatingModel {
    pilotId: number;
    pilotName: string;
    country: string | null;
    averageGapPercent: number | null;
    averageGapChange: number | null;
    rank: number;
    rankChange: number | null;
}

export interface GlobalRatingData {
    calculatedOn: string;
    ratings: PilotRatingModel[];
}

interface GlobalRatingState {
    data: GlobalRatingData | null;
    loadingState: LoadingStates;
}

interface GlobalRatingActions {
    fetchRatings: (cupId: string) => Promise<void>;
}

type GlobalRatingStore = GlobalRatingState & GlobalRatingActions;

export const useGlobalRatingStore = create<GlobalRatingStore>()((set, get) => ({
    data: null,
    loadingState: "Idle",

    fetchRatings: async (cupId: string) => {
        const { loadingState } = get();
        if (loadingState === "Loading" || loadingState === "Loaded") return;

        set({ loadingState: "Loading" });

        try {
            const result = await client.get<GlobalRatingData, unknown, false>({
                url: "/api/ratings/get",
                query: { cupId },
            });

            if (!result.data) throw new Error("No data");

            set({ data: result.data, loadingState: "Loaded" });
        } catch {
            set({ loadingState: "Error" });
        }
    },
}));
