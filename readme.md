# Tour Planner

Tour Planner is a full-stack tour management app for planning, tracking,
and reviewing bike, hike, running, and vacation tours.

It combines an **ASP.NET Core 10 backend** with an **Angular 21 frontend**.

This is a **school project** created for the SWEN semester assignment.

## Main functionality

- User registration, login, and refresh-token auth
- Create, edit, and delete tours and tour logs
- Search tours and logs, including computed values
- OpenRouteService-backed routing with a fallback estimate
- Tour import/export as JSON
- Tour image storage on disk

## Clean Architecture

The backend is split into layers with clear dependency direction:

- **Domain** holds the business entities and rules.
- **Application** defines use cases and interfaces for things like persistence, routing, and file handling.
- **Infrastructure** implements those interfaces with EF Core, PostgreSQL, JWT, filesystem storage, and external APIs.
- **API** exposes controllers and request/response models.
- **TourPlanner** is the host that wires everything together.

In this project, the implementation follows that structure by keeping business logic
out of controllers, putting data access in repositories, and registering dependencies
through DI in the host startup. The frontend stays separate and calls the backend
through the HTTP API.

## Project layout

| Path | Purpose |
| --- | --- |
| `Backend/` | ASP.NET Core API, EF Core, PostgreSQL, auth, file storage, tests |
| `Frontend/` | Angular client with Leaflet and OpenRouteService integration |

## Requirements

- .NET 10 SDK
- Node.js 18+ and npm
- Docker Desktop if you want the easiest backend setup
- An OpenRouteService API key for live routing and geocoding

## Recommended setup

### 1. Start the backend with Docker

```bash
cd Backend
cp .env.example .env
```

Fill in `.env` with your own values. At minimum, set:

- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `POSTGRES_DB`
- `JWT_SIGNING_KEY`
- `OPENROUTE_API_KEY` (optional, but recommended)

Then start the API and database:

```bash
docker compose up --build
```

The API is available at `http://localhost:10001` by default.

Development API docs:

- `http://localhost:10001/scalar/v1`
- `http://localhost:10001/openapi/v1.json`

### 2. Configure the frontend

```bash
cd ../Frontend
npm install
```

Create `src/environments/environment.ts` from the template and add both values:

```ts
export const environment = {
  apiUrl: 'http://localhost:10001',
  orsApiKey: 'YOUR_API_KEY_HERE'
};
```

`apiUrl` must point to the backend that is running locally or in Docker.

### 3. Run the frontend

```bash
npm start
```

Open `http://localhost:4200` in your browser.

## Local development without Docker

If you want to run the backend directly:

```bash
cd Backend
dotnet restore
dotnet run --project TourPlanner/TourPlanner.csproj
```

You need a PostgreSQL database running locally and backend configuration values set through appsettings or environment variables. The default local connection string expects:

- host: `localhost`
- database: `tourplanner`
- user: `postgres`
- password: `postgres`

The backend applies EF Core migrations automatically in development unless `DisableDatabaseMigrations=true` is set.

## Testing

Backend:

```bash
cd Backend
dotnet test TourPlanner.sln
```

Frontend:

```bash
cd Frontend
npm test
```

## Useful notes

- The backend serves interactive API docs only in non-production environments.
- Tour images are stored on disk; the database stores the path, not the file itself.
- If OpenRouteService is unavailable, the backend falls back to a deterministic route estimate.

# Project protocol

## Tracked time

Backend Implementation : 28h
Frontend Implementation : 24h

## Git link

[Git Link](https://github.com/Nareiki/ss26swen2team05) 'https://github.com/Nareiki/ss26swen2team05'

### Architecture

The backend follows Clean Architecture:

- **Domain**: entities, value objects, and business rules
- **Application**: use cases and ports/interfaces
- **Infrastructure**: EF Core, PostgreSQL, JWT, file storage, routing client
- **API**: controllers and HTTP contracts
- **Host**: composition root and DI wiring

The dependency rule is kept strict: outer layers depend inward, while domain code stays independent.

### Use cases

The main use cases are:

- register, login, refresh token, logout
- create, update, delete, and list tours
- create, update, delete, and list tour logs
- search tours and logs, including computed attributes
- import and export tour data as JSON

### UX

The frontend uses a split dashboard layout with:

- tour list sidebar
- interactive Leaflet map
- bottom drawer for tour details and forms
- reusable map and popup components

Wireframe notes are documented in `Frontend/Protocols/UX_Protocol_Design.md`.

### Library decisions

- **Angular** for the frontend UI
- **Leaflet** for map rendering
- **OpenRouteService** for route planning and geocoding
- **EF Core + Npgsql** for PostgreSQL persistence
- **NUnit** for backend unit tests
- **NSubstitute** for better injection Unit testing
- **log4net** for backend logging
- **FluentValidation** for Validation outside of the UseCases
- **Scrutor** for easier Injections into FluidValidation
- **Scalar** for a swagger implementation - accessible with **localhost:PORT:/scalar/v1**

### Design pattern

The main architectural pattern is a combination of:

- **Repository pattern** for persistence access
- **Dependency Injection** for swapping implementations
- **Use-case / application-service pattern** for business workflows

### Unit testing

The backend test suite uses NUnit with in-memory fakes for repositories and services.
The tests focus on use-case behavior instead of implementation details, which keeps them stable and useful.

### Unique feature

The standout feature is route handling with an OpenRouteService integration and a fallback route estimate,
plus computed tour metrics such as popularity and child-friendliness. On the frontend, tours can also be
planned by clicking directly on the map to set the start and destination — each click reverse-geocodes to a
place name and triggers a live route preview via OpenRouteService before the tour is even saved.

### Architecture & Design Patterns
The backend follows a strict Clean Architecture layout matching the traditional UI/BL/DAL framework:
- **Domain**: Core entities, value objects, and business rules.
- **Application (Business Logic)**: Use cases and ports/interfaces. Defines its own custom exceptions to prevent implementation-specific leaks.
- **Infrastructure (DAL)**: EF Core, PostgreSQL, JWT, file storage, and routing clients. Parameterized queries are enforced via EF Core, ensuring the system **does not allow for SQL injection**.
- **API / Frontend (UI)**: Controllers and HTTP contracts. The Angular client maps directly to the **MVVM pattern** via structured components and data-bound view models.

#### Architectural Diagrams

**Class Diagram:** *Documentation/uml_diagram.puml*

**Use Case Diagram** *Documentation/UseCaseDiagram.puml*

**Sequence Diagram** *Documentation/Backend-Sequence-Diagram.puml*

### UX
The frontend uses a responsive split dashboard layout that handles window resizing smoothly:
- Tour list sidebar
- Interactive Leaflet map
- Bottom drawer for tour details and forms
- Reusable map and popup components

*Wireframes and UI design notes can be viewed in the [UX Protocol Design Document](Frontend/Protocols/UX_Protocol_Design.md).*

### Library Decisions & Lessons Learned
- **Angular & Leaflet**: For UI and map rendering.
- **EF Core + Npgsql**: For secure PostgreSQL persistence.
- ... [keep your existing library bullet points] ...

**Lessons Learned:**
Implementing a strict inward-depending Clean Architecture in .NET 10 taught us the value of separating persistence interfaces from their actual SQL implementations. We also learned how to gracefully handle third-party API rate limits by engineering a deterministic mathematical fallback system for route estimation when OpenRouteService is hitting limits.