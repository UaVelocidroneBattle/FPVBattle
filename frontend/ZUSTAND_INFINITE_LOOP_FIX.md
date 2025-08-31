# Zustand Infinite Loop Fix - Maximum Update Depth Exceeded

## Issue Summary

The application was experiencing "Maximum update depth exceeded" errors after migrating from Redux Toolkit (RTK) to Zustand. This is a React error that occurs when components repeatedly trigger re-renders, creating an infinite loop.

## Root Cause Analysis

The issue was caused by a **breaking change in Zustand v5** regarding selector behavior. In v4, Zustand automatically handled reference stability for selectors that returned new objects/arrays. In v5, this behavior changed to match React's default behavior, causing infinite loops when selectors return new references on every call.

### What Triggered the Infinite Loop

1. **Selector Functions Returning New References**: Several selectors in our stores were returning new arrays or computed values on every call:
   ```typescript
   // ❌ PROBLEMATIC - Creates new array every time
   export const usePilotsResults = (pilots: (string | null)[]) => {
     return usePilotsStore((state) => pilots.map((p) => (p ? state.pilotResults[p] || [] : [])));
   };
   
   // ❌ PROBLEMATIC - Creates new empty array every time
   export const usePilotResults = (pilotName: string | null) => {
     return usePilotsStore((state) => pilotName ? state.pilotResults[pilotName] || [] : []);
   };
   
   // ❌ PROBLEMATIC - Computed boolean value
   export const useIsMaxPilotsReached = () => 
     useSelectedPilotsStore((state) => state.pilots.length >= MAX_SELECTED_PILOTS);
   ```

2. **The Infinite Loop Cycle**:
   - Component renders and calls selector
   - Selector returns new reference (array/object)
   - React detects new reference, triggers re-render
   - Component re-renders, calls selector again
   - Process repeats infinitely

## The Fix

### 1. Import `useShallow` from Zustand

```typescript
import { useShallow } from 'zustand/shallow';
```

### 2. Wrap Selectors with `useShallow`

The `useShallow` hook performs shallow comparison to prevent re-renders when the actual data hasn't changed:

```typescript
// ✅ FIXED - Using useShallow for array selector
export const usePilotsResults = (pilots: (string | null)[]) => {
  return usePilotsStore(
    useShallow((state) => pilots.map((p) => (p ? state.pilotResults[p] || EMPTY_RESULTS : EMPTY_RESULTS)))
  );
};

// ✅ FIXED - Using stable reference for empty results
const EMPTY_RESULTS: PilotResult[] = [];
export const usePilotResults = (pilotName: string | null) => {
  return usePilotsStore((state) => pilotName ? state.pilotResults[pilotName] || EMPTY_RESULTS : EMPTY_RESULTS);
};

// ✅ FIXED - Using useShallow for computed value
export const useIsMaxPilotsReached = () => 
  useSelectedPilotsStore(
    useShallow((state) => state.pilots.length >= MAX_SELECTED_PILOTS)
  );
```

### 3. Stable References for Constants

Created a stable reference for empty arrays to avoid creating new instances:

```typescript
const EMPTY_RESULTS: PilotResult[] = [];
```

## Files Modified

### `src/store/pilotsStore.ts`
- Added `useShallow` import
- Fixed `usePilotsResults` and `usePilotResultsLoadingState` selectors
- Created stable `EMPTY_RESULTS` constant
- Updated `usePilotResults` to use stable reference

### `src/store/selectedPilotsStore.ts`  
- Added `useShallow` import
- Fixed `useIsMaxPilotsReached` selector

## Technical Details

### Why This Happened

Zustand v5 changed its behavior to be more aligned with React's expectations. From the [official migration guide](https://github.com/pmndrs/zustand/blob/main/docs/migrations/migrating-to-v5.md):

> "Zustand v5 has a behavioral change where selectors returning new references can cause infinite loops, similar to React's default behavior."

### How `useShallow` Works

`useShallow` performs a shallow comparison of the selector's output:
- If the shallow comparison indicates no change, the same reference is returned
- This prevents unnecessary re-renders even when new objects/arrays are created
- Similar to React's `useMemo` with shallow dependency comparison

## Prevention Guidelines

### ✅ Safe Selector Patterns
```typescript
// Direct property access (primitives)
const count = useStore((state) => state.count)

// Existing object/array references from state
const items = useStore((state) => state.items)

// Using useShallow for computed values
const total = useStore(useShallow((state) => state.items.reduce((sum, item) => sum + item.value, 0)))
```

### ❌ Patterns to Avoid
```typescript
// Creating new arrays/objects without useShallow
const filteredItems = useStore((state) => state.items.filter(item => item.active))

// Computed values without useShallow  
const hasActiveItems = useStore((state) => state.items.some(item => item.active))

// New objects/arrays in selector
const summary = useStore((state) => ({ total: state.items.length, active: state.items.filter(i => i.active) }))
```

## Documentation References

- [Zustand v5 Migration Guide](https://github.com/pmndrs/zustand/blob/main/docs/migrations/migrating-to-v5.md)
- [useShallow Documentation](https://github.com/pmndrs/zustand/blob/main/docs/apis/shallow.md)
- [Zustand Best Practices](https://github.com/pmndrs/zustand#readme)

## Testing

After applying the fixes:
- ✅ Build completes successfully (`npm run build`)
- ✅ No TypeScript errors
- ✅ No infinite loop errors in browser console
- ✅ Application renders normally

## Key Takeaways

1. **Always use `useShallow` for selectors that return new objects/arrays**
2. **Create stable references for constants (like empty arrays)**
3. **Test thoroughly after Zustand version upgrades**
4. **Monitor browser console for "Maximum update depth exceeded" errors**
5. **Consider the performance implications of selector complexity**

This fix ensures that the Zustand migration is complete and the application runs without infinite re-render loops.