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
    /// Score de manche cumulatif (plus bas gagne) doublé d'une progression de phase par joueur
    /// (ex: Phase 10) : la partie se termine quand un joueur valide la dernière phase, qui prime
    /// sur le score — celui-ci ne sert qu'à départager les joueurs à égalité de phase.
    /// </summary>
    PhaseProgression,

    /// <summary>
    /// Score par catégorie avec règles personnalisées (ex: Akropolis).
    /// </summary>
    Structured
}
