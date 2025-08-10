import { createAppSlice } from "@/lib/createAppSlice";
import { PayloadAction } from "@reduxjs/toolkit";

export const MAX_SELECTED_PILOTS = 2;

interface SelectedPilotsState {
  pilots: (string | null)[];
}

const initialState: SelectedPilotsState = {
  pilots: [null],
};

interface AddPilotActionData {
  pilotName: string;
  index: number;
}

export const selectedPilotsSlice = createAppSlice({
  name: "selectedPilots",
  initialState,
  reducers: {
    addPilot: (state) => {
      if (state.pilots.length < MAX_SELECTED_PILOTS) {
        state.pilots.push(null);
      }
    },
    selectPilot: (state, action: PayloadAction<AddPilotActionData>) => {
      state.pilots[action.payload.index] = action.payload.pilotName;
    },
    removePilot: (state) => {
      state.pilots.pop();
    },
    clearPilots: (state) => {
      state.pilots = [null];
    },
  },
  selectors: {
    selectSelectedPilots: (state) => state.pilots,
    selectIsMaxPilotsReached: (state) =>
      state.pilots.length >= MAX_SELECTED_PILOTS,
  },
});

export const { addPilot, removePilot, clearPilots, selectPilot } =
  selectedPilotsSlice.actions;

export const { selectSelectedPilots, selectIsMaxPilotsReached } =
  selectedPilotsSlice.selectors;

export default selectedPilotsSlice.reducer;
