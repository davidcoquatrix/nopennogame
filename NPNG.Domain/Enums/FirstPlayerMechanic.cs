namespace NPNG.Domain.Enums;

/// <summary>
/// Définit comment est choisi le premier joueur pour la prochaine manche.
/// </summary>
public enum FirstPlayerMechanic
{
    Sequential, // Le badge de premier joueur passe au suivant à chaque manche
    Winner,     // Le joueur actuellement en tête du classement commence
    Loser,      // Le joueur actuellement dernier du classement commence
    None        // Le premier joueur ne change jamais automatiquement
}
