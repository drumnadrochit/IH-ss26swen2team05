# Review Guide based on Excel

## Architecture & Principles Review

### 3-Tier / Layer-Based Architecture
Expected structure and checks:
- UI Layer
- Business Logic Layer (BL)
- Data Access Layer (DAL)
- Strict rule: each layer only calls the immediate layer below (or own methods).

Assessment checklist:
- [ ] UI only depends on BL
- [ ] BL only depends on DAL (and domain/shared abstractions)
- [ ] DAL encapsulates persistence details
- [ ] No cross-layer shortcuts (e.g., UI directly to DAL)

### SOLID Principles (reference: https://refactoring.guru/)
Assessment checklist:
- [ ] **S**ingle Responsibility Principle applied in services/view models/repositories
- [ ] **O**pen/Closed Principle via extensible abstractions (interfaces/strategies)
- [ ] **L**iskov Substitution Principle respected in inheritance and interface implementations
- [ ] **I**nterface Segregation Principle (small, cohesive interfaces)
- [ ] **D**ependency Inversion Principle (high-level modules depend on abstractions)

---

## Technology Requirements

- [ ] Uses **C# or Java** for backend (expected: C#)
- [ ] Uses **Angular** as frontend framework
- [ ] Uses **MVVM** for UI
- [ ] Implements **layer-based architecture** (UI/BL/DAL)
- [ ] Implements at least **one design pattern**
- [ ] Uses **PostgreSQL** for storing tour data
- [ ] Prevents **SQL injection**
- [ ] Uses an **OR-Mapping** library
- [ ] Uses configuration (not hardcoded), at minimum **DB connection string**
- [ ] Integrates **OpenRouteServices.org API** and **Leaflet**
- [ ] Implements at least **20 unit tests**

---

## Feature Requirements

### GUI in General
- [ ] Correct data binding between UI elements and view model properties
- [ ] UI responds to window size changes (responsive behavior)
- [ ] Defines reusable UI component(s)

### Tours
- [ ] Create / modify / delete tour (including DAL support)
- [ ] Tour has required attributes (including image) and is managed in list view
- [ ] Tour has computed attributes
- [ ] Tour details show all selected tour attributes including map image
- [ ] User input validation (no crash on wrong input)

### Tour Logs
- [ ] Create / modify / delete tour log (including DAL support)
- [ ] Tour log has required attributes
- [ ] Logs of selected tour displayed with all log attributes in list view
- [ ] User input validation (no crash on wrong input)

### Full-Text Search
- [ ] Search covers tours, tour logs, and computed attributes
- [ ] Tour list reflects current search results

### Import / Export
- [ ] Export tour data
- [ ] Import tour data

### Mandatory Unique Feature
- [ ] Unique feature implemented and documented

---

## Non-Functional Requirements

- [ ] Layers only call immediate lower layer (or own methods)
- [ ] Layers define own exceptions; avoid leaking implementation-specific exceptions
- [ ] Uses OpenRouteServices.org Directions API for tour retrieval
- [ ] Uses Leaflet for map rendering
- [ ] All tour data (except optional image binary handling) persisted in DB
- [ ] All configuration stored in config (not code)
- [ ] Logs exceptions, errors, and technical diagnostics
- [ ] Unit tests are high quality (useful, non-duplicative)

---

## Protocol / Documentation Requirements

- [ ] Describes app architecture (layers, responsibilities, class diagrams)
- [ ] Describes use cases (use-case and sequence diagrams)
- [ ] Describes UX (including wireframes)
- [ ] Describes library decisions and lessons learned
- [ ] Describes implemented design pattern
- [ ] Describes unit testing decisions
- [ ] Describes unique feature
- [ ] Contains tracked time
- [ ] Contains link to Git repository

---

## Recommended Evidence Collection for Re-Analysis
To complete a precise re-analysis, verify and reference:
1. Solution/project structure (UI/BL/DAL separation)
2. Dependency direction in project references
3. ORM and DB provider configuration (PostgreSQL)
4. API integration points for OpenRouteServices and Leaflet
5. Input validation paths (Tours and Tour Logs)
6. Search implementation (full-text scope)
7. Import/export implementation details
8. Unit test count and test quality
9. Logging and exception strategy
10. Documentation artifacts (diagrams, protocol sections, tracked time)

---

## Notes
- This file structures your feedback into a grading-ready checklist.
- If desired, I can generate a second version with weighted scoring per requirement.
