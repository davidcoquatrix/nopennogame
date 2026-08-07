namespace NPNG.Domain.Enums;

/// <summary>
/// Définit les boutons d'incrément rapide proposés lors de la saisie du score.
/// </summary>
public enum ScoreIncrementMode
{
    OneAndTen,      // ±1 / ±10
    FiveAndFifty,   // ±5 / ±50
    Free            // Pas de boutons rapides, saisie clavier uniquement
}
