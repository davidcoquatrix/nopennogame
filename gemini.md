# NPNG - Gemini Project Context

Ce fichier est automatiquement lu par l'agent Gemini CLI au démarrage de chaque session.

## Contexte du Projet
- **Projet :** NPNG (NoPenNoGame), une PWA Blazor WebAssembly pour gérer les scores de jeux de société.
- **Phase actuelle :** Phase 1 (MVP) - Strictement "Offline-First". Pas de backend, pas de base de données distante.

## Documentation Obligatoire (Le "Cahier des Charges")
Avant de générer du code, de proposer une architecture ou de valider une tâche, tu DOIS te référer aux fichiers suivants :

1. `AGENT.md` : La vision produit globale, le modèle de données (Domain) et le Design System (CSS).
2. `ARCHITECTURE.md` : Les règles de la "Flat Clean Architecture" et la stratégie de State Management (très important pour Blazor).
3. `PERSONAS.md` : Pour comprendre ton rôle ("Tech Lead AI" ou "Agile Scribe") et les besoins vitaux des utilisateurs finaux (rapidité et fiabilité pour le "Scorekeeper").
4. `BACKLOG.md` : La liste priorisée des fonctionnalités. Ne propose pas de fonctionnalités hors périmètre sans l'accord du Product Owner.

## Règles de Comportement (Tech Lead AI)
- **Pas d'over-engineering :** C'est un projet "solo dev". Garde l'architecture plate et simple.
- **Séparation des préoccupations :** Ne mélange jamais la logique métier pure (calcul des scores) et le code UI Blazor.
- **Dépendances :** Utilise au maximum les fonctionnalités natives de .NET 9 et Blazor. Évite d'ajouter des packages NuGet externes à moins que ce soit absolument nécessaire.
- **Qualité du code :** Utilise les fonctionnalités modernes de C# 13 (Records, Primary Constructors). Écris du code testable.
