# NPNG — NoPenNoGame 🎲

Une Progressive Web App (PWA) conçue avec **Blazor WebAssembly** pour gérer les scores de vos jeux de société. Plus besoin de chercher un stylo qui fonctionne ou un bout de papier brouillon : NPNG est fait pour être plus rapide, plus fiable et plus fun que la méthode traditionnelle.

## 🎯 Philosophie du projet

Ce projet suit une architecture stricte ("Flat Clean Architecture") et un manifeste technique très précis :
- **Offline-first :** La base de données, c'est votre téléphone (`LocalStorage`). L'application doit marcher au fond d'une cave ou au milieu de la forêt.
- **Mobile-first :** L'UI/UX est pensée comme une application native iOS/Android, jouable à une main, avec des gestes rapides.
- **Zero "Backend" :** Pas d'API, pas de base de données distante, pas de temps de latence, pas de coûts de serveurs (pour le MVP).
- **KISS (Keep It Simple, Stupid) :** .NET 10, C# 13, et le moins de dépendances NuGet possible.

## 🚀 Fonctionnalités (MVP - Phase 1)

- Sélection d'un jeu (Skyjo, Uno, Rami...) ou création d'un jeu aux règles personnalisées (Le + haut gagne, Le + bas gagne).
- Gestion des joueurs avec avatars (emojis) et couleurs.
- Interface de saisie ultra-rapide (+1, -1, +10, -10).
- Calcul automatique des totaux et mise à jour du classement en direct.
- **Time Travel :** Navigation dans l'historique des tours pour corriger une erreur de saisie sans casser la partie.

## 🛠️ Stack Technique

- **Framework:** .NET 10 (Blazor WebAssembly)
- **Architecture:** Flat Clean Architecture (`Domain` <- `Application` <- `Infrastructure` <- `UI`)
- **UI/CSS:** CSS natif (Custom Properties, Flexbox/Grid), sans framework externe pour des performances maximales.

## 💻 Comment lancer le projet en local

1. Assurez-vous d'avoir le **SDK .NET 10** d'installé.
2. Clonez le dépôt.
3. Ouvrez un terminal dans le dossier racine du projet.
4. Lancez l'application Blazor :
   ```bash
   dotnet run --project NPNG.UI/NPNG.UI.csproj
   ```
5. Ouvrez votre navigateur sur `http://localhost:5152` (ou le port indiqué dans la console).

---
*Ce projet est développé de manière assistée par l'IA (Gemini CLI) selon le paradigme de "Tech Lead AI & Agile Scribe".*
