## Frontend

- @frontend/README.md

## Development Guidelines for Claude Code

### Educational Approach
- **ALWAYS teach React/Zustand best practices** while implementing features
- **Explain the "why" behind implementation choices** using educational insights
- **Use context7 to research current best practices** before writing any React/Zustand code
- **Provide clear examples** of correct vs incorrect patterns
- **Focus on idiomatic, maintainable solutions** over quick fixes

### Research Requirements
- **Check context7 for React/Zustand patterns** before implementing state management
- **Verify current best practices** for hooks, effects, and store patterns
- **Research component organization patterns** for complex UIs
- **Look up TypeScript patterns** for React components and Zustand stores

### Code Quality Standards
- **Write idiomatic React/Zustand code** that follows community standards
- **Separate concerns properly**: Components render, stores manage state, hooks coordinate
- **Eliminate anti-patterns**: No setTimeout hacks, no manual state synchronization
- **Follow single responsibility principle** for all React components and custom hooks

### React Component Declaration Standards
- **NEVER use `React.FC`** - it's deprecated in modern React/TypeScript development
- **Use regular function declarations** with direct prop typing for better TypeScript inference
- **Avoid implicit children prop** that React.FC adds unnecessarily

```typescript
// ✅ CORRECT: Modern React component declaration
interface ComponentProps {
    name: string;
    className?: string;
}

function MyComponent({ name, className = "" }: ComponentProps) {
    return <div className={className}>{name}</div>;
}

// ❌ WRONG: Legacy React.FC approach
const MyComponent: React.FC<ComponentProps> = ({ name, className = "" }) => {
    return <div className={className}>{name}</div>;
};
```

**Why avoid React.FC:**
- Adds implicit `children?: ReactNode` prop even when not needed
- Interferes with TypeScript inference and generic components
- No longer recommended by React team or used in official docs
- Creates unnecessary verbosity without benefits

## Component Organization Rules

### UI Components Structure
- **Generic UI components** (buttons, inputs, dialogs, etc.) go in `src/components/ui/`
- **Reusable business components** (LeaderBoard, CurrentCompetition, etc.) go in `src/components/`
- **Page-specific components** stay within their respective page folders in `src/pages/`

### When to Move Components
Move components from page-specific folders to shared locations when:
- The component is generic and could be reused elsewhere
- The component implements common UI patterns (navigation items, form controls, etc.)
- The component has no page-specific business logic

### Import Path Updates
When moving components, always update all import statements to reflect the new location.

### Page File Naming Convention
- **Never use `index.tsx` for page components** - it makes VS Code search, navigation, and tab management confusing
- **Use descriptive filenames**: `[Feature]Page.tsx` (e.g., `DashboardPage.tsx`, `PilotsPage.tsx`)
- **Layout files**: Use `[Name]Layout.tsx` pattern (e.g., `MainLayout.tsx`)
- **Organize pages by feature/route hierarchy** in folder structure while keeping descriptive names

### Windows Development Environment Notes
- **Primary dev environment is Windows** - be aware of case-insensitive filesystem behavior
- **NEVER change case of files already committed to Git** - Windows filesystem case-insensitivity causes conflicts between old/new paths
- **For existing files**: Only rename content (component names, exports) - keep file paths unchanged
- **For new files**: Use proper casing from the start to avoid future issues
- **TypeScript is case-sensitive** even on Windows, which can cause build errors with duplicate file references

### Build Testing Requirements
- **ALWAYS run `npm run build` after completing frontend changes** to verify no TypeScript or build errors
- Fix any build errors immediately before considering the task complete
- This ensures code quality and prevents breaking changes from reaching production

### API Client Generation
- **Generate TypeScript client**: Run `npm run gen` to update the generated client from OpenAPI specification
- **Client location**: Generated client is in `src/api/client/`
- **When to regenerate**: After backend API changes, before implementing new features that use new endpoints
- **Type safety**: Generated types ensure compile-time safety between frontend and backend

## Zustand State Management Best Practices

### Store Structure
```typescript
// ✅ CORRECT: Separate state and actions interfaces
interface MyState { 
  data: string; 
  loading: boolean; 
}

interface MyActions { 
  fetchData: (id: string) => Promise<void>; 
}

type MyStore = MyState & MyActions;

const useMyStore = create<MyStore>()((set, get) => ({
  data: '',
  loading: false,
  fetchData: async (id: string) => { /* implementation */ }
}));
```

### Selector Usage Rules
```typescript
// ✅ CORRECT: Use useShallow only for objects/arrays
const { action1, action2 } = useStore(useShallow((state) => ({ 
  action1: state.action1, 
  action2: state.action2 
})));

// ✅ CORRECT: No useShallow needed for primitives
const loadingState = useStore((state) => state.loading); // Returns string/number/boolean

// ❌ WRONG: useShallow with primitives causes infinite loops
const loading = useStore(useShallow((state) => state.loading)); // BAD!
```

### useEffect Dependencies with Store Actions
```typescript
// ✅ CORRECT: Access actions directly in useEffect
useEffect(() => {
  const { fetchData } = useStore.getState();
  fetchData(id);
}, [id]); // Only depend on stable values

// ❌ WRONG: Don't put store actions in dependencies (causes infinite loops)
const { fetchData } = useStore(useShallow(...));
useEffect(() => {
  fetchData(id);
}, [id, fetchData]); // fetchData is recreated every render!
```

### Complex Computations
```typescript
// ❌ WRONG: Complex computed values in selectors
const complexValue = useStore(useShallow((state) => ({
  computed: state.items.map(item => ({ ...item, processed: true }))
}))); // Recreates array every time!

// ✅ CORRECT: Simple selectors with useMemo for complex computations
const items = useStore((state) => state.items);
const processedItems = useMemo(() => 
  items.map(item => ({ ...item, processed: true })), 
[items]);
```

### Critical Rules to Prevent Infinite Loops
1. **useShallow**: Only for objects/arrays, never for primitives
2. **useEffect deps**: Never include store actions, access them via `getState()`
3. **Selectors**: Keep them simple, use `useMemo` for complex computations
4. **Store actions**: Always stable - access directly in effects
5. **TypeScript**: Always separate State and Actions interfaces

## API Integration Patterns

### Generated Client Usage
```typescript
// ✅ CORRECT: Import from generated client
import { client } from '@/api/client';
import type { PilotProfileModel } from '@/api/client/types.gen';

// Use in Zustand store actions
const fetchPilotProfile = async (pilotId: string) => {
  const response = await client.GET('/api/pilots/{id}', {
    params: { path: { id: pilotId } }
  });
  return response.data;
};
```

### API State Management with Zustand
```typescript
interface PilotState {
  pilots: PilotProfileModel[];
  selectedPilot: PilotProfileModel | null;
  loading: boolean;
  error: string | null;
}

interface PilotActions {
  fetchPilots: () => Promise<void>;
  selectPilot: (pilot: PilotProfileModel) => void;
  clearError: () => void;
}

type PilotStore = PilotState & PilotActions;

const usePilotStore = create<PilotStore>()((set, get) => ({
  pilots: [],
  selectedPilot: null,
  loading: false,
  error: null,
  
  fetchPilots: async () => {
    set({ loading: true, error: null });
    try {
      const response = await client.GET('/api/pilots/All');
      if (response.data) {
        set({ pilots: response.data, loading: false });
      }
    } catch (error) {
      set({ error: error.message, loading: false });
    }
  },
  
  selectPilot: (pilot) => set({ selectedPilot: pilot }),
  clearError: () => set({ error: null })
}));
```

### Error Handling Patterns
```typescript
// ✅ CORRECT: Handle API errors gracefully
const fetchData = async () => {
  try {
    const response = await client.GET('/api/endpoint');
    if (response.error) {
      // Handle API-level errors
      set({ error: `API Error: ${response.error}` });
      return;
    }
    set({ data: response.data });
  } catch (error) {
    // Handle network/client errors
    set({ error: `Network Error: ${error.message}` });
  }
};
```

### Loading States
```typescript
// ✅ CORRECT: Manage loading states properly
const useDataStore = create<DataStore>()((set, get) => ({
  fetchData: async () => {
    const { loading } = get();
    if (loading) return; // Prevent duplicate requests
    
    set({ loading: true, error: null });
    try {
      const response = await client.GET('/api/data');
      set({ data: response.data, loading: false });
    } catch (error) {
      set({ error: error.message, loading: false });
    }
  }
}));
```

## Custom Hooks and File Organization

### Hook Placement Rules
Follow these principles when deciding where to place custom hooks:

#### **Page-Specific Hooks (Recommended for Single-Use)**
```typescript
// ✅ CORRECT: Co-locate hooks with their only consumer
frontend/src/pages/statistics/pilots/
├── PilotsPage.tsx
├── useUrlPilotSync.ts ← Page-specific hook
└── ...

// Import: import { useUrlPilotSync } from './useUrlPilotSync';
```

**When to use:**
- Hook is used by only one page/component
- Hook contains page-specific business logic
- Hook is unlikely to be reused elsewhere

#### **Global Hooks**
```typescript
// ✅ CORRECT: Global hooks for cross-cutting concerns
frontend/src/hooks/
├── useAuth.ts
├── useLocalStorage.ts
└── useDebounce.ts

// Import: import { useAuth } from '@/hooks/useAuth';
```

**When to use:**
- Hook is used across multiple pages/components
- Hook provides general utility functionality
- Hook contains no page-specific logic

#### **Feature/Domain Hooks**
```typescript
// ✅ CORRECT: Domain-specific hooks when you have multiple related hooks
frontend/src/features/pilots/hooks/
├── useUrlPilotSync.ts
├── usePilotValidation.ts
└── usePilotFiltering.ts
```

**When to use:**
- Multiple hooks related to the same domain/feature
- Feature-based architecture is adopted
- Hooks are reused within the feature but not globally

### Hook Architecture Patterns

#### **Store-First Pattern (Recommended)**
```typescript
// ✅ CORRECT: Store as single source of truth
export const useUrlPilotSync = () => {
  // 1. Store is authoritative state
  // 2. URL params override store when present (sharing/bookmarking)
  // 3. Store changes automatically update URL
  // 4. Store changes trigger data fetching
  // 5. No URL params = store unchanged (navigation persistence)
  
  // Effect 1: URL → Store (when URL has pilots)
  useEffect(() => {
    if (urlPilots.length > 0) {
      setPilots(finalPilots); // Only update store if URL has data
    }
  }, [searchParams]);

  // Effect 2: Store → URL (for sharing/bookmarking)
  useEffect(() => {
    // Prevent clearing URL on initial render
    if (params.size === 0 && searchParams.size != 0) return;
    setSearchParams(params);
  }, [selectedPilots]);
};
```

#### **Anti-Patterns to Avoid**
```typescript
// ❌ WRONG: Bidirectional sync without coordination
useEffect(() => {
  // Can cause infinite loops
  setSearchParams(storeState);
}, [storeState]);

useEffect(() => {
  // Can conflict with above effect
  setStoreState(urlParams);
}, [urlParams]);

// ❌ WRONG: Manual timing coordination
setTimeout(() => updateUrl(), 0); // Race conditions

// ❌ WRONG: useRef flags for effect coordination
const isUpdating = useRef(false); // Complex and error-prone
```

### File Organization Decision Tree

1. **Is the hook used by only one page?** → Co-locate with page
2. **Is the hook used by multiple pages in same feature?** → Feature hooks folder
3. **Is the hook used across different features?** → Global hooks folder
4. **Is the hook general utility?** → Global hooks folder

### Moving Hooks Guidelines

**When to move from co-located to global:**
- Second page starts using the hook
- Hook becomes useful for other features
- Hook provides general utility value

**When to keep co-located:**
- Hook remains page-specific
- Hook contains page-specific business logic
- No other pages show interest in the functionality

**Example progression:**
```typescript
// Start: Page-specific
pages/pilots/useUrlPilotSync.ts

// Growth: Multiple pilot pages need it
features/pilots/hooks/useUrlPilotSync.ts

// Expansion: Other statistics pages need URL sync
hooks/useUrlSync.ts (generalized)
```