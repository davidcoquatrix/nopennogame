namespace NPNG.Domain.Enums;

/// <summary>
/// Comment la feuille de score d'un jeu structuré doit s'afficher.
/// </summary>
public enum StructuredLayoutStyle
{
    /// <summary>
    /// Une carte par catégorie, une ligne par joueur à l'intérieur (ex: Akropolis — peu de
    /// catégories, chacune avec ses propres champs de saisie).
    /// </summary>
    Cards,

    /// <summary>
    /// Un tableau façon feuille officielle : catégories en lignes, joueurs en colonnes (ex: Yams —
    /// beaucoup de catégories à valeur unique, plus lisible en grille qu'en cartes empilées).
    /// </summary>
    Table
}
