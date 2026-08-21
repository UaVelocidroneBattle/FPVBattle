import { create } from "zustand";
import { CupModel, getApiCupsGet } from "../api/client";
import { LoadingStates } from "../lib/loadingStates";

export type { CupModel } from "../api/client";

interface CupsState {
    /** Enabled cups, in the order the backend configures them. */
    cups: CupModel[];
    loadingState: LoadingStates;
}

interface CupsActions {
    fetchCups: () => Promise<void>;
}

type CupsStore = CupsState & CupsActions;

export const useCupsStore = create<CupsStore>()((set, get) => ({
    cups: [],
    loadingState: "Idle",

    fetchCups: async () => {
        // Cups change only with a backend deploy, so one fetch per session is enough.
        const { loadingState } = get();
        if (loadingState === "Loading" || loadingState === "Loaded") return;

        set({ loadingState: "Loading" });

        try {
            const result = await getApiCupsGet();

            if (!result.data) throw new Error("No data");

            set({ cups: result.data, loadingState: "Loaded" });
        } catch {
            set({ loadingState: "Error" });
        }
    },
}));
