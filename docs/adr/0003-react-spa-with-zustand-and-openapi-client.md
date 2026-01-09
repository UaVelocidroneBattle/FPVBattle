# ADR-0003: React SPA with Zustand State Management and OpenAPI Client Generation

## Status
Accepted

## Context
VelocidroneBot needs a web dashboard for displaying:

- **Competition Leaderboards**: Current and historical rankings
- **Pilot Statistics**: Individual pilot performance, achievements, heatmaps
- **Track Information**: Current track, historical track data
- **Achievement System**: Pilot achievements and progress

The frontend requirements are:
- **Fast Loading**: Single Page Application (SPA) for responsive user experience
- **Type Safety**: Strong typing between frontend and backend
- **Maintainability**: Clean, readable code that's easy to modify
- **Simple Deployment**: Static files that can be served from CDN/static hosting
- **Limited Complexity**: No server-side rendering or complex frameworks needed

The backend already provides:
- ASP.NET Core API with OpenAPI specification
- RESTful endpoints for all required data
- Auto-generated API documentation

## Decision
Build the frontend as a **React SPA** with **Zustand** for state management and **generated TypeScript client** from OpenAPI specification.

## Rationale

### React Choice
- **Popular and Mature**: Large ecosystem, extensive documentation, community support
- **Team Familiarity**: Widely known framework, easy to find developers
- **SPA Perfect Fit**: Excellent for single-page applications with client-side routing
- **Component-Based**: Natural fit for dashboard with reusable UI components (leaderboards, charts, cards)
- **Performance**: Fast rendering for data-heavy interfaces like leaderboards and statistics
- **Static Deployment**: Builds to static files for simple hosting

### Simplicity Over Complexity
- **No Next.js/Nuxt**: SSR not needed for dashboard use case
- **No SSG**: Data is dynamic (live competitions, real-time leaderboards)
- **Static File Serving**: Can deploy to Vercel, Netlify, or serve from nginx
- **CDN Friendly**: Static assets can be cached and distributed globally

### Zustand for State Management
- **Simplicity**: Minimal boilerplate compared to Redux
- **Type Safety**: Excellent TypeScript support
- **Clean Code**: Separation of state and actions prevents common React pitfalls
- **Performance**: No unnecessary re-renders with proper selector usage
- **Learning Curve**: Easier to understand and maintain than complex state management solutions

### OpenAPI Client Generation
- **Type Safety**: Compile-time safety between frontend and backend
- **DRY Principle**: Single source of truth for API contracts
- **Development Velocity**: Auto-generated client reduces manual API integration work
- **Consistency**: Ensures frontend stays in sync with backend API changes
- **Error Prevention**: TypeScript catches API contract violations at build time

## Consequences

### Positive
- **Fast Development**: React ecosystem accelerates feature development
- **Type Safety**: Generated client prevents API integration bugs
- **Clean State Management**: Zustand patterns reduce state-related bugs
- **Simple Deployment**: Static files deploy anywhere (Vercel, nginx, CDN)
- **Performance**: SPA provides responsive user experience
- **Maintainability**: Clear separation of concerns with React components and Zustand stores
- **Team Velocity**: Popular technologies reduce onboarding time

### Negative
- **Client-Side Only**: No SEO benefits (not relevant for dashboard use case)
- **Bundle Size**: JavaScript bundle larger than server-rendered pages
- **Initial Load Time**: All code downloaded upfront (mitigated by code splitting)
- **API Dependency**: Frontend requires backend to be functional

### Neutral
- **Learning Curve**: Developers need React and Zustand knowledge
- **Build Process**: Requires build step for deployment (standard for modern frontends)
- **Client Generation**: Additional step when API changes (automated via npm script)

## Alternatives Considered

### Vue.js
- **Pros**: Simpler learning curve, good TypeScript support
- **Cons**: Smaller ecosystem, less team familiarity
- **Verdict**: React's larger ecosystem and team knowledge outweigh Vue's simplicity

### Angular
- **Pros**: Full framework, excellent TypeScript support
- **Cons**: Overkill for dashboard needs, steeper learning curve, heavier bundle
- **Verdict**: Too complex for project requirements

### Svelte/SvelteKit
- **Pros**: Smaller bundle sizes, good performance
- **Cons**: Smaller ecosystem, less team familiarity, newer technology
- **Verdict**: React's maturity and ecosystem more important than bundle size

### Next.js
- **Pros**: Full-stack React framework, excellent DX
- **Cons**: SSR complexity not needed, deployment complexity, overkill for static dashboard
- **Verdict**: Static SPA deployment simpler for our use case

### State Management Alternatives

#### Redux Toolkit
- **Pros**: Mature, predictable, excellent DevTools
- **Cons**: More boilerplate, steeper learning curve
- **Verdict**: Zustand's simplicity better for project scale

#### React Context + useReducer
- **Pros**: Built into React, no additional dependencies
- **Cons**: Performance issues with frequent updates, prop drilling
- **Verdict**: Zustand provides better performance and DX

#### Jotai/Recoil
- **Pros**: Atomic state management, good performance
- **Cons**: Less mature, more complex mental model
- **Verdict**: Zustand's simplicity preferred

### API Client Alternatives

#### Manual Fetch/Axios
- **Pros**: Full control, no code generation
- **Cons**: Manual type definitions, API contract drift, more boilerplate
- **Verdict**: Generated client provides better type safety

#### GraphQL with Apollo
- **Pros**: Flexible data fetching, excellent tooling
- **Cons**: Backend doesn't use GraphQL, adds complexity
- **Verdict**: REST + OpenAPI simpler for current architecture

## Implementation Notes

### Project Structure
```
src/
├── api/                    # Generated API client
│   ├── client/            # Auto-generated from OpenAPI
│   └── api.ts            # Client configuration
├── components/            # Reusable React components
│   └── ui/               # Generic UI components
├── pages/                # Page components and routing
├── store/                # Zustand stores
├── hooks/                # Custom React hooks
└── utils/                # Utility functions
```

### API Client Generation
```bash
# Generate TypeScript client from OpenAPI spec
npm run gen
```

### Zustand Store Pattern
```typescript
interface PilotState {
  pilots: Pilot[];
  selectedPilot: Pilot | null;
  loading: boolean;
  error: string | null;
}

interface PilotActions {
  fetchPilots: () => Promise<void>;
  selectPilot: (pilot: Pilot) => void;
}

type PilotStore = PilotState & PilotActions;

const usePilotStore = create<PilotStore>()((set, get) => ({
  // Implementation...
}));
```

### Deployment Strategy
- **Development**: Vite dev server
- **Production**: Build to static files, deploy to Vercel/nginx
- **API Integration**: Environment-based API URL configuration

### Performance Considerations
- **Code Splitting**: Route-based lazy loading
- **Bundle Analysis**: Monitor bundle size growth
- **Caching**: Static asset caching via CDN
- **API Optimizations**: Minimize API calls, implement caching where appropriate

### Type Safety Workflow
1. Backend API changes → OpenAPI spec regenerated
2. Frontend runs `npm run gen` → TypeScript client updated
3. TypeScript compilation catches breaking changes
4. Fix frontend code to match new API contract

## Date
2025-01-14

## Participants
- Development Team
- Frontend Developers

## Related Decisions
- ADR-0001: Hangfire for Background Jobs (provides data for frontend)
- ADR-0002: SQLite for Data Storage (backend data source)
- Future ADR: Component Library and Design System
- Future ADR: Performance Monitoring and Analytics