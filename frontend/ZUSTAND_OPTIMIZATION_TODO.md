# Zustand Optimization TODO

This document contains additional optimization suggestions for the Zustand implementation, organized by priority.

## High Priority (Performance Impact)

### 1. Optimize Complex Selector in `pilotsStore.ts`
**File**: `src/store/pilotsStore.ts:114-127`
**Issue**: `usePilotResultsLoadingState` performs expensive computation on every render.

```typescript
// Current - expensive computation in selector
export const usePilotResultsLoadingState = (pilots: (string | null)[]): LoadingStates => {
  return usePilotsStore(
    useShallow((state) => {
      const states = pilots
        .filter((p) => p != null)
        .map((p) => state.selectPilotResultLoadingState[p!]);

      if (states.length === 0) return 'Idle';
      if (states.find((s) => s === 'Loading')) return 'Loading';
      return 'Loaded';
    })
  );
};

// Optimized - extract computation with useMemo
import { useMemo } from 'react';

export const usePilotResultsLoadingState = (pilots: (string | null)[]): LoadingStates => {
  const states = usePilotsStore(
    useShallow((state) => pilots
      .filter(p => p != null)
      .map(p => state.selectPilotResultLoadingState[p!])
    )
  );
  
  return useMemo(() => {
    if (states.length === 0) return 'Idle';
    return states.find(s => s === 'Loading') ? 'Loading' : 'Loaded';
  }, [states]);
};
```

### 2. Improve Error Handling in Stores
**Files**: All store files
**Issue**: Errors reset data to empty states, causing UI flickers.

#### Add error tracking to store interfaces:
```typescript
// pilotsStore.ts
export interface PilotsState {
  state: LoadingStates;
  pilots: string[];
  pilotResults: Record<string, PilotResult[]>;
  selectPilotResultLoadingState: Record<string, LoadingStates>;
  error: string | null; // Add this
  pilotErrors: Record<string, string | null>; // Add this for individual pilot errors
}

// In error handling - preserve existing data:
catch (error) {
  set({ 
    state: 'Error',
    error: error instanceof Error ? error.message : 'Unknown error'
    // Don't reset pilots array or pilotResults
  });
}
```

#### Similar changes needed in:
- `dashboardStore.ts`
- `heatmapStore.ts` (if you add async operations)

### 3. Add Loading State Management to `heatmapStore.ts`
**File**: `src/store/heatmapStore.ts`
**Issue**: Has unused `state` property, inconsistent with other stores.

```typescript
// Either remove unused state or implement proper loading:
export const useHeatmapStore = create<HeatmapStore>()((set) => ({
  state: 'Idle', // Remove this if not using
  currentPilot: null,
  choosePilot: (pilotName: string) => {
    set({ 
      currentPilot: pilotName,
      state: 'Loading' // If keeping state, update it
    });
  },
}));
```

## Medium Priority (Code Quality)

### 4. Add Zustand DevTools Integration
**Files**: All store files
**Benefit**: Better debugging experience during development.

```typescript
import { devtools } from 'zustand/middleware';

export const usePilotsStore = create<PilotsStore>()(
  devtools(
    (set, get) => ({
      // ... existing implementation
    }),
    { name: 'pilots-store' }
  )
);

// Apply to all stores:
// - dashboardStore: { name: 'dashboard-store' }
// - selectedPilotsStore: { name: 'selected-pilots-store' }
// - heatmapStore: { name: 'heatmap-store' }
```

### 5. Create Semantic Hooks for Complex Operations
**File**: `src/store/pilotsStore.ts`
**Benefit**: Hide complexity, provide better API for components.

```typescript
// Add these hooks to pilotsStore.ts:

export const usePilotData = (pilotName: string | null) => {
  const store = usePilotsStore();
  
  useEffect(() => {
    if (pilotName && store.pilots.includes(pilotName)) {
      store.fetchPilotResults(pilotName);
    }
  }, [pilotName, store.pilots, store.fetchPilotResults]);
  
  return usePilotResults(pilotName);
};

export const useAutoFetchPilots = () => {
  const { state, fetchPilots } = usePilotsStore(
    useShallow((state) => ({
      state: state.state,
      fetchPilots: state.fetchPilots
    }))
  );
  
  useEffect(() => {
    if (state === 'Idle' || state === 'Error') {
      fetchPilots();
    }
  }, [state, fetchPilots]);
  
  return state;
};
```

### 6. Add Store Persistence for User Selections
**File**: `src/store/selectedPilotsStore.ts`
**Benefit**: Remember user's pilot selections across browser sessions.

```typescript
import { persist } from 'zustand/middleware';

export const useSelectedPilotsStore = create<SelectedPilotsStore>()(
  devtools(
    persist(
      (set, get) => ({
        // ... existing implementation
      }),
      { 
        name: 'selected-pilots',
        // Only persist the pilots selection, not actions
        partialize: (state) => ({ pilots: state.pilots })
      }
    ),
    { name: 'selected-pilots-store' }
  )
);
```

### 7. Standardize Async Operation Patterns
**Files**: All stores with async operations
**Benefit**: Consistent error handling and loading states.

```typescript
// Create a generic async handler utility:
// src/utils/asyncStoreHandler.ts
export const createAsyncAction = <T>(
  actionName: string,
  asyncFn: () => Promise<T>
) => {
  return async (set: (partial: any) => void, get: () => any) => {
    const currentState = get();
    
    // Prevent duplicate requests
    if (currentState.state === 'Loading') return;
    
    set({ state: 'Loading', error: null });
    
    try {
      const result = await asyncFn();
      set({ 
        state: 'Loaded',
        error: null,
        // ... set result data
      });
      return result;
    } catch (error) {
      set({ 
        state: 'Error',
        error: error instanceof Error ? error.message : `${actionName} failed`
      });
      throw error;
    }
  };
};
```

## Low Priority (Architectural Improvements)

### 8. Consider Store Splitting for Better Performance
**File**: `src/store/pilotsStore.ts`
**Issue**: Large monolithic store where parts could be separated.

```typescript
// Split into:
// src/store/pilotsListStore.ts - handles pilot list only
export const usePilotsListStore = create<PilotsListStore>()(
  devtools((set, get) => ({
    state: 'Idle',
    pilots: [],
    error: null,
    fetchPilots: async () => { /* ... */ }
  }), { name: 'pilots-list-store' })
);

// src/store/pilotResultsStore.ts - handles individual pilot results
export const usePilotResultsStore = create<PilotResultsStore>()(
  devtools((set, get) => ({
    pilotResults: {},
    loadingStates: {},
    errors: {},
    fetchPilotResults: async (pilotName: string) => { /* ... */ }
  }), { name: 'pilot-results-store' })
);

// src/store/pilotsComposed.ts - combines them when needed
export const usePilotsComposed = () => {
  const pilotsList = usePilotsListStore();
  const pilotResults = usePilotResultsStore();
  
  return {
    ...pilotsList,
    ...pilotResults
  };
};
```

### 9. Improve Store Structure for Performance
**File**: `src/store/pilotsStore.ts`
**Benefit**: Reduce unnecessary re-renders when individual pilot data changes.

```typescript
// Current flat structure causes broader re-renders
interface PilotsState {
  pilots: string[];
  pilotResults: Record<string, PilotResult[]>;
  selectPilotResultLoadingState: Record<string, LoadingStates>;
}

// Better nested structure
interface PilotsState {
  pilots: {
    list: string[];
    loadingState: LoadingStates;
    error?: string;
  };
  pilotResults: Record<string, {
    data: PilotResult[];
    loadingState: LoadingStates;
    error?: string;
    lastFetched?: Date;
  }>;
}
```

## Testing & Monitoring

### 10. Add Performance Monitoring
Create hooks to measure performance impact:

```typescript
// src/hooks/usePerformanceMonitoring.ts
export const useRenderCount = (componentName: string) => {
  const renderCount = useRef(0);
  renderCount.current++;
  
  useEffect(() => {
    console.log(`${componentName} rendered ${renderCount.current} times`);
  });
};

// Use in components during optimization:
const PagePilots = () => {
  useRenderCount('PagePilots');
  // ... rest of component
};
```

### 11. Bundle Size Analysis
Monitor the impact of additional Zustand middleware:

```bash
# Add to package.json scripts:
"analyze": "npx vite-bundle-analyzer dist"

# Run after implementing changes:
npm run build
npm run analyze
```

## Implementation Notes

- **Order of Implementation**: Follow the priority order for maximum impact
- **Testing**: Test each change individually with `npm run build`
- **Browser DevTools**: Use React DevTools Profiler to measure re-render improvements
- **Performance Baseline**: Measure current performance before implementing changes
- **Gradual Migration**: Don't implement all changes at once - do them incrementally

## Success Metrics

- **Reduced Re-renders**: Measure with React DevTools Profiler
- **Faster Load Times**: Monitor initial page load and data fetching
- **Better Error Recovery**: Test error scenarios and recovery
- **Bundle Size**: Keep middleware additions minimal
- **Developer Experience**: Improved debugging with DevTools integration

---
*Generated on: 2025-08-11*
*For: VelocidroneBot Frontend Zustand Optimization*