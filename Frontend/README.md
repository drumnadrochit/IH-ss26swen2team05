# Tour Planner — Frontend

A web-based tour planning application built with **Angular 21** and **Leaflet**.
Users can create, manage and log bike, hike, running and vacation tours with
interactive map routing via OpenRouteService.

## Prerequisites

- **Node.js** (v18 or higher) and **npm**
- An **OpenRouteService API key** (free at [openrouteservice.org/dev/#/signup](https://openrouteservice.org/dev/#/signup))

## Setup

### 1. Install dependencies

```bash
npm install
```

### 2. Configure API key

Copy the environment template and add your OpenRouteService API key:

```bash
cp src/environments/environment.template.ts src/environments/environment.ts
```

Then open `src/environments/environment.ts` and replace the placeholder:

```ts
export const environment = {
  orsApiKey: 'YOUR_API_KEY_HERE'  // ← paste your key here
};
```

> **Important:** `environment.ts` is in `.gitignore` and will not be committed.
> This keeps the API key out of version control as required by the project specification.

### 3. Start the development server

```bash
ng serve
```

If you don't have Angular CLI installed globally, use:

```bash
npx ng serve
```

Open [http://localhost:4200](http://localhost:4200) in your browser.
The app reloads automatically when you change source files.

## Login

Authentication is real, backed by the Tour Planner API (JWT access + refresh tokens) — register a
new account from the login screen, there are no mock/demo users anymore.

## Project Structure

```
src/app/
├── components/
│   ├── auth/                  # Login / Register screen
│   ├── dashboard/             # Main layout (list + map + bottom panel), owns the shared map state
│   ├── tour-list/             # Sidebar with tour cards and backend-driven search
│   ├── tour-detail/           # Tour info + logs (bottom panel)
│   ├── tour-form/             # Create / edit tour form, with live route preview
│   ├── tour-log-form/         # Create / edit log form
│   └── shared/
│       ├── map-display/       # Reusable Leaflet map component (click-to-set start/destination)
│       └── popup/             # Reusable confirmation / info popup
├── guards/
│   └── auth-guard.ts          # Route guard redirecting to /auth when logged out
├── interceptors/
│   └── auth.interceptor.ts    # Attaches the JWT to outgoing API requests
├── models/
│   ├── tour.ts                # Tour interface + TransportType enum
│   ├── tour_log.ts            # TourLog interface + Difficulty enum
│   └── API/                   # DTOs mirroring the backend's request/response contracts
├── services/
│   ├── auth.ts                # Login / register / refresh against the real backend
│   ├── tour.ts                # Tour CRUD
│   ├── tour-log.ts            # Tour log CRUD
│   ├── search.ts               # Full-text search across tours and logs
│   ├── import-export.ts        # Account-wide tour export/import
│   └── open-route.ts           # Client-side OpenRouteService calls (geocoding, live preview)
├── utils/
│   └── format.ts                # Shared display formatting (e.g. duration)
└── environments/
    └── environment.template.ts  # API key template (committed)
```

## Features

- User registration/login against the real backend (JWT access + refresh tokens)
- Tour CRUD with name, description, from, to, transport type
- Interactive Leaflet map with sepia-tinted tiles
- Click directly on the map to set the start/destination — reverse-geocodes to a place name and
  computes a live route preview before you save (unique feature; typing a location name works the
  same way via forward geocoding)
- Tour log CRUD with date, distance, time, difficulty, rating, comment
- Automatically computed tour attributes (popularity, child-friendliness)
- Full-text search across tours and logs (including computed values), backed by the API
- Account-wide tour export/import as JSON
- Reusable components (map-display, popup)
- Responsive design (desktop, tablet, mobile)

## Building

```bash
ng build
```

Build output is stored in `dist/`. Production builds are optimized automatically.

## Running Tests

```bash
ng test
```

Uses [Vitest](https://vitest.dev/) as the test runner.

## Technologies

- Angular 21 (standalone components, signals, zoneless change detection)
- Leaflet + OpenRouteService API
- TypeScript
- SCSS with CSS custom properties

## Git Repository

See the git history for the full development timeline.
The repository is linked in the submitted `README.txt` on Moodle.
