import { create } from 'zustand';
import api from '../api/api';
import { DashboardModel } from '../api/client';
import { LoadingStates } from '../lib/loadingStates';

export interface DashboardState {
  state: LoadingStates;
  data: DashboardModel | null;
}

export interface DashboardActions {
  fetch: () => Promise<void>;
}

export type DashboardStore = DashboardState & DashboardActions;

export const useDashboardStore = create<DashboardStore>()((set, get) => ({
  state: 'Idle',
  data: null,
  fetch: async () => {
    if (get().state === 'Loading') return;
    
    set({ state: 'Loading' });
    try {
      const response = await api.getDashboard();
      set({ 
        state: 'Loaded',
        data: response.data 
      });
    } catch (error) {
      set({ 
        state: 'Error',
        data: null 
      });
    }
  },
}));