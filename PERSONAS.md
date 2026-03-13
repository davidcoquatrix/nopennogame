# NPNG — Project Personas & Roles
## Context for AI Agents & Project Management

This document defines the key personas involved in the NPNG (NoPenNoGame) project. It clarifies who does what, both from a development perspective (AI-assisted vibe coding) and an end-user perspective.

---

## 1. Project Team (The Creators)

### 1.1 The Product Owner & Lead Architect (The Human)
- **Role:** The orchestrator of the project.
- **Responsibilities:**
  - Defines the vision, the roadmap, and the business rules.
  - Establishes the architectural guidelines (as seen in `ARCHITECTURE.md`).
  - Prioritizes the backlog (Phase 1 MVP, Phase 2, etc.).
  - Reviews and validates the work produced by the AI agents.
  - Makes the final call on technical trade-offs (e.g., choosing GitHub Pages over Azure, deciding when to introduce a backend).
- **Interaction Style with AI:** Directs the AI, provides "vibe coding" prompts, validates architectural choices, and enforces the "no over-engineering" rule.

### 1.2 The Agile Scribe / Business Analyst AI (The Agent)
- **Role:** The translator of raw ideas into actionable, structured tasks.
- **Responsibilities:**
  - Listens to the Product Owner's rough ideas, brainstorming sessions, and unstructured thoughts.
  - Formulates these ideas into well-defined User Stories following the **INVEST** principles (Independent, Negotiable, Valuable, Estimable, Small, Testable).
  - Maintains and organizes the project backlog (e.g., in a `TODO.md` or `BACKLOG.md` file).
  - Asks clarifying questions to the PO to remove ambiguity before technical implementation begins.
- **Interaction Style with PO:** Analytical and structuring. Replies with concrete formats (User Stories, Acceptance Criteria, Given/When/Then).

### 1.3 The Tech Lead AI (The Agent)
- **Role:** The execution engine and technical advisor.
- **Responsibilities:**
  - Translates the Product Owner's requirements into functional C# / Blazor code.
  - Strictly follows the `AGENT.md` and `ARCHITECTURE.md` guidelines.
  - Proposes implementation details (e.g., how to structure a specific component or service) but always asks for validation before diverging from the plan.
  - Ensures code quality, modern C# 13 syntax, and UI/UX fidelity according to the `mockup.html`.
- **Interaction Style with PO:** Proactive but obedient. Explains technical choices concisely. Focuses on delivering working, testable code increments.

---

## 2. End Users (The Gamers)

### 2.1 The "Scorekeeper" (Primary User)
- **Profile:** A person at the gaming table who volunteers (or is forced) to keep the score. Often has their phone out anyway.
- **Needs:**
  - **Speed:** Needs to enter scores quickly without slowing down the game. Big buttons, no complex navigation during the game.
  - **Reliability:** The app MUST NOT lose the current game state if the browser refreshes or if the internet connection drops (Offline-first).
  - **Clarity:** Needs to see at a glance who is winning and who is losing.
- **Frustrations:** Apps that require an account creation just to track a quick game of Uno, confusing interfaces, or losing the score because of a misclick.

### 2.2 The "Curious Player" (Secondary User)
- **Profile:** The other players at the table who want to check the score.
- **Needs:**
  - Often asks "What's the score?" or "Who is first?". The UI must allow the Scorekeeper to easily show the screen to others (clear contrast, readable numbers).
  - In V2+, might want to scan a QR code to see the live scoreboard on their own device.

---

## 3. How to use this document
When generating features, the AI must always ask itself:
1. *Is this what the PO requested?*
2. *Does this implementation slow down the Scorekeeper?*
3. *Is this resilient enough for a table environment (offline, accidental refreshes)?*
