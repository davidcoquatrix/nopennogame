# NPNG — Software Architecture & Technical Specifications
## Context for AI Agents (Vibe Coding Blueprint)

This document defines the architectural guidelines for the NPNG (NoPenNoGame) project. 
As an AI assisting on this project, you must strictly adhere to these principles to ensure a maintainable, scalable, and future-proof codebase. 

---

## 1. Architectural Style: Flat Clean Architecture

Although V1 is a pure client-side Blazor WebAssembly (WASM) application without a backend, the architecture must anticipate future evolutions (V2+: backend, real-time sync, database). 

We adopt a **Flat Clean Architecture** pattern. The codebase is organized into logical layers, with strict dependency rules pointing inwards toward the Domain.

### Layer Breakdown

#### 1.1 `NPNG.Domain` (Core Business Logic)
- **Role:** The heart of the application. Contains enterprise logic and types.
- **Contents:** Entities (`GameTemplate`, `Session`, `Player`, `ScoreEntry`), Value Objects, Enums (`ScoreType`), and pure domain services (e.g., `ScoreCalculator`).
- **Rules:** 
  - **Zero dependencies** on any other project layer or external framework (no Blazor, no JS Interop, no EF Core).
  - Must be 100% unit-testable.

#### 1.2 `NPNG.Application` (Use Cases & State Management)
- **Role:** Orchestrates business use cases and holds the application state.
- **Contents:** 
  - Interfaces for external concerns (e.g., `ISessionRepository`, `ILocalStorageService`).
  - State containers (e.g., `GameSessionState` to hold the active game in memory).
  - Use case handlers or managers (e.g., `StartNewSessionCommand`, `AddScoreEntryCommand`).
- **Rules:**
  - Depends ONLY on `NPNG.Domain`.
  - Exposes events (`Action OnStateChanged`) for the UI to react to state mutations.

#### 1.3 `NPNG.Infrastructure` (External Concerns)
- **Role:** Implements the interfaces defined in the Application layer.
- **Contents:**
  - JS Interop wrappers for LocalStorage and IndexedDB.
  - Implementations like `LocalStorageSessionRepository`.
  - In V2+, this is where HTTP Clients or SignalR connections will live to communicate with the backend.
- **Rules:**
  - Depends on `NPNG.Application` and `NPNG.Domain`.
  - Confines all JavaScript interactions (`IJSRuntime`).

#### 1.4 `NPNG.UI` (Blazor WebAssembly)
- **Role:** The presentation layer.
- **Contents:** Blazor Pages (`.razor`), Components, Scoped CSS (`.razor.css`), and static assets.
- **Rules:**
  - Depends on `NPNG.Application` and `NPNG.Domain`.
  - UI components should be as "dumb" as possible. They observe the state from `NPNG.Application` and dispatch user actions to it.
  - Strictly follows the Design System variables defined in `mockup.html`.

---

## 2. Future-Proofing for V2+ (Backend & Interactivity)

The separation of concerns guarantees a smooth transition when a backend is introduced:
- The `NPNG.Domain` can be shared directly with a future ASP.NET Core backend.
- The `NPNG.Application` interfaces (like `ISessionRepository`) will simply get new implementations in `NPNG.Infrastructure` (e.g., `ApiSessionRepository` instead of `LocalStorageSessionRepository`).
- The UI remains completely untouched, as it only talks to the Application layer.

---

## 3. State Management Strategy (Blazor WASM)

Since Blazor WASM runs in the browser, state is lost on refresh unless persisted.
- **In-Memory State:** A Scoped service (e.g., `GameStateService`) holds the current session data.
- **Reactivity:** Components subscribe to `GameStateService.OnChange` and call `StateHasChanged()` when notified.
- **Persistence:** Every mutation in the `GameStateService` triggers a fire-and-forget save to `ILocalStorageService` (via the Infrastructure layer) to ensure crash recovery.

---

## 4. Infrastructure & Hosting (V1)

- **Paradigm:** Static Web App / SPA.
- **Hosting:** GitHub Pages or Azure Static Web Apps (Free Tier).
- **CI/CD:** GitHub Actions to build `dotnet publish -c Release` and deploy the `wwwroot` folder.
- **PWA:** The app must be installable. The default Blazor Service Worker will be configured to cache static assets for offline use.

---

## 5. Coding Directives for AI

- **Do not mix UI and Logic:** Never put complex score calculation or data fetching directly inside a `@code {}` block of a `.razor` file. Inject an Application service instead.
- **Keep it Simple:** This is a solo dev project. Avoid over-engineering (e.g., no MediatR for V1, simple service injection is enough).
- **C# Modernity:** Use C# 13 features where applicable (primary constructors, record types for DTOs/Domain models).
