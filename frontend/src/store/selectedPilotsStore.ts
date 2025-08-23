import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import { usePilotsStore } from './pilotsStore';

export const MAX_SELECTED_PILOTS = 2;

export interface SelectedPilotsState {
  pilots: (string | null)[];
}

export interface SelectedPilotsActions {
  addPilot: () => void;
  selectPilot: (pilotName: string, index: number) => void;
  setPilots: (pilots: (string | null)[]) => void;
  removePilot: () => void;
  clearPilots: () => void;
}

export type SelectedPilotsStore = SelectedPilotsState & SelectedPilotsActions;

export const useSelectedPilotsStore = create<SelectedPilotsStore>()(
  devtools(
    (set, get) => ({
      pilots: [null],
      addPilot: () => {
        const currentPilots = get().pilots;
        if (currentPilots.length < MAX_SELECTED_PILOTS) {
          set({ pilots: [...currentPilots, null] }, false, 'addPilot');
        }
      },
      selectPilot: (pilotName: string, index: number) => {
        const currentPilots = [...get().pilots];
        currentPilots[index] = pilotName;
        set({ pilots: currentPilots }, false, `selectPilot/${index}/${pilotName}`);
        
        // Smart data fetching - automatically fetch when pilot is selected
        usePilotsStore.getState().fetchPilotResults(pilotName);
      },
      setPilots: (pilots: (string | null)[]) => {
        set({ pilots }, false, 'setPilots');
        
        // Fetch data for all valid pilots
        pilots.forEach(pilot => {
          if (pilot) {
            usePilotsStore.getState().fetchPilotResults(pilot);
          }
        });
      },
      removePilot: () => {
        const currentPilots = [...get().pilots];
        currentPilots.pop();
        set({ pilots: currentPilots }, false, 'removePilot');
      },
      clearPilots: () => {
        set({ pilots: [null] }, false, 'clearPilots');
      },
    }),
    { name: 'selectedPilots' }
  )
);

// Derived selectors
export const useIsMaxPilotsReached = () => 
  useSelectedPilotsStore((state) => state.pilots.length >= MAX_SELECTED_PILOTS);