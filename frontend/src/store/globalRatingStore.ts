import { create } from "zustand";
import { client, LeagueSettingsModel } from "../api/client";
import { LoadingStates } from "../lib/loadingStates";

export type { LeagueSettingsModel, LeagueDescriptorModel } from "../api/client";

export interface PilotRatingModel {
    pilotId: number;
    pilotName: string;
    country: string | null;
    averageGapPercent: number | null;
    averageGapChange: number | null;
    rank: number;
    rankChange: number | null;
    league: string | null;
}

export interface GlobalRatingData {
    calculatedOn: string;
    ratings: PilotRatingModel[];
    droppedOutPilots: PilotRatingModel[];
    leagueSettings: LeagueSettingsModel;
}

interface GlobalRatingState {
    /** Cup the currently held data belongs to. */
    cupId: string | null;
    data: GlobalRatingData | null;
    loadingState: LoadingStates;
}

interface GlobalRatingActions {
    fetchRatings: (cupId: string) => Promise<void>;
}

type GlobalRatingStore = GlobalRatingState & GlobalRatingActions;

export const useGlobalRatingStore = create<GlobalRatingStore>()((set, get) => ({
    cupId: null,
    data: null,
    loadingState: "Idle",

    fetchRatings: async (cupId: string) => {
        const { cupId: currentCupId, loadingState } = get();
        const alreadyRequested = loadingState === "Loading" || loadingState === "Loaded";
        if (cupId === currentCupId && alreadyRequested) return;

        set({ cupId, data: null, loadingState: "Loading" });

        try {
            const result = await client.get<GlobalRatingData, unknown, false>({
                url: "/api/ratings/get",
                query: { cupId },
            });

            if (!result.data) throw new Error("No data");

            // A quick cup switch can leave an earlier request in flight; its
            // response must not overwrite the cup the user is now looking at.
            if (get().cupId !== cupId) return;

            set({ data: result.data, loadingState: "Loaded" });
        } catch {
            if (get().cupId === cupId) set({ loadingState: "Error" });
        }
    },
}));
