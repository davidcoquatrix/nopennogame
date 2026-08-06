# NPNG — Product Backlog
## Idea Board & User Stories

This document is maintained by the **Agile Scribe AI**. It translates raw ideas into structured, prioritized features.

---

## 🎯 Phase 1: MVP (The Core Offline Experience)
*Goal: Provide a fully functional, offline-first scoring experience that is strictly better than pen and paper.*

### Epic: Session Setup
- [x] **Story 1.1: Start a Game**
  - [x] UI/UX Integration (Game Catalog)
  - [x] Domain / State Management integration
- [x] **Story 1.2: Manage Players**
  - [x] UI/UX Integration (Add/Remove players, Emoji picker)
  - [x] Domain / State Management integration
- [x] **Story 1.3: Custom Basic Game**
  - [x] UI/UX Integration (Custom rules form)
  - [x] Domain / State Management integration
- [x] **Story 1.7: Abandon Active Session**
  - *As a User, I want to be able to abandon an active session directly from the Home page's "active session" card, so that I can easily start a new game without resuming the old one.*
  - *Must include a confirmation dialog to prevent accidental deletion.*
- [x] **Story 1.8: Game Catalogue Extraction**
  - *As a Developer, I want to extract the hardcoded game list from the UI (Blazor) and move it to a dedicated Application Service, so that the Clean Architecture is respected.*
- [x] **Story 1.9: Auto-completion of Scorekeeper profile**
  - *As a Scorekeeper, I want the app to remember my name and favorite emoji from one game to the next in local storage, so I don't have to re-enter them every time I start a new game.*
- [x] **Story 1.10: Favorite Players**
  - *As a Scorekeeper, I want to be able to save my friends as "Favorite Players" (Name + Emoji) so that I can quickly add them to a new session with a single tap, without re-typing their names.*
  - *These favorites should be displayed below the "Add player" form or "Start Game" button during session setup.*

### Epic: Score Entry
- [x] **Story 1.4: Quick Score Input**
  - [x] UI/UX Integration (Leaderboard & Quick increment buttons)
  - [x] Domain / State Management integration
- [x] **Story 1.5: Correct Mistakes (Time Travel)**
  - [x] UI/UX Integration (Round table, Edit mode)
  - [x] Domain / State Management integration

### Epic: First Player Selection & Turn Order
- [x] **Story 1.6: Player Order & Starting Player**
  - *As a Scorekeeper, I want to reorder players to match the seating order around the table, and manually pick who goes first, so that the app can automatically shift the "first player" badge to the next person each round.*
  - [x] UI/UX Integration (Reordering, First player toggle)
  - [x] Domain / State Management integration

---

## 🚀 Phase 2/3: Polish & Offline Extensions
*Goal: Enhance the offline app with better content and sharing capabilities.*

### Epic: First Player Selection (Enhanced)
- **Story 2.4: Random First Player Animation**
  - *As a group of Players, we want the app to randomly pick the first player with a fun visual animation (e.g., spinning wheel or flashing avatars) to resolve the "who goes first" debate fairly and entertainingly.*
- [x] **Story 2.5: Dynamic First Player Selection (Score-based)**
  - *As a Scorekeeper, I want to configure the first player selection rule (e.g., highest/lowest score of the previous round) so that the app automatically assigns the first player badge each round.*
  - *Tie-breaker rules: 1. Compare total global score. 2. If still tied, use the current table order.*

### Epic: Score Entry (Enhanced)
- [x] **Story 2.9: Lock Player List Once a Game Has Started**
  - *Decision (Product Owner): no valid use case justifies editing the player list once scores exist — it's simpler and safer to block it outright than to build "-" placeholders for players who joined/left mid-game.*
  - *The players screen (`Players/Setup.razor`) is now unreachable once `Session.HasProgress` is true (round > 1, or a round-1 score already recorded) — enforced by a redirect guard on page init, not just a UI warning, so it holds regardless of entry path: back button, the Home "resume active session" card, or a typed URL.*
  - *`Session.HasProgress` (Domain) centralizes the predicate, replacing the duplicated/inverted check that used to live separately in `Index.razor` and the old warning banner in `Players/Setup.razor`.*

### Epic: Game Rules & Templates
- [ ] **Story 2.1: View Rules**
  - *As a Scorekeeper or Player, I want to read a quick summary of the rules for the selected game directly in the app to resolve disputes quickly.*
- [x] **Story 2.2: Advanced Custom Rules**
  - *As a Scorekeeper, I want to set score limits (e.g., "Game ends at 500 points") or round limits when creating a custom game.*
  - *Implemented in `CustomGame/Setup.razor` (score/round limit fields) and enforced in `GameStateService.AdvanceToNextRoundAsync`.*
- [x] **Story 2.6: Save and Manage Custom Games**
  - *As a Scorekeeper, I want to be able to save a custom game configuration (name, scoring type, rules) so that I can reuse it later without having to recreate it.*
  - *As a Scorekeeper, I want to be able to delete a custom game that I previously saved, so that I can keep my game catalog clean.*
- [x] **Story 2.7: Team Scoring (Belote-style "Nous/Vous")**
  - *As a Scorekeeper, I want to group session players into teams that share a single score, so that I can track games like Belote or Tarot where teammates play together instead of individually.*
  - *Teams are formed by grouping existing session players (multi-select + "Créer une équipe" in `Players/Setup.razor`); not hard-limited to 2 teams of 2 — uneven splits are supported. When team mode is on, every player must belong to a team before starting (no mixed individual/team state).*
  - *A custom game (`CustomGame/Setup.razor`) can set "Jouer en équipes par défaut" (`GameRules.TeamsEnabled`), so a game always played in teams (e.g. Belote) pre-enables the toggle on `Players/Setup.razor` for every new session — still overridable per-session.*
  - *Each team gets a display name generated from its members (e.g. "David & Marion"), with the option to rename it — e.g. back to the classic "Nous"/"Vous" convention.*
  - *Score entry becomes one input per team per round instead of one per player; the leaderboard shows the team total with its members listed underneath.*
  - *First-player mechanics stay tied to individual players and are unaffected by team grouping.*
  - *Implementation note: no separate `Team` entity/collection — a team is just the set of `SessionPlayer`s sharing the same `TeamId`, with an optional `TeamCustomName` denormalized across members (`NPNG.Domain.Entities.SessionPlayer`). This avoids adding an `ImmutableArray<Team>` to `Session`, whose C# default can't be `.Empty` (not a compile-time constant) and would deserialize old localStorage sessions into a crash-prone default array. `GameStateService.RecordTeamScoreAsync` writes the same value to every member's individual `ScoreEntry`, so `RecordScoreAsync`, `AdvanceToNextRoundAsync`, `IsGameFinished` and first-player advancement all stay unmodified — teams are purely a presentation grouping (`ScoreCalculator.GroupIntoTeams`, `TeamNameFormatter`).*
  - *Time Travel (`Session/History/Index.razor`) and the history round-replay screen (`Pages/History/Rounds.razor`) are also team-aware: one row per team (via `TeamNameFormatter`/`ScoreCalculator.GroupIntoTeams`), reading/writing through a representative member's `ScoreEntry` since values are identical across teammates by construction.*

### Epic: History
- [x] **Story 2.8: Session History Screen**
  - *As a Scorekeeper, I want to browse the list of my past games (finished or abandoned mid-way) so that I can look back at previous results.*
  - *Implemented via `SessionHistoryService` (Application) filtering `ISessionRepository.GetAllSessionsAsync()` through `SessionHistoryFilter` (Domain), and a new `Pages/History/Index.razor` screen at `/history` — the route the `BottomNav` "Histo." tab already pointed to.*
  - *Abandoned sessions only appear once at least one round has been scored, to avoid cluttering the list with sessions abandoned before they ever started.*
  - *Sessions can be deleted from the history (with confirmation), via a new `ISessionRepository.DeleteSessionAsync`.*
  - *IndexedDB migration remains a separate, not-yet-started item — see `AGENT.md` Phase 2.*

### Epic: Social (Offline)
- **Story 2.3: Share Score Sheet**
  - *As a Scorekeeper, I want to generate an image or text summary of the final scoreboard to share it via WhatsApp/SMS with my friends.*

---

## ☁️ Phase 4 / V2+: Cloud, Real-Time & Magic
*Goal: Introduce a backend, authentication, and advanced interactions.*

### Epic: Cloud Sync & Accounts
- **Story 3.1: User Accounts**
  - *As a Player, I want to log in so that my history and active sessions are saved in the cloud and synced across devices.*
- **Story 3.2: Live Multiplayer (Invite)**
  - *As a Player, I want to join an active session via a link/QR code so that I can view the live score on my own phone and potentially enter my own scores.*

### Epic: Experimental UI
- **Story 3.3: "Draw" the Score**
  - *As a Scorekeeper, I want to draw the number directly on the screen with my finger (gesture recognition) to input scores even faster than using buttons.*
- **Story 3.4: "Touch to Start" (Chwazi Style)**
  - *As a group of Players, we want to all put one finger on the phone screen simultaneously, and have the app magically select one of our fingers as the first player.*