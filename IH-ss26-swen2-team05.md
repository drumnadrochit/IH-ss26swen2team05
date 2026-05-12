# Intermediate Hand-In Review — Team 05 (SS26 SWEN2)

**Repository reviewed:** [drumnadrochit/IH-ss26swen2team05](https://github.com/drumnadrochit/IH-ss26swen2team05) (fork of [Nareiki/ss26swen2team05](https://github.com/Nareiki/ss26swen2team05))
**Review date:** 2026-05-12
**Reviewed commit:** `6b8e31d` on `main`
**Language composition:** SCSS 44 % · TypeScript 34.9 % · HTML 21 % · C# 0.1 %

---

## 1. Executive Summary

The team delivers a polished, visually consistent Angular SPA (“Cartographer’s Atlas” theme) that covers nearly all functional must-haves for the Intermediate Hand-In: authentication, tour CRUD, log CRUD, list view, detail view, validation, reusable components, responsive layout, and even goes beyond the requirement by integrating a real Leaflet map with OpenRouteService routing/geocoding instead of the required *map placeholder*. The protocol with wireframes is present and well written.

The main weaknesses are **architectural / structural** rather than functional:

- The "3-tier architecture" is essentially **missing**: the `Backend/TourPlanner.API` is an empty `Hello, World!` console app, so there is no Presentation / Business Logic / Data Access separation across tiers — everything lives in the Angular client and is backed by in-memory mock data.
- The frontend follows an Angular **service + standalone-component** pattern rather than a clean **MVVM** separation. Components do hold reactive state via signals (acting as informal view models), but there are no dedicated ViewModel classes.
- The central `TourService` violates the **Single Responsibility Principle** — it owns persistence, business rules, statistics, search, import/export and route hydration in one class.
- A few classic *refactoring smells* from [refactoring.guru](https://refactoring.guru/) are visible: `switch`-on-enum (replace with polymorphism), feature envy, primitive obsession on coordinates, dead/empty classes (`TourLog` service).

Overall: **on track for the IH grade**, with concrete and small refactorings that would significantly raise the architecture and SOLID score for the final hand-in.

---

## 2. Architecture Review

### 2.1 3-Tier Architecture — ❌ not implemented yet

| Tier | Expected | Found in repo |
|---|---|---|
| **Presentation (UI)** | Angular components, templates, routing | ✅ `Frontend/src/app/components/**`, `app.routes.ts` |
| **Business Logic** | Services orchestrating use-cases, validation, computed values | ⚠️ Mixed into `TourService` (`Frontend/src/app/services/tour.ts`) — also performs persistence and external API calls |
| **Data Access** | Repository / DAL / Backend API + DB | ❌ Replaced by mock arrays (`mock_data/mock_tours.ts`, `mock_tour_logs.ts`, `users.ts`) and `sessionStorage`; backend is empty |

> 📄 `Backend/TourPlanner.API/Program.cs` is literally:
> ```csharp
> // See https://aka.ms/new-console-template for more information
> Console.WriteLine("Hello, World!");
> ```
> No controllers, no DTOs, no persistence layer, no `dotnet` web host configuration.

**Recommendation for final hand-in:**

1. Implement the C# backend as an ASP.NET Core Web API with at least three projects:
   - `TourPlanner.API` (Presentation: Controllers + DTOs)
   - `TourPlanner.BL`  (Business Logic: services, validators, route computation, child-friendliness/popularity calculations)
   - `TourPlanner.DAL` (Data Access: EF Core `DbContext`, repositories, migrations against PostgreSQL/SQL Server)
2. Move the OpenRouteService calls (`open-route.ts`) into the BL/DAL tier so the API key is no longer exposed to the browser.
3. Replace `signal<Tour[]>([...MOCK_TOURS])` in `TourService` with calls to the backend through an Angular `HttpClient`-based repository (an interface like `ITourRepository`).

### 2.2 Frontend layering (today)

```
Frontend/src/app
├── components/        # View layer (standalone Angular components)
│   ├── auth/
│   ├── dashboard/        ← orchestrator, owns selection & panel state
│   ├── tour-list/
│   ├── tour-detail/
│   ├── tour-form/
│   ├── tour-log-form/
│   └── shared/
│       ├── map-display/   ← reusable
│       └── popup/         ← reusable
├── services/          # "BL + DAL" mixed: tour.ts, auth.ts, open-route.ts, tour-log.ts (empty)
├── models/            # Plain TS interfaces & enums: Tour, TourLog, User, Difficulty, TransportType
├── mock_data/         # In-memory "database"
└── guards/            # authGuard
```

This is reasonable for a frontend-only iteration, but it should be re-cut into three **logical** tiers (view ↔ view-model ↔ service-facade-over-API) for the final hand-in.

---

## 3. MVVM Assessment

### 3.1 What is correct ✅
- The **Model** layer is clean: `models/tour.ts`, `models/tour_log.ts`, `models/user.ts` are pure TypeScript interfaces / enums without UI concerns.
- The **View** layer is well separated into `*.html` templates with `*.scss` styles, and the templates use Angular's modern control flow (`@if`, `@for`, `@switch`).
- Components use **signals** (`signal`, `computed`, `update`, `asReadonly`) to expose reactive state — conceptually this *is* a ViewModel in Angular terms.
- Inputs/Outputs are correctly used to keep child components dumb and the `DashboardComponent` acts as a coordinator.

### 3.2 What is missing / could be improved ⚠️
- There is no explicit **ViewModel class** per view. Logic such as `filteredTours`, `deleteMessage`, `avgRating`, `avgDifficulty`, validation rules and form state machines live directly inside the component class, mixed with UI event handlers. For a proper MVVM split:
  - Extract a `TourListViewModel`, `TourDetailViewModel`, `TourFormViewModel`, `TourLogFormViewModel` (plain TS classes or `@Injectable()` providers).
  - The component should only forward DOM events and bind to ViewModel signals — no business decisions in the component class.
- The component currently knows about both the `TourService` *and* the `AuthService` (`dashboard.ts` constructor). In MVVM this is what a **ViewModel** should orchestrate, not the View.

### 3.3 Data binding
- Two-way / one-way bindings are **idiomatic and correct**: `[value]="searchQuery()"` + `(input)="onSearch($event)"` is used consistently instead of `[(ngModel)]` for signal-friendly flow. ✅
- `@Input set tours(value)` properly forwards inputs into local signals. ✅
- Outputs are typed `EventEmitter<Tour>`, `EventEmitter<TourLog>`, `EventEmitter<void>`. ✅

---

## 4. SOLID Principles

### 4.1 Single Responsibility Principle — ⚠️ violated in `services/tour.ts`

`TourService` (`Frontend/src/app/services/tour.ts`) is currently responsible for:

1. In-memory storage of tours (`tours = signal<Tour[]>(...)`)
2. In-memory storage of logs (`tourLogs = signal<TourLog[]>(...)`)
3. Id generation (`nextTourId`, `nextLogId`)
4. CRUD for tours
5. CRUD for logs
6. Business rule: **child-friendliness scoring** (`recomputeTourStats`)
7. Business rule: **popularity** counting
8. Full-text search across tours *and* their logs (`searchTours`)
9. Import / export to JSON (`exportTour`, `importTour`)
10. Hydration of mock data by calling `OpenRouteService` on construction (`loadMockRoutes`)

**Refactoring suggestions (refactoring.guru):**
- *Extract Class* → `TourRepository` (1–4), `TourLogRepository` (2,5), `TourStatsCalculator` (6–7), `TourSearchService` (8), `TourImportExportService` (9), `TourRouteHydrator` (10).
- *Move Method* → `recomputeTourStats` should live in a `TourStatsCalculator` operating on a `Tour` + `TourLog[]` pair (pure function, easily unit-testable).

The empty class `services/tour-log.ts` (`export class TourLog {}`) is a good place to grow into a real `TourLogService` / `TourLogRepository`.

### 4.2 Open/Closed Principle — ⚠️

- `recomputeTourStats` hard-codes the difficulty → numeric mapping inside the method (`{ EASY:1, MEDIUM:2, HARD:3, EXPERT:4 }`). Adding a new `Difficulty` value requires editing the method (closed for extension, open for modification — wrong way around).
- `OpenRouteService.getProfile()` uses a `switch` over `TransportType` strings.
- `tour-list.html`, `tour-detail.html` and `tour-form.html` each repeat a `@switch (tour.transportType)` block to pick an SVG icon — the same enum is switched on in **three different places**.

**Refactoring suggestion:** *Replace Conditional with Polymorphism* (or simpler in TS: a configuration map):
```ts
export const TRANSPORT_CONFIG: Record<TransportType, { profile: string; iconPath: string; label: string }> = { ... };
export const DIFFICULTY_WEIGHT: Record<Difficulty, number> = { EASY:1, MEDIUM:2, HARD:3, EXPERT:4 };
```
…then the components and services only do a lookup, and adding a new transport/difficulty is a one-line change.

### 4.3 Liskov Substitution Principle — ✅ (not really exercised; no inheritance hierarchies of concern)

### 4.4 Interface Segregation Principle — ⚠️

- `TourService` exposes a single fat class to every component that imports it. A component that only needs to list tours pulls in CRUD, search, import/export and route hydration.
- Introduce small interfaces: `ITourReader`, `ITourWriter`, `ITourLogReader`, `ITourLogWriter`, `ITourSearch`, `ITourImportExport`, and provide them as Angular DI tokens so each component depends only on what it uses.

### 4.5 Dependency Inversion Principle — ⚠️

- Components reference **concrete** services directly (`constructor(private tourService: TourService, private authService: AuthService, private router: Router)` in `dashboard.ts`).
- For testability, depend on **interfaces** registered with Angular DI tokens; the mock implementation (current behaviour) and the real HTTP-backed implementation (final hand-in) become drop-in replacements without touching components.
- The `AuthService` and `TourService` directly import mock arrays — DIP would say the *data source* should be injected.

---

## 5. Refactoring (refactoring.guru) — concrete code smells found

| # | Smell | Location | Suggested refactoring |
|---|---|---|---|
| 1 | **God Class** | `services/tour.ts` (~190 LOC, 10 responsibilities) | *Extract Class*, *Split Phase* |
| 2 | **Switch Statements** on enums | `tour-list.html`, `tour-detail.html`, `tour-form.html`, `open-route.ts#getProfile`, `tour.ts#recomputeTourStats` | *Replace Conditional with Polymorphism* / config map |
| 3 | **Dead Code / Lazy Class** | `services/tour-log.ts` is an empty `@Injectable` class | *Inline Class* (remove) or grow it into a real `TourLogService` |
| 4 | **Primitive Obsession** | `[number, number]` tuples for coordinates everywhere | *Replace Data Value with Object* → introduce `Coordinate` / `LatLng` type with helpers |
| 5 | **Duplicated Code** | `formatTime(minutes)` is duplicated in `tour-list.ts`, `tour-detail.ts`, `tour-form.ts` | *Extract Function* into `src/app/utils/format.ts` |
| 6 | **Long Method** | `recomputeTourStats` performs 4 calculations + map updates | *Extract Function* per metric |
| 7 | **Feature Envy** | `TourFormComponent.tryCalculateRoute` reaches into `OpenRouteService` API and converts units that the service already does — fine, but `DashboardComponent` reaches into `TourFormComponent` via `ViewChild` (`this.tourForm?.onMapFromSelected(coords)`) | *Replace with event emitter* through a shared signal store |
| 8 | **Magic Numbers** | Child-friendliness scoring uses `0.8`, `120`, `20`, `5` with no explanation | *Symbolic Constant* + JSDoc |
| 9 | **Secret in Frontend** | `OpenRouteService.apiKey = environment.orsApiKey` — any key shipped to the browser is public | Move ORS calls to backend (also fixes 3-tier) |

---

## 6. Must-Haves Checklist (Intermediate Hand-In)

Legend: ✅ done · 🟡 partially / with caveats · ❌ missing

### Framework & Architecture
| Requirement | Status | Evidence |
|---|---|---|
| Uses **Angular** as frontend framework | ✅ | `Frontend/package.json` → `@angular/* ^21.2.0`, `ng serve`/`ng build` scripts |
| Uses **MVVM** for UI | 🟡 | Models + standalone components with signal-based state. No explicit ViewModel classes (see §3.2). |

### GUI — General
| Requirement | Status | Evidence |
|---|---|---|
| Correct data binding between UI and view-model properties | ✅ | Idiomatic signal-binding: `[value]="searchQuery()" (input)="onSearch($event)"`, `@Input set tours(v) { this.allTours.set(v) }`. Outputs strongly typed. |
| UI responds to window size changes | ✅ | Documented breakpoints in `UX_Protocol_Design.md` (1024 px / 768 px); SCSS files exist for each component (`tour-list.scss` 7 KB, `dashboard.scss` 5 KB, etc.). Worth adding a screenshot in the protocol. |
| Defines reusable UI component | ✅ | `shared/popup/popup.ts` is reused by `tour-list` (delete tour confirm) and `tour-detail` (delete log confirm). `shared/map-display/map-display.ts` is reused inside the dashboard and (planned) the tour form. |

### Tours
| Requirement | Status | Evidence |
|---|---|---|
| Create / modify / delete tour | ✅ | `tour-form.ts#onSave` + `TourService#createTour/updateTour/deleteTour`, wired through `dashboard.ts#onTourFormSave/onTourEdit/onTourDelete`. |
| Tour has required attributes (incl. **image**) | ✅ | `models/tour.ts` defines `name, description, from, to, transportType, distance, estimatedTime, routeGeoJson, routeImagePath, imageUrl, popularity, childFriendliness, fromCoords, toCoords`. The form has an Image URL field with a live preview. |
| Managed in a **list view** | ✅ | `tour-list.html` with search, card per tour, transport icon, distance/time/popularity stats. |
| **Tour Details** show all attributes + map-placeholder | ✅ (exceeds) | `tour-detail.html` shows from/to, distance, est. time, popularity, child-friendliness, transport. The "map placeholder" was implemented as a real Leaflet map (`shared/map-display`) — already exceeds the requirement. |
| Validates user input (no crash on wrong input) | ✅ | `TourFormComponent#validate()` (name/from/to required) + `[class.invalid]` styling + field-level error messages. Geocoding/route failures degrade gracefully via `statusMessage`. |

### Tour Logs
| Requirement | Status | Evidence |
|---|---|---|
| Create / modify / delete tour log | ✅ | `tour-log-form.ts#onSave` + `TourService#createLog/updateLog/deleteLog`. Edit triggered from `tour-detail.html` row click, delete via popup confirm. |
| Tour log has required attributes | ✅ | `models/tour_log.ts`: `id, tourId, dateTime, comment, difficulty, totalDistance, totalTime, rating`. |
| Show all logs of selected tour with all attributes in a list view | ✅ | `tour-detail.html#logs-scroll` lists every log of the selected tour with date, ★ rating, distance, time, difficulty and comment. |
| Validates user input | ✅ | `TourLogFormComponent#validate()`: `dateTime` required, `totalDistance > 0`, `totalTime > 0`, with per-field error display. |

### Protocol
| Requirement | Status | Evidence |
|---|---|---|
| Describes UX (incl. wireframes) | ✅ | `Frontend/Protocols/UX_Protocol_Design.md` — 290 lines, includes initial + final ASCII wireframes (desktop & mobile), design-evolution narrative, UI flow diagram and a design-decisions summary table. Consider adding **rendered** wireframes (PNG/SVG) for clarity. |

---

## 7. Strengths

- **Modern Angular usage:** standalone components, `signal`/`computed`, `ChangeDetectionStrategy.OnPush`, new control-flow syntax (`@for`, `@if`, `@switch`).
- **Reusable shared components** (`popup`, `map-display`) are actually reused.
- **Polished UX:** consistent theme, custom SVG icons, sepia map tiles, well-thought responsive breakpoints.
- **Tests scaffolded** for every component and service (`*.spec.ts` present everywhere) — even if currently shallow, the structure is in place for the final hand-in.
- **Goes beyond the spec** by implementing real geocoding/routing via OpenRouteService.
- **Protocol with wireframes** is detailed and clearly explains the design evolution.

---

## 8. Concrete Action Items for the Final Hand-In

1. **Build the real backend (3-tier).** Replace `Hello, World!` with an ASP.NET Core solution split into `TourPlanner.API` / `TourPlanner.BL` / `TourPlanner.DAL`, including EF Core + a relational DB and migrations.
2. **Move OpenRouteService calls to the backend** to hide the API key and keep the frontend pure.
3. **Introduce explicit ViewModel classes** (`*-view-model.ts`) so components only do binding/UI events.
4. **Split `TourService`** into Repository / Stats / Search / Import-Export / RouteHydrator classes (§4.1).
5. **Replace `switch` over `TransportType` / `Difficulty`** with config maps or a strategy pattern (§4.2).
6. **Extract shared helpers** (`formatTime`, transport icons, difficulty labels) into `src/app/utils/`.
7. **Introduce DI tokens** for repository interfaces so the mock backend and real backend are swappable (§4.5).
8. **Remove the empty `services/tour-log.ts` stub** or implement it.
9. **Flesh out the existing `*.spec.ts` test files** with assertions on validation, signal updates and CRUD flows; add at least one BL-layer unit test in the backend.
10. **Embed rendered wireframes (PNG/SVG)** in `UX_Protocol_Design.md` next to the ASCII versions, and add a screenshot section of the running app.

---

## 9. Verdict

- **Functional Must-Haves:** essentially complete — Angular, MVVM-ish, data binding, responsive, reusable components, tour & log CRUD with validation, list + details, and a written protocol with wireframes.
- **Architectural Must-Haves:** the 3-tier separation is the largest gap; SOLID is mostly respected at the component level but the central service is a clear SRP violation.
- **Recommendation:** treat the action items in §8 as the work plan for the Final Hand-In. With the backend in place and `TourService` split up, this project will be in very good shape for the final grade.
