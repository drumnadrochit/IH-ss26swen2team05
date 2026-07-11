# TourPlanner Backend

This backend is structured using a strict Clean Architecture approach:

*   `TourPlanner` – ASP.NET Core host / composition root
*   `TourPlanner.API` – Presentation layer with controllers
*   `TourPlanner.Application` – Use cases and application ports
*   `TourPlanner.Domain` – Entities, value rules, and business calculations
*   `TourPlanner.Infrastructure` – EF Core, PostgreSQL, JWT, filesystem storage, and external API integrations
*   `TourPlanner.Tests` – NUnit-based unit tests

---

## Key Backend Features

*   **Authentication:** JWT-based authentication featuring user registration, login, and token refresh.
*   **Tours & Logs:** Full CRUD operations for tours and corresponding tour logs.
*   **Route Planning:** OpenRouteService-backed route planning featuring a local, deterministic fallback.
*   **Storage:** Tour image storage managed directly on the local filesystem.
*   **Search Engine:** Full search capabilities across tours and tour logs, including computed values.
*   **Data Portability:** Seamless import and export of tours via JSON format.
*   **Metrics:** Automated popularity and child-friendliness scoring algorithms.
*   **Logging:** Robust application logging powered by log4net.

---

## Configuration

Keep secrets and environment-specific values strictly outside of the source code.
The included `compose.yaml` file automatically wires these values via environment variables.

### Required Configuration Keys

*   `ConnectionStrings:DefaultConnection`
*   `Jwt:Issuer`
*   `Jwt:Audience`
*   `Jwt:SigningKey`
*   `Storage:BasePath`
*   `OpenRouteService:ApiKey`

---

## Run Locally

Execute the following commands in your terminal to restore dependencies, run unit tests, and start the host application:

```bash
cd /Users/christianredl/TechnikumWien/Semester4/SWEN/ss26swen2team05/Backend
dotnet restore
dotnet test
cd TourPlanner
dotnet run