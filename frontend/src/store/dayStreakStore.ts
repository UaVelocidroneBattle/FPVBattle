import { create } from "zustand";
import { client } from "../api/client";
import { LoadingStates } from "../lib/loadingStates";

export interface DayStreakLeaderboardRow {
    pilotName: string;
    dayStreak: number;
    maxStreak: number;
}

interface DayStreakState {
    rows: DayStreakLeaderboardRow[];
    loadingState: LoadingStates;
}

interface DayStreakActions {
    fetchLeaderboard: () => Promise<void>;
}

type DayStreakStore = DayStreakState & DayStreakActions;

export const useDayStreakStore = create<DayStreakStore>()((set, get) => ({
    rows: [],
    loadingState: "Idle",

    fetchLeaderboard: async () => {
        const { loadingState } = get();
        if (loadingState === "Loading" || loadingState === "Loaded") return;

        set({ loadingState: "Loading" });

        try {
            const result = await client.get<DayStreakLeaderboardRow[], unknown, false>({
                url: "/api/daystreak/Leaderboard",
            });

            if (!result.data) throw new Error("No data");

            set({ rows: result.data, loadingState: "Loaded" });
        } catch {
            set({ loadingState: "Error" });
        }
    },
}));
