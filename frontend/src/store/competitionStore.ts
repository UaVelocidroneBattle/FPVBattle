import { create, type UseBoundStore, type StoreApi } from 'zustand';
import api from '../api/api';
import { CompetitionOverviewModel } from '../api/client';
import { LoadingStates } from '../lib/loadingStates';

export interface CompetitionState {
  state: LoadingStates;
  data: CompetitionOverviewModel | null;
  selectedDate: string | null;
}

export interface CompetitionActions {
  fetch: () => Promise<void>;
  refresh: () => Promise<void>;
  selectDate: (date: string | null) => void;
}

export type CompetitionStore = CompetitionState & CompetitionActions;

function createCompetitionStore(cupId: string): UseBoundStore<StoreApi<CompetitionStore>> {
  return create<CompetitionStore>()((set, get) => ({
    state: 'Idle',
    data: null,
    selectedDate: null,
    fetch: async () => {
      if (get().state === 'Loading') return;

      set({ state: 'Loading' });
      try {
        const response = await api.getCompetitionOverview(cupId, get().selectedDate ?? undefined);
        set({ state: 'Loaded', data: response.data });
      } catch {
        set({ state: 'Error', data: null });
      }
    },
    refresh: async () => {
      if (get().state === 'Loading') return;

      try {
        const response = await api.getCompetitionOverview(cupId, get().selectedDate ?? undefined);
        set({ state: 'Loaded', data: response.data });
      } catch {
        // Keep existing data on refresh failure — only initial load shows error state
      }
    },
    selectDate: (date: string | null) => {
      set({ selectedDate: date, state: 'Idle' });
    },
  }));
}

const storeCache = new Map<string, UseBoundStore<StoreApi<CompetitionStore>>>();

export function getCompetitionStore(cupId: string): UseBoundStore<StoreApi<CompetitionStore>> {
  if (!storeCache.has(cupId)) {
    storeCache.set(cupId, createCompetitionStore(cupId));
  }
  return storeCache.get(cupId)!;
}
