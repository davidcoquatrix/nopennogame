# NPNG - Coding Standards & Quality Guidelines

Ce document définit le niveau d'exigence et les conventions de code pour le projet NoPenNoGame (NPNG). L'objectif est de maintenir un code de qualité professionnelle, lisible, testable et maintenable, en respectant les principes de l'artisanat logiciel (Software Craftsmanship).

## 1. Principes Fondamentaux (L'état d'esprit)

*   **KISS (Keep It Simple, Stupid) :** La simplicité est la priorité. Ne sur-complexifiez pas les solutions.
*   **YAGNI (You Aren't Gonna Need It) :** Implémentez uniquement ce qui est requis par le MVP actuel défini dans `BACKLOG.md`. Pas de code spéculatif ou d'abstractions "au cas où".
*   **Boy Scout Rule :** Laissez toujours le code dans un état plus propre que celui dans lequel vous l'avez trouvé. Le refactoring continu (renommage, extraction de méthodes) fait partie du développement quotidien.
*   **Séparation des Préoccupations (SoC) :** L'UI (composants Blazor) ne doit pas contenir de logique métier. Le Domaine (règles de calcul des scores, modèles) doit être pur et indépendant de l'UI.

## 2. Modern C# 13 & .NET 9 (Clean Code)

*   **Immuabilité par défaut :** Utilisez massivement les `record` pour les modèles du Domaine, les DTOs et l'état. Privilégiez les structures de données immuables.
*   **Constructeurs Primaires (Primary Constructors) :** Utilisez-les pour réduire le code *boilerplate*, particulièrement pour l'injection de dépendances dans les services.
*   **Nullable Reference Types (NRT) :** La fonctionnalité est activée de manière stricte (`<Nullable>enable</Nullable>`). Aucun avertissement lié à la nullité ne doit être ignoré.
*   **Pattern Matching :** Utilisez les expressions `switch` et le pattern matching fonctionnel plutôt que les longues chaînes de `if/else` lorsque cela améliore la lisibilité.

## 3. Nommage et Style

*   **Intention-Revealing Names :** Les noms (variables, méthodes, classes) doivent révéler leur intention. Le nom doit expliquer *pourquoi* l'élément existe, ce qu'il fait et comment il est utilisé, sans nécessiter de commentaire.
*   **Conventions standards C# :**
    *   `PascalCase` pour les Classes, Records, Interfaces (toujours préfixées par `I`), Méthodes, et Propriétés publiques.
    *   `camelCase` pour les paramètres et les variables locales.
    *   `_camelCase` pour les champs privés (si les constructeurs primaires ne sont pas utilisables).
*   **Commentaires :** Le code doit s'expliquer de lui-même. Réservez les commentaires (XML Docs) aux interfaces publiques complexes ou pour expliquer le *pourquoi* d'une décision technique non triviale. Pas de commentaires redondants du type `// Calcule le score` au-dessus d'une méthode `CalculateScore()`.

## 4. Architecture et SOLID (Flat Clean Architecture)

*   **Single Responsibility Principle (SRP) :** Une classe ou un composant ne doit avoir qu'une seule raison de changer. Divisez les composants trop volumineux.
*   **Dependency Inversion Principle (DIP) :** Dépendez des abstractions (Interfaces), pas des implémentations.
*   **Smart vs Dumb Components (Blazor) :** 
    *   **Smart (Containers) :** S'abonnent à l'état, injectent les services, et gèrent la logique d'orchestration.
    *   **Dumb (Présentationnels) :** Ne reçoivent que des `[Parameter]` et émettent des événements via `[EventCallback]`. Ils sont purs et faciles à tester.

## 5. Tolérance Zéro (Code Smells)

*   Pas de *Magic Strings* ni de *Magic Numbers* (utilisez des `const` ou `enum`).
*   Pas de méthodes de plus de 15-20 lignes (extrayez des sous-méthodes privées nommées explicitement).
*   Pas de code mort ou commenté dans le dépôt principal.

## 6. Stratégie de Test et Couverture (Testing Business Logic)

*   **Tester le Métier, pas le Framework :** Les Tests Unitaires (TU) doivent se concentrer impérativement sur la logique métier complexe (le Domaine : calculs de scores, validations d'état de partie, règles spécifiques aux jeux). Ne testez pas ce que le framework .NET ou Blazor fait déjà (ex: ne pas tester qu'une propriété auto-implémentée affecte bien sa valeur).
*   **Qualité > Quantité (Vanity Metrics) :** Un taux de couverture de 100% n'est pas un but en soi s'il est atteint avec des tests fragiles ou inutiles. L'objectif est une couverture de **100% sur le cœur de métier (Domain)**.
*   **Pattern AAA (Arrange, Act, Assert) :** Structurez clairement chaque test en trois sections distinctes.
*   **Nommage des Tests :** Utilisez une convention claire comme `MethodName_StateUnderTest_ExpectedBehavior` (ex: `CalculateTotal_WhenModifiersApplied_ReturnsCorrectScore`).
*   **Lisibilité des Assertions :** Les assertions doivent lire comme des phrases (l'utilisation de bibliothèques comme *FluentAssertions* ou *Shouldly* est fortement encouragée pour la clarté si nécessaire, sinon utilisez les assertions standard de manière explicite).
