import { create } from 'zustand';
import api from '../api/api';
import { LandingDataModel } from '../api/client';
import { LoadingStates } from '../lib/loadingStates';

export interface LandingState {
  state: LoadingStates;
  data: LandingDataModel | null;
}

export interface LandingActions {
  fetch: () => Promise<void>;
}

export type LandingStore = LandingState & LandingActions;

export const useLandingStore = create<LandingStore>()((set, get) => ({
  state: 'Idle',
  data: null,

  fetch: async () => {
    if (get().state === 'Loading') return;

    set({ state: 'Loading' });
    try {
      const response = await api.getLandingData();
      set({ state: 'Loaded', data: response.data ?? null });
    } catch {
      set({ state: 'Error', data: null });
    }
  },
}));
