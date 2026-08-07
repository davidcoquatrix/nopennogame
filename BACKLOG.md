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
  - *The "Incrément par défaut des boutons" toggle (±1/±10, ±5/±50, Libre) on the same screen used to be pure decoration — it set a local `incrementType` string that nothing read. Wired up: `GameRules.ScoreIncrement` (`ScoreIncrementMode`, Domain enum) now flows through `ScoreInput.razor`'s new `Mode` parameter, which computes the actual ±step and, for "Libre", hides the quick buttons entirely so only keyboard entry remains. All 4 `ScoreInput` call sites (`Session/Active.razor`, `Session/History/Index.razor`, individual + team each) read it from `Template.Rules.ScoreIncrement`.*
- [x] **Story 2.6: Save and Manage Custom Games**
  - *As a Scorekeeper, I want to be able to save a custom game configuration (name, scoring type, rules) so that I can reuse it later without having to recreate it.*
  - *As a Scorekeeper, I want to be able to delete a custom game that I previously saved, so that I can keep my game catalog clean.*
- [x] **Story 2.7: Team Scoring (Belote-style "Nous/Vous")**
  - *As a Scorekeeper, I want to group session players into teams that share a single score, so that I can track games like Belote or Tarot where teammates play together instead of individually.*
  - *Teams are formed by grouping existing session players (multi-select + "Créer une équipe" in `Players/Setup.razor`); not hard-limited to 2 teams of 2 — uneven splits are supported. When team mode is on, every player must belong to a team before starting (no mixed individual/team state).*
  - *A custom game (`CustomGame/Setup.razor`) sets `GameRules.Teams` (a `TeamRules?`) to decide once and for all whether it's played in teams (e.g. Belote) — team mode is entirely config-driven, there's no per-session toggle on `Players/Setup.razor` to override it. The presence of `Teams` (instead of a separate `TeamsEnabled` bool) *is* the on/off switch, so a game not configured for teams can never go ad hoc into team mode, and vice versa.*
  - *`TeamRules(TeamSize, RequireEqualTeams)` constrains team composition — e.g. Belote/Tarot at a fixed size per team. `TeamRules.RequiresEqualSizes` (`TeamSize is not null || RequireEqualTeams`) makes explicit that a fixed size implies equal teams — there's no representable state where `TeamSize` is set but teams aren't required to match it. `CustomGame/Setup.razor` hides the "Équipes de taille égale obligatoire" checkbox once a team size is entered, for the same reason. Enforced only in `Players/Setup.razor` (disables "Créer une équipe"/"Ajouter la sélection" once a team would exceed the configured size, and gates "Lancer la partie" until every team matches) — not in `GameStateService`, consistent with how `MinTeamsRequired` was already UI-only.*
  - *Adding a player to an already-formed team (not just creating new ones) is supported via `GameStateService.AddPlayersToTeamAsync` + a "➕" button on each team card in `Players/Setup.razor`, reusing the same checkbox multi-select as team creation. Drag-and-drop was considered and rejected again for the same touch-reliability reason as the original player-reorder DnD removal (`5f0d873`).*
  - *Each team gets a display name generated from its members (e.g. "David & Marion"), with the option to rename it — e.g. back to the classic "Nous"/"Vous" convention.*
  - *Score entry becomes one input per team per round instead of one per player; the leaderboard shows the team total with its members listed underneath.*
  - *First-player mechanics stay tied to individual players and are unaffected by team grouping.*
  - *Implementation note: `Team` (`NPNG.Domain.Entities.Team` — `TeamId`/`CustomName`/`CustomEmoji`) is a first-class collection on `Session.Teams`; `SessionPlayer` only carries a plain `Guid? TeamId` foreign key, no data duplicated across members. An earlier version denormalized the custom name onto every member instead, specifically to dodge an `ImmutableArray<Team>` field deserializing old localStorage sessions into a crash-prone default (`ImmutableArray<T>`'s default isn't `.Empty` and isn't a valid compile-time default-parameter value either) — normalized once the app no longer needed to stay compatible with pre-existing local sessions. `GameStateService.RecordTeamScoreAsync` writes the same value to every member's individual `ScoreEntry`, so `RecordScoreAsync`, `AdvanceToNextRoundAsync`, `IsGameFinished` and first-player advancement all stay unmodified — teams are purely a presentation grouping (`ScoreCalculator.GroupIntoTeams`, `TeamNameFormatter`, `TeamEmojiFormatter`).*
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

## 🧹 Tech Debt & Code Quality (from full-repo review)

*Full-repo review conducted after the Team normalization refactor. The `ResumeSessionAsync` bug below (and its missing test coverage) has already been fixed; everything else is deliberately left for later — not urgent, but worth tracking rather than forgetting.*

### Architecture / consistency
- [x] **Missing `ILocalStorageService` abstraction.** `ARCHITECTURE.md` (section 1.2) names it as the intended JS-interop wrapper for LocalStorage, but it was never built. Instead, `LocalStorageSessionRepository`, `LocalStoragePlayerProfileRepository`, and `LocalStorageGameCatalogueService` each hand-rolled identical `JsonSerializerOptions` plus the same get/set/try-catch-deserialize boilerplate.
  - *Implemented as `NPNG.Application.Interfaces.ILocalStorageService` (`GetItemAsync<T>`/`SetItemAsync<T>`/`RemoveItemAsync`/`GetAllItemsAsync<T>`) with a single JS-interop implementation, `NPNG.Infrastructure.Services.LocalStorageService`, registered in `Program.cs`. The three repos now depend on it instead of `IJSRuntime` directly and lost all their JSON boilerplate. `LocalStorageThemeService`/`LocalStorageAppDataResetService` were left untouched — they don't fit the JSON get/set shape (custom JS helper / raw key enumeration).*
- [x] **`_Imports.razor` doesn't import `NPNG.Application.Models`/`NPNG.Application.Interfaces`** the way it does `NPNG.Domain.*`, forcing `Index.razor`, `Settings.razor`, `CustomGame/Setup.razor` to fully-qualify `Application.Models.GameCatalogueItem`, `Application.Interfaces.IGameCatalogueService`, etc. repeatedly.
- [ ] **`GameStateService` is 600+ lines / ~25 public methods** spanning session lifecycle, player setup, team management, scoring, and round progression. Each method stays small, but it's worth watching against `STANDARDS.md`'s SRP guidance if it keeps growing — a candidate split would be team management as its own concern. *(Left as-is — advisory only, no active pain yet.)*

### Duplication
- [x] **`AkropolisScoreSheet.razor`**: 5 near-identical ~15-line blocks (Carrières/Habitations/Marchés/Casernes/Jardins), differing only by label and which `AkropolisCategoryScore` property they bind. Replaced with a loop over a `CategoryConfig` list (title, CSS class, selector, updater).

### Dead code
- [x] **Unused `IJSRuntime` injections** in `Players/Setup.razor`, `CustomGame/Setup.razor`, and `Session/Active.razor` — likely left behind by the drag-and-drop removal (`5f0d873`). Deleted.
- [x] **`GameTemplate.Description`** was assigned (via `GameCatalogueItem.ToGameTemplate()`) but never read once embedded in a `Session` — the only place a description is actually shown (`Index.razor`'s game selection tile) reads `GameCatalogueItem.Description` directly, before conversion. Removed from `GameTemplate`; kept on `GameCatalogueItem`, where it's real.
- [x] **The `?game=Name` query param** on `/players` was purely decorative (fed only the header title) and, worse, went stale on F5: the actual session is reloaded from `GameState.LoadLatestActiveSessionAsync()` (most recent Active/Setup session in storage) independently of the URL, so the title could show a different game than the one actually loaded if the URL was stale (back button, reopened tab). Removed the query param entirely; the header now reads `GameState.CurrentSession.Template.Name` directly, which can't drift from the loaded session.

### Minor polish
- [x] `LocalStorageGameCatalogueService.cs`: redundant comment restating the literal directly above it (`true); // Set to true for custom games`) — against the `STANDARDS.md` rule on comments. Removed.
- [x] `Index.razor.TryContinueToSetup()`: `_ = ConfirmAndContinue();` discarded the task inside an already-`async Task` method — now `await`ed, so an exception from `GameState.InitializeNewSessionAsync` no longer gets silently swallowed.
- [x] `LocalStorageGameCatalogueService.GetCustomGamesAsync`'s defensive `IsCustom = true` re-coercion is a migration shim for a historical bug — checked: `SaveCustomGameAsync` always writes `IsCustom = true` today, so the shim only matters for pre-fix data still sitting in a user's local storage. Zero-cost to keep, so left in place.
- [x] `GameRules`' doc comment claimed "règles de fin de partie" (end-game rules) but only 2 of its 7 properties (`TargetScore`/`MaxRounds`) actually are — the rest are setup constraints, turn order, and a UI preference (`ScoreIncrement`). Reworded to describe its actual scope instead of renaming/restructuring (no active pain from the shape itself, just a stale comment).

### Known bug found, not yet fixed
- [ ] **Editing a saved custom game creates a duplicate instead of updating it.** `CustomGame/Setup.razor.CreateGame()` always calls `new GameTemplate(Guid.NewGuid(), ...)`, even when reached via the edit flow (`Settings.razor` → "✎" → `custom-game?baseGameId=...`). `LocalStorageGameCatalogueService.SaveCustomGameAsync`'s upsert matches by `Id`, so a fresh Id never matches the original entry — every edit (with "Sauvegarder ce modèle" checked) adds a new catalogue entry and leaves the old one orphaned. There's also no duplicate-name check on creation. Found while investigating a report of duplicate-named custom games; not yet fixed.

### Test coverage
- [x] `MovePlayerUpAsync`/`MovePlayerDownAsync`/`SetFirstPlayerManuallyAsync`/`FinishSessionAsync`/`AbandonSessionAsync` had no dedicated unit test. Added (including no-op/idempotency cases for Finish/Abandon and boundary cases for Move).

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