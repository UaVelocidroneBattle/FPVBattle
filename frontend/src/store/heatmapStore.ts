import { create } from 'zustand';

export interface HeatmapState {
  state: 'Idle' | 'Loading' | 'Loaded' | 'Error';
  currentPilot: string | null;
}

export interface HeatmapActions {
  choosePilot: (pilotName: string) => void;
}

export type HeatmapStore = HeatmapState & HeatmapActions;

export const useHeatmapStore = create<HeatmapStore>()((set) => ({
  state: 'Idle',
  currentPilot: null,
  choosePilot: (pilotName: string) => {
    set({ currentPilot: pilotName });
  },
}));