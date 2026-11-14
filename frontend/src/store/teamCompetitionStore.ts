import { create, type UseBoundStore, type StoreApi } from 'zustand';
import api from '../api/api';
import { TeamOverviewModel } from '../api/client';
import { LoadingStates } from '../lib/loadingStates';

export interface TeamCompetitionState {
  state: LoadingStates;
  data: TeamOverviewModel | null;
  /** The day the data is for; null is today. */
  date: string | null;
}

export interface TeamCompetitionActions {
  /** Loads the given day unless it is already loaded or loading. */
  load: (date: string | null) => Promise<void>;
  refresh: () => Promise<void>;
}

export type TeamCompetitionStore = TeamCompetitionState & TeamCompetitionActions;

function createTeamCompetitionStore(cupId: string): UseBoundStore<StoreApi<TeamCompetitionStore>> {
  return create<TeamCompetitionStore>()((set, get) => ({
    state: 'Idle',
    data: null,
    date: null,
    load: async (date: string | null) => {
      const { state, date: current } = get();
      if (current === date && (state === 'Loading' || state === 'Loaded')) return;

      set({ state: 'Loading', date });
      try {
        const response = await api.getTeamCompetitionOverview(cupId, date ?? undefined);
        // Another day may have been picked while this one was loading
        if (get().date === date) set({ state: 'Loaded', data: response.data });
      } catch {
        if (get().date === date) set({ state: 'Error', data: null });
      }
    },
    refresh: async () => {
      const { state, date } = get();
      if (state !== 'Loaded') return;

      try {
        const response = await api.getTeamCompetitionOverview(cupId, date ?? undefined);
        if (get().date === date) set({ data: response.data });
      } catch {
        // Keep existing data on refresh failure — only initial load shows error state
      }
    },
  }));
}

const storeCache = new Map<string, UseBoundStore<StoreApi<TeamCompetitionStore>>>();

export function getTeamCompetitionStore(cupId: string): UseBoundStore<StoreApi<TeamCompetitionStore>> {
  if (!storeCache.has(cupId)) {
    storeCache.set(cupId, createTeamCompetitionStore(cupId));
  }
  return storeCache.get(cupId)!;
}
