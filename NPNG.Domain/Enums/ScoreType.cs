namespace NPNG.Domain.Enums;

/// <summary>
/// Définit comment les scores sont calculés pour déterminer le vainqueur.
/// </summary>
public enum ScoreType
{
    /// <summary>
    /// Total = somme des manches. Le score le plus élevé gagne (ex: Rami, Uno).
    /// </summary>
    Cumulative,
    
    /// <summary>
    /// Total = somme des manches. Le score le plus bas gagne (ex: Skyjo).
    /// </summary>
    CumulativeLower,
    
    /// <summary>
    /// Score par catégorie avec règles personnalisées (ex: Accropolis).
    /// </summary>
    Structured
}
