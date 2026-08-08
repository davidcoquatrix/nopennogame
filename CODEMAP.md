# NPNG — Carte du code

Index de "où chercher quoi" dans le code, en complément des fichiers de doctrine (`AGENT.md`,
`ARCHITECTURE.md`, `STANDARDS.md`, `BACKLOG.md`). Ceux-ci décrivent le *pourquoi* et les règles ;
ce fichier pointe vers les fichiers concrets pour ne pas avoir à ré-explorer le repo à chaque session.

À tenir à jour : quand un nouveau `ScoreType`, une nouvelle grande mécanique (ex: équipes, first-player)
ou un nouveau service transverse apparaît, ajouter une entrée ici plutôt que de laisser la seule trace
dans `BACKLOG.md`.

## Les 4 `ScoreType` et leur implémentation

- **`Cumulative` / `CumulativeLower`** : pas de fichiers dédiés. Juste `GameRules` (TargetScore/MaxRounds)
  + le switch dans `NPNG.Domain/Services/ScoreCalculator.cs`. Saisie via
  `NPNG.UI/Components/ScoreInput.razor`, manche par manche, dans `Session/Active.razor`.

- **`Structured`** (Akropolis, Yams — feuille de score unique en fin de partie, pas de notion de manche) :
  - Entities : `CategoryDefinition.cs`, `CategoryValue.cs`, `SectionBonus.cs`, `StructuredScoreDetail.cs`
  - Enums : `CategoryInputShape.cs`, `StructuredGameKind.cs`, `StructuredLayoutStyle.cs`
  - Services : `StructuredScoringDefinition.cs` (base abstraite), `StructuredScoringCatalogue.cs`
    (registre `StructuredGameKind → definition`), `AkropolisScoringDefinition.cs`, `YamsScoringDefinition.cs`
  - UI : `NPNG.UI/Components/StructuredScoreSheet.razor` + `StructuredCategoryCell.razor`
  - Flux : un seul round fixe (`GameStateService.StructuredScoringRound = 1`) ; la soumission
    (`SubmitStructuredScoreAndFinishAsync`) termine la partie immédiatement, pas de boucle de manches.

- **`PhaseProgression`** (Phase 10 — score de manche cumulatif + progression de phase par joueur) :
  - Entities : `PhaseScoreDetail.cs` (sur `ScoreEntry.PhaseDetail`), `Phase10Phases.cs` (les 10
    descriptions, données propres au jeu, hors logique générique)
  - Services : `PhaseProgressCalculator.cs` (dérive la progression depuis l'historique de `ScoreEntry`,
    ne connaît pas `Phase10Phases`)
  - `GameRules.WinningPhase` pilote la fin de partie (`GameStateService.IsGameFinished`)
  - UI : `NPNG.UI/Components/PhaseScoreInput.razor`

Un `ScoreType` n'a besoin d'un "kind + catalogue" (comme `Structured`) que si **au moins deux jeux**
partagent le même mécanisme avec des données différentes. En dessous de ça, une classe statique dédiée
(comme `Phase10Phases`) suffit — voir la note YAGNI de `Structured` (né d'un refactor, pas construit
en avance).

## Cycle de vie d'une session — `NPNG.Application/State/GameStateService.cs`

Seule dépendance : `ISessionRepository`. État en mémoire (`CurrentSession`), notifié via `OnStateChanged`,
sauvegardé sur chaque mutation (`SaveStateAsync`, cf. `ARCHITECTURE.md` §3).

- `RecordScoreAsync` / `RecordTeamScoreAsync` : upsert d'un `ScoreEntry` par `(playerId, round)`.
- `AdvanceToNextRoundAsync` : incrémente `CurrentRound` + avance le premier joueur, ou termine la
  partie si `IsGameFinished`.
- `IsGameFinished` (privé) : `MaxRounds` / `TargetScore` / `WinningPhase`, court-circuité par
  `Session.RulesOverridden`.
- `ResumeSessionAsync` : réouvre une partie `Finished`. Cas spécial pour `Structured` (pas d'avancement
  de round, puisqu'il n'y en a qu'un). `PhaseProgression` n'a pas ce cas spécial : elle avance en
  manches normales comme `Cumulative`.
- `SubmitStructuredScoreAndFinishAsync` : flux dédié `Structured`, voir plus haut.

## Catalogue de jeux

- `NPNG.Application/Models/GameCatalogueItem.cs` — DTO catalogue, `ToGameTemplate()` construit le
  `GameTemplate` de la session.
- `NPNG.Infrastructure/Services/LocalStorageGameCatalogueService.cs` — `_baseGames` (jeux intégrés,
  hardcodés) + jeux personnalisés (LocalStorage, clé `npng_custom_games`).
- `NPNG.UI/Pages/CustomGame/Setup.razor` — formulaire de jeu personnalisé. Ne peut produire que
  `Cumulative`/`CumulativeLower` (pas de `Structured`/`PhaseProgression` : ces types restent réservés
  aux jeux intégrés, câblés à la main).

## Écrans de saisie de score

- `NPNG.UI/Pages/Session/Active.razor` — saisie en cours, branche sur `Template.ScoreType`.
- `NPNG.UI/Pages/Session/History/Index.razor` (route `/session/history`) — **Time Travel**, édition
  d'une manche passée d'une partie active/terminée. Branche aussi par `ScoreType`. À ne pas confondre
  avec l'écran suivant.
- `NPNG.UI/Pages/History/Rounds.razor` (route `/history/{SessionId}/rounds`) — récap **lecture seule**
  d'une partie terminée, dans l'historique des sessions.
- `NPNG.UI/Components/GameRecapPanel.razor` — bandeau vainqueur + classement de fin de partie, générique
  via `ScoreCalculator` (fonctionne pour tout `ScoreType` une fois `CalculateLeaderboard` étendu).

## Tests

- `NPNG.Tests/Domain/Services/*Tests.cs` — logique pure, AAA, nommage `MethodName_State_Expected`.
- `NPNG.Tests/Application/State/GameStateServiceTests.cs` — `GameStateService` avec
  `Mock<ISessionRepository>` (Moq), pas d'autre dépendance mockée.
