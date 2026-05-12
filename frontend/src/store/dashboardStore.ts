import { create, type UseBoundStore, type StoreApi } from 'zustand';
import api from '../api/api';
import { DashboardModel } from '../api/client';
import { LoadingStates } from '../lib/loadingStates';

export interface DashboardState {
  state: LoadingStates;
  data: DashboardModel | null;
  selectedDate: string | null;
}

export interface DashboardActions {
  fetch: () => Promise<void>;
  refresh: () => Promise<void>;
  selectDate: (date: string | null) => void;
}

export type DashboardStore = DashboardState & DashboardActions;

function createDashboardStore(cupId: string): UseBoundStore<StoreApi<DashboardStore>> {
  return create<DashboardStore>()((set, get) => ({
    state: 'Idle',
    data: null,
    selectedDate: null,
    fetch: async () => {
      if (get().state === 'Loading') return;

      set({ state: 'Loading' });
      try {
        const response = await api.getDashboard(cupId, get().selectedDate ?? undefined);
        set({ state: 'Loaded', data: response.data });
      } catch {
        set({ state: 'Error', data: null });
      }
    },
    refresh: async () => {
      if (get().state === 'Loading') return;

      try {
        const response = await api.getDashboard(cupId, get().selectedDate ?? undefined);
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

const storeCache = new Map<string, UseBoundStore<StoreApi<DashboardStore>>>();

export function getDashboardStore(cupId: string): UseBoundStore<StoreApi<DashboardStore>> {
  if (!storeCache.has(cupId)) {
    storeCache.set(cupId, createDashboardStore(cupId));
  }
  return storeCache.get(cupId)!;
}
