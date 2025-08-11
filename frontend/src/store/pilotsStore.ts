import { create } from 'zustand';
import { useShallow } from 'zustand/shallow';
import {
  getApiPilotsAll,
  getApiResultsForPilot,
  PilotResult,
} from '../api/client';
import { LoadingStates } from '../lib/loadingStates';

export interface PilotsState {
  state: LoadingStates;
  pilots: string[];
  pilotResults: Record<string, PilotResult[]>;
  selectPilotResultLoadingState: Record<string, LoadingStates>;
}

export interface PilotsActions {
  fetchPilots: () => Promise<void>;
  fetchPilotResults: (pilotName: string) => Promise<void>;
}

export type PilotsStore = PilotsState & PilotsActions;

export const usePilotsStore = create<PilotsStore>()((set, get) => ({
  state: 'Idle',
  pilots: [],
  pilotResults: {},
  selectPilotResultLoadingState: {},

  fetchPilots: async () => {
    if (get().state === 'Loading') return;
    
    set({ state: 'Loading' });

    try {
      const result = await getApiPilotsAll();
      set({ 
        state: 'Loaded',
        pilots: result.data || []
      });
    } catch (error) {
      set({ 
        state: 'Error',
        pilots: []
      });
    }
  },

  fetchPilotResults: async (pilotName: string) => {
    const currentState = get();
    
    // Check if already loaded or loading
    if (currentState.selectPilotResultLoadingState[pilotName] === 'Loaded') {
      return;
    }
    if (currentState.selectPilotResultLoadingState[pilotName] === 'Loading') {
      return;
    }

    set({ 
      selectPilotResultLoadingState: {
        ...currentState.selectPilotResultLoadingState,
        [pilotName]: 'Loading'
      }
    });

    try {
      const result = await getApiResultsForPilot({
        query: { pilotName: pilotName },
      });
      
      set({
        selectPilotResultLoadingState: {
          ...get().selectPilotResultLoadingState,
          [pilotName]: 'Loaded'
        },
        pilotResults: {
          ...get().pilotResults,
          [pilotName]: result.data || []
        }
      });
    } catch (error) {
      set({
        selectPilotResultLoadingState: {
          ...get().selectPilotResultLoadingState,
          [pilotName]: 'Error'
        },
        pilotResults: {
          ...get().pilotResults,
          [pilotName]: []
        }
      });
    }
  },
}));

// Derived selectors
const EMPTY_RESULTS: PilotResult[] = [];

export const usePilotResults = (pilotName: string | null) => {
  return usePilotsStore((state) => pilotName ? state.pilotResults[pilotName] || EMPTY_RESULTS : EMPTY_RESULTS);
};

export const usePilotsResults = (pilots: (string | null)[]) => {
  return usePilotsStore(
    useShallow((state) => pilots.map((p) => (p ? state.pilotResults[p] || EMPTY_RESULTS : EMPTY_RESULTS)))
  );
};

export const usePilotResultLoadingState = (pilotName: string | null) => {
  return usePilotsStore((state) => pilotName ? state.selectPilotResultLoadingState[pilotName] || 'Idle' : 'Idle');
};

export const usePilotResultsLoadingState = (pilots: (string | null)[]): LoadingStates => {
  return usePilotsStore(
    useShallow((state) => {
      const states = pilots
        .filter((p) => p != null)
        .map((p) => state.selectPilotResultLoadingState[p!]);

      if (states.length === 0) return 'Idle';
      if (states.find((s) => s === 'Loading')) return 'Loading';

      return 'Loaded';
    })
  );
};