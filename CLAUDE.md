# NPNG - Claude Code Project Context

Ce fichier est automatiquement lu par Claude Code au démarrage de chaque session dans ce dépôt.

## Contexte du Projet
- **Projet :** NPNG (NoPenNoGame), une PWA Blazor WebAssembly pour gérer les scores de jeux de société.
- **Phase actuelle :** Phase 1 (MVP) terminée — Strictement "Offline-First". Pas de backend, pas de base de données distante.
- **Dev solo**, assisté par IA. Le Product Owner reste seul décisionnaire sur la roadmap et les choix d'architecture.

## Documentation Obligatoire (Le "Cahier des Charges")
Avant de générer du code, de proposer une architecture ou de valider une tâche, se référer à :

1. `AGENT.md` : La vision produit globale, le modèle de données (Domain), le Design System (CSS) et la roadmap détaillée (avec les bugs connus).
2. `ARCHITECTURE.md` : Les règles de la "Flat Clean Architecture" et la stratégie de State Management (très important pour Blazor).
3. `STANDARDS.md` : Les règles de qualité de code (SRP, taille des méthodes, pas de magic strings, stratégie de test).
4. `PERSONAS.md` : Le rôle attendu de l'agent IA et les besoins vitaux des utilisateurs finaux (rapidité et fiabilité pour le "Scorekeeper").
5. `BACKLOG.md` : La liste priorisée des fonctionnalités et les bugs connus. Ne pas proposer de fonctionnalités hors périmètre sans l'accord du Product Owner.
6. `CODEMAP.md` : Index de "où chercher quoi" dans le code (implémentation de chaque `ScoreType`, cycle de vie de `GameStateService`, écrans de saisie de score, conventions de test). À consulter avant d'explorer le repo pour une tâche touchant le Domain/l'Application/le scoring — évite de redécouvrir par exploration ce qui est déjà cartographié. À tenir à jour quand une nouvelle mécanique transverse apparaît.

## Règles de Comportement
- **Pas d'over-engineering :** projet "solo dev". Garder l'architecture plate et simple (voir `ARCHITECTURE.md`).
- **Séparation des préoccupations :** ne jamais mélanger la logique métier pure (calcul des scores, règles de jeu) et le code UI Blazor. Le Domain doit rester pur et testable.
- **Dépendances :** utiliser au maximum les fonctionnalités natives de .NET 10 et Blazor. Éviter d'ajouter des packages NuGet externes sauf nécessité absolue.
- **Qualité du code :** C# 13 moderne (records, primary constructors), méthodes courtes et à responsabilité unique, pas de magic strings/numbers (voir `STANDARDS.md`).
- **Tests :** viser 100% de couverture sur le Domain (logique métier pure), pas sur le framework. Pattern AAA.

## État connu à date (voir `AGENT.md` pour le détail)
- MVP Phase 1 fonctionnellement complet (setup, saisie de score, Time Travel, favoris, mécaniques de premier joueur, catalogue de jeux custom).
- Phase 2/3 (historique des parties finies, règles avancées, partage) pas encore commencées.
