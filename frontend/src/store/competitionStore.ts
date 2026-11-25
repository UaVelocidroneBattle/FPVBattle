import { create, type UseBoundStore, type StoreApi } from 'zustand';
import api from '../api/api';
import { CompetitionOverviewModel, postApiCompetitionsRestartTrack } from '../api/client';
import { LoadingStates } from '../lib/loadingStates';

// The new track is picked in the background, so the board is checked until it shows up
const NEW_TRACK_CHECKS = 6;
const NEW_TRACK_CHECK_INTERVAL_MS = 5_000;

const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

export interface CompetitionState {
  state: LoadingStates;
  data: CompetitionOverviewModel | null;
  selectedDate: string | null;
}

export interface CompetitionActions {
  fetch: () => Promise<void>;
  refresh: () => Promise<void>;
  /** Resolves to an error message, or null once the new track is on the board (or the checks ran out). */
  restartTrack: () => Promise<string | null>;
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
    restartTrack: async () => {
      const restartedCompetitionId = get().data?.competition?.id;

      try {
        const response = await postApiCompetitionsRestartTrack({ query: { cupId } });
        if (!response.response.ok) {
          return typeof response.error === 'string' && response.error
            ? response.error
            : 'Could not restart the track. Please try again.';
        }
      } catch {
        return 'Network error. Please try again.';
      }

      for (let check = 0; check < NEW_TRACK_CHECKS; check++) {
        await wait(NEW_TRACK_CHECK_INTERVAL_MS);
        await get().refresh();

        const competition = get().data?.competition;
        if (competition && competition.id !== restartedCompetitionId) break;
      }

      return null;
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
