namespace NPNG.Domain.Enums;

/// <summary>
/// Définit comment est choisi le premier joueur pour la prochaine manche.
/// </summary>
public enum FirstPlayerMechanic
{
    Sequential,                 // Le badge de premier joueur passe au suivant à chaque manche
    Winner,                     // Le joueur actuellement en tête du classement global commence
    Loser,                      // Le joueur actuellement dernier du classement global commence
    HighestInPreviousRound,     // Le joueur ayant fait le plus haut score à la manche précédente commence
    LowestInPreviousRound,      // Le joueur ayant fait le plus bas score à la manche précédente commence
    None                        // Le premier joueur ne change jamais automatiquement
}
