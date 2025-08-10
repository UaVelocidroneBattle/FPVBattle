# Redux Slice Creation Guide

This guide outlines the steps to create a new Redux slice in this project, following the established conventions.

## 1. Create the Slice File

Create a new file for your slice in the `src/lib/features` directory. The file should be named `[featureName]Slice.ts` (e.g., `userSlice.ts`).

## 2. Define the State Interface

Define an interface for your slice's state. This interface should include all the properties of your slice's state.

```typescript
// src/lib/features/user/userSlice.ts
import { LoadingStates } from "@/lib/loadingStates";

export interface UserSliceState {
  state: LoadingStates;
  userData: User | null;
}
```

## 3. Create the Slice

Use the `createAppSlice` function to create the slice. This function takes an object with the following properties:

* `name`: The name of the slice.
* `initialState`: The initial state of the slice.
* `reducers`: An object containing the slice's reducers.

```typescript
// src/lib/features/user/userSlice.ts
import { createAppSlice } from "../../createAppSlice";
import { LoadingStates } from "@/lib/loadingStates";

// ... (import User model)

export interface UserSliceState {
  state: LoadingStates;
  userData: User | null;
}

const initialState: UserSliceState = {
  state: "Idle",
  userData: null,
};

export const userSlice = createAppSlice({
  name: "user",
  initialState,
  reducers: (create) => ({
    // ... reducers
  }),
  selectors: {
    // ... selectors
  },
});
```

## 4. Define Reducers and Async Thunks

Define your slice's reducers and async thunks within the `reducers` object. Use the `create.asyncThunk` function to create async thunks.

```typescript
// src/lib/features/user/userSlice.ts
// ... (imports)

export const userSlice = createAppSlice({
  name: "user",
  initialState,
  reducers: (create) => ({
    fetchUser: create.asyncThunk(
      async (userId: string) => {
        const response = await api.getUser(userId); // Replace with your API call
        return response.data;
      },
      {
        pending: (state) => {
          state.state = "Loading";
        },
        fulfilled: (state, action) => {
          state.state = "Loaded";
          state.userData = action.payload!;
        },
        rejected: (state) => {
          state.state = "Error";
          state.userData = null;
        },
      }
    ),
  }),
  // ... (selectors)
});
```

## 5. Define Selectors

Define your slice's selectors within the `selectors` object. Selectors are used to retrieve data from the slice's state.

```typescript
// src/lib/features/user/userSlice.ts
// ... (imports and slice definition)

export const userSlice = createAppSlice({
  // ... (name, initialState, reducers)
  selectors: {
    selectUserState: (state) => state.state,
    selectUserData: (state) => state.userData,
  },
});
```

## 6. Export Actions and Selectors

Export the slice's actions and selectors.

```typescript
// src/lib/features/user/userSlice.ts
// ... (slice definition)

export const { fetchUser } = userSlice.actions;

export const { selectUserState, selectUserData } = userSlice.selectors;
```

## 7. Add the Slice to the Root Reducer

Finally, add the new slice to the root reducer in `src/lib/store.ts`.

```typescript
// src/lib/store.ts
import { combineSlices, configureStore } from "@reduxjs/toolkit";
import { dashboardSlice } from "./features/dashboard/dashboardSlice";
import { pilotsSlice } from "./features/pilots/pilotsSlice";
import { heatmapSlice } from "./features/heatmap/heatmapSlice";
import { selectedPilotsSlice } from "./features/selectedPilots/selectedPilotsSlice";
import { userSlice } from "./features/user/userSlice"; // Import the new slice

const rootReducer = combineSlices(
  dashboardSlice,
  pilotsSlice,
  heatmapSlice,
  selectedPilotsSlice,
  userSlice // Add the new slice to the root reducer
);

// ... (rest of the file)
```
