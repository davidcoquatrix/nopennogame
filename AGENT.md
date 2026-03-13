# NPNG — NoPenNoGame
## Agent Context File — v0.1

This file is the single source of truth for AI tools working on this project.
Read it fully before generating any code, component, or suggestion.

---

## Project Summary

**NPNG** (NoPenNoGame) is a mobile-first PWA for tracking scores in board games.
It replaces pen-and-paper score sheets with a fast, fun, digital interface.
Target: personal use / friend group. Solo developer project.

---

## Tech Stack

| Layer | Choice |
|---|---|
| Framework | .NET 10 / Blazor WebAssembly (PWA) |
| Language | C# 13 |
| Styling | CSS custom properties + scoped Blazor CSS |
| Fonts | Fredoka (UI), Space Mono (scores/numbers) — Google Fonts |
| Storage (V1) | localStorage / IndexedDB via Blazor JS interop |
| Hosting | TBD (GitHub Pages or Azure Static Web Apps) |
| Offline | Service Worker (built-in Blazor PWA template) |

---

## Domain Model

```
GameTemplate
  Id: Guid
  Name: string                  // "Skyjo", "Rami", "Accropolis"...
  ScoreType: ScoreType          // enum — see below
  Description: string?
  Rules: JsonElement?           // structured rules for complex games

Session
  Id: Guid
  TemplateId: Guid              // ref to GameTemplate
  StartedAt: DateTime
  EndedAt: DateTime?
  Status: SessionStatus         // Active | Finished | Abandoned
  Players: List<SessionPlayer>

SessionPlayer
  PlayerId: Guid
  DisplayOrder: int             // Order around the table
  Color: string                 // hex color assigned to this player in this session
  IsFirstPlayer: bool           // Track who starts the current round

Player
  Id: Guid
  Name: string
  DefaultEmoji: string          // e.g. "🎮"

ScoreEntry
  Id: Guid
  SessionId: Guid
  PlayerId: Guid
  Round: int
  Value: int                    // raw score for this round

enum ScoreType
  Cumulative        // total = sum of rounds, higher wins (Rami, Uno)
  CumulativeLower   // total = sum of rounds, lower wins (Skyjo)
  Structured        // per-category scoring with custom rules (Accropolis)

enum SessionStatus
  Active | Finished | Abandoned
```

---

## Roadmap

### Phase 1 — MVP (current focus)
- [x] Blazor PWA project setup
- [ ] GameTemplate model + 2 built-in games: Skyjo, Rami
- [x] Create session screen (pick game, add players)
- [x] Score entry screen (per round, +/- buttons)
- [x] Live leaderboard during game
- [x] Session History / Time Travel UI
- [ ] LocalStorage persistence of active session & State Management

### Phase 2 — History
- [ ] IndexedDB persistence of finished sessions
- [ ] Session history screen
- [ ] End-of-game summary screen

### Phase 3 — Game catalogue
- [ ] More built-in games (Uno, Accropolis, custom)
- [ ] Structured ScoreType implementation (Accropolis categories)
- [ ] Extensible rules engine for new game types

### Phase 4 — Polish
- [ ] Per-player stats (wins, average score, history)
- [ ] Persistent player profiles
- [ ] End-of-game animations
- [ ] Share session summary
- [ ] Full dark/light theme support

---

## Design System

### Color Palette

```css
/* Dark theme */
--bg-primary:    #0D1B2A;   /* deep navy */
--bg-surface:    #162232;   /* card surface */
--bg-elevated:   #1E2F42;   /* elevated elements */
--accent:        #F5E642;   /* lemon yellow — primary CTA */
--accent-dim:    #C9BC1A;   /* darker accent for hover */
--accent-text:   #0D1B2A;   /* text ON accent bg */
--text-primary:  #F0F4F8;
--text-secondary:#8BA3BA;
--success:       #4ADE80;
--danger:        #F87171;
--border:        rgba(255,255,255,0.08);

/* Light theme */
--bg-primary:    #F5F7FA;
--bg-surface:    #FFFFFF;
--bg-elevated:   #EBF0F6;
--accent:        #1B3A5C;   /* navy becomes the accent in light mode */
--accent-dim:    #0D1B2A;
--accent-text:   #F5E642;   /* yellow text on navy */
--text-primary:  #0D1B2A;
--text-secondary:#4A6580;
--success:       #16A34A;
--danger:        #DC2626;
--border:        rgba(0,0,0,0.08);
```

### Typography
- **Display / UI labels**: Fredoka — rounded, friendly, readable at small sizes
- **Scores / numbers**: Space Mono — monospace ensures score columns align perfectly
- Base size: 16px. Scale: 0.65rem (meta) → 0.85rem (body) → 1rem (label) → 1.4rem (score) → 2rem (big score)

### Component Principles
- Border radius: 14–20px on cards, 10px on inputs, 50% on avatars
- Touch targets: minimum 44px height on all interactive elements
- Score buttons (+/−): 36px minimum, well-separated to avoid mispress
- No modals stacked on modals — prefer in-page transitions
- Animations: subtle, fast (150–200ms). No animations during score entry.
- Player identity: each player gets a fixed color + emoji for the session duration

### Key Screens
1. **Home** — active session shortcut + game catalogue
2. **New Session** — pick game → name session → add players
3. **Score Entry** — live leaderboard (top) + round input (bottom)
4. **End of Game** — winner banner + full score recap
5. **History** — past sessions list
6. **Stats** *(Phase 4)* — per-player breakdown

---

## Naming Conventions (C#)

- Models in `NPNG.Core/Models/`
- Services in `NPNG.Core/Services/`
- Blazor pages in `NPNG.App/Pages/`
- Blazor components in `NPNG.App/Components/`
- JS interop helpers in `NPNG.App/Interop/`
- Use `Async` suffix on all async methods
- Prefix interfaces with `I` (e.g. `ISessionService`)

---

## Constraints & Preferences

- **Offline first**: every feature must work without network
- **Mobile first**: design and test at 390px width
- **No auth in V1**: fully local, no accounts, no backend
- **No NuGet bloat**: prefer built-in Blazor/dotnet features over third-party libs
- **Minimal JS**: use JS interop only when Blazor has no alternative (storage, PWA APIs)
- **Solo dev**: keep architecture simple and flat — avoid over-engineering

---

## Out of Scope (for now)

- Multiplayer / real-time sync
- User accounts / cloud save
- Monetization
- Tablet / desktop layout optimization
- Accessibility audit (deferred to post-MVP)