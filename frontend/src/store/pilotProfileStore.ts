import { create } from "zustand";
import {
  getApiPilotsProfile,
  getApiResultsForPilot,
  PilotProfileModel,
  PilotResult,
} from "../api/client";
import { LoadingStates } from "../lib/loadingStates";

export interface PilotProfileState {
  profiles: Record<string, PilotProfileModel>;
  heatmapData: Record<string, PilotResult[]>;
  profileLoadingState: Record<string, LoadingStates>;
  heatmapLoadingState: Record<string, LoadingStates>;
  currentPilot: string | null;
}

export interface PilotProfileActions {
  fetchPilotProfile: (pilotName: string) => Promise<void>;
  fetchPilotHeatmapData: (pilotName: string) => Promise<void>;
  clearProfile: (pilotName: string) => void;
  choosePilot: (pilotName: string) => void;
}

export type PilotProfileStore = PilotProfileState & PilotProfileActions;

export const usePilotProfileStore = create<PilotProfileStore>()((set, get) => ({
  profiles: {},
  heatmapData: {},
  profileLoadingState: {},
  heatmapLoadingState: {},
  currentPilot: null,

  fetchPilotProfile: async (pilotName: string) => {
    const currentState = get();

    // Check if already loaded or loading
    if (currentState.profileLoadingState[pilotName] === "Loaded") {
      return;
    }
    if (currentState.profileLoadingState[pilotName] === "Loading") {
      return;
    }

    set({
      profileLoadingState: {
        ...currentState.profileLoadingState,
        [pilotName]: "Loading",
      },
    });

    try {
      const result = await getApiPilotsProfile({
        query: { pilotName: pilotName },
      });

      if (result.error || !result.data) {
        throw new Error("Failed to fetch pilot profile");
      }

      set({
        profileLoadingState: {
          ...get().profileLoadingState,
          [pilotName]: "Loaded",
        },
        profiles: {
          ...get().profiles,
          [pilotName]: result.data,
        },
      });
    } catch (error) {
      set({
        profileLoadingState: {
          ...get().profileLoadingState,
          [pilotName]: "Error",
        },
      });
    }
  },

  fetchPilotHeatmapData: async (pilotName: string) => {
    const currentState = get();

    // Check if already loaded or loading
    if (currentState.heatmapLoadingState[pilotName] === "Loaded") {
      return;
    }
    if (currentState.heatmapLoadingState[pilotName] === "Loading") {
      return;
    }

    set({
      heatmapLoadingState: {
        ...currentState.heatmapLoadingState,
        [pilotName]: "Loading",
      },
    });

    try {
      const result = await getApiResultsForPilot({
        query: { pilotName: pilotName },
      });

      set({
        heatmapLoadingState: {
          ...get().heatmapLoadingState,
          [pilotName]: "Loaded",
        },
        heatmapData: {
          ...get().heatmapData,
          [pilotName]: result.data || [],
        },
      });
    } catch (error) {
      set({
        heatmapLoadingState: {
          ...get().heatmapLoadingState,
          [pilotName]: "Error",
        },
        heatmapData: {
          ...get().heatmapData,
          [pilotName]: [],
        },
      });
    }
  },

  clearProfile: (pilotName: string) => {
    const state = get();

    // Remove pilot data from all records
    const newProfiles = { ...state.profiles };
    const newHeatmapData = { ...state.heatmapData };
    const newProfileLoadingState = { ...state.profileLoadingState };
    const newHeatmapLoadingState = { ...state.heatmapLoadingState };

    delete newProfiles[pilotName];
    delete newHeatmapData[pilotName];
    delete newProfileLoadingState[pilotName];
    delete newHeatmapLoadingState[pilotName];

    set({
      profiles: newProfiles,
      heatmapData: newHeatmapData,
      profileLoadingState: newProfileLoadingState,
      heatmapLoadingState: newHeatmapLoadingState,
    });
  },

  choosePilot: (pilotName: string) => {
    const state = get();

    // Set the current pilot
    set({ currentPilot: pilotName });

    // Automatically fetch data for the selected pilot if not already loaded/loading
    if (
      state.profileLoadingState[pilotName] !== "Loaded" &&
      state.profileLoadingState[pilotName] !== "Loading"
    ) {
      state.fetchPilotProfile(pilotName);
    }

    if (
      state.heatmapLoadingState[pilotName] !== "Loaded" &&
      state.heatmapLoadingState[pilotName] !== "Loading"
    ) {
      state.fetchPilotHeatmapData(pilotName);
    }
  },
}));

const empty: PilotResult[] = [];

// Derived selectors
export const usePilotProfile = (pilotName: string | null) => {
  return usePilotProfileStore((state) =>
    pilotName ? state.profiles[pilotName] || null : null
  );
};

export const usePilotHeatmapData = (pilotName: string | null) => {
  return usePilotProfileStore((state) =>
    pilotName ? state.heatmapData[pilotName] || empty : empty
  );
};

export const usePilotProfileLoadingState = (pilotName: string | null) => {
  return usePilotProfileStore((state) =>
    pilotName ? state.profileLoadingState[pilotName] || "Idle" : "Idle"
  );
};

export const usePilotHeatmapLoadingState = (pilotName: string | null) => {
  return usePilotProfileStore((state) =>
    pilotName ? state.heatmapLoadingState[pilotName] || "Idle" : "Idle"
  );
};

// Combined loading state for the entire pilot profile page
export const usePilotProfilePageLoadingState = (pilotName: string | null) => {
  return usePilotProfileStore((state) => {
    if (!pilotName) return "Idle";

    const profileState = state.profileLoadingState[pilotName] || "Idle";
    const heatmapState = state.heatmapLoadingState[pilotName] || "Idle";

    // If either is loading, show loading
    if (profileState === "Loading" || heatmapState === "Loading") {
      return "Loading";
    }

    // If profile failed to load, show error (heatmap is optional)
    if (profileState === "Error") {
      return "Error";
    }

    // If profile is loaded, consider it loaded (heatmap is optional)
    if (profileState === "Loaded") {
      return "Loaded";
    }

    return "Idle";
  });
};
