import { create } from 'zustand';
import { useShallow } from 'zustand/shallow';

export const MAX_SELECTED_PILOTS = 2;

export interface SelectedPilotsState {
  pilots: (string | null)[];
}

export interface SelectedPilotsActions {
  addPilot: () => void;
  selectPilot: (pilotName: string, index: number) => void;
  removePilot: () => void;
  clearPilots: () => void;
}

export type SelectedPilotsStore = SelectedPilotsState & SelectedPilotsActions;

export const useSelectedPilotsStore = create<SelectedPilotsStore>()((set, get) => ({
  pilots: [null],
  addPilot: () => {
    const currentPilots = get().pilots;
    if (currentPilots.length < MAX_SELECTED_PILOTS) {
      set({ pilots: [...currentPilots, null] });
    }
  },
  selectPilot: (pilotName: string, index: number) => {
    const currentPilots = [...get().pilots];
    currentPilots[index] = pilotName;
    set({ pilots: currentPilots });
  },
  removePilot: () => {
    const currentPilots = [...get().pilots];
    currentPilots.pop();
    set({ pilots: currentPilots });
  },
  clearPilots: () => {
    set({ pilots: [null] });
  },
}));

// Derived selectors
export const useIsMaxPilotsReached = () => 
  useSelectedPilotsStore(
    useShallow((state) => state.pilots.length >= MAX_SELECTED_PILOTS)
  );