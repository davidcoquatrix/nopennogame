# NPNG — Product Backlog
## Idea Board & User Stories

This document is maintained by the **Agile Scribe AI**. It translates raw ideas into structured, prioritized features.

---

## 🎯 Phase 1: MVP (The Core Offline Experience)
*Goal: Provide a fully functional, offline-first scoring experience that is strictly better than pen and paper.*

### Epic: Session Setup
- [ ] **Story 1.1: Start a Game**
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
- [ ] **Story 1.10: Favorite Players**
  - *As a Scorekeeper, I want to be able to save my friends as "Favorite Players" (Name + Emoji) so that I can quickly add them to a new session with a single tap, without re-typing their names.*
  - *These favorites should be displayed below the "Add player" form or "Start Game" button during session setup.*

### Epic: Score Entry
- [x] **Story 1.4: Quick Score Input**
  - [x] UI/UX Integration (Leaderboard & Quick increment buttons)
  - [x] Domain / State Management integration
- [ ] **Story 1.5: Correct Mistakes (Time Travel)**
  - [x] UI/UX Integration (Round table, Edit mode)
  - [ ] Domain / State Management integration

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
- **Story 2.5: Dynamic First Player Selection (Score-based)**
  - *As a Scorekeeper, I want to configure the first player selection rule (e.g., highest/lowest score of the previous round) so that the app automatically assigns the first player badge each round.*
  - *Tie-breaker rules: 1. Compare total global score. 2. If still tied, use the current table order.*

### Epic: Game Rules & Templates
- **Story 2.1: View Rules**
  - *As a Scorekeeper or Player, I want to read a quick summary of the rules for the selected game directly in the app to resolve disputes quickly.*
- **Story 2.2: Advanced Custom Rules**
  - *As a Scorekeeper, I want to set score limits (e.g., "Game ends at 500 points") or round limits when creating a custom game.*
- **Story 2.6: Save and Manage Custom Games**
  - *As a Scorekeeper, I want to be able to save a custom game configuration (name, scoring type, rules) so that I can reuse it later without having to recreate it.*
  - *As a Scorekeeper, I want to be able to delete a custom game that I previously saved, so that I can keep my game catalog clean.*

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