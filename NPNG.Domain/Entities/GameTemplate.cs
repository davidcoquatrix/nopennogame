using NPNG.Domain.Enums;

namespace NPNG.Domain.Entities;

/// <summary>
/// Contraintes de composition des équipes pour un jeu qui se joue en équipes.
/// Sa seule présence sur GameRules.Teams (au lieu d'un booléen séparé) fait foi de l'activation du mode équipe.
/// </summary>
public record TeamRules(int? TeamSize = null, bool RequireEqualTeams = false)
{
    /// <summary>
    /// Une taille fixe impose de facto des équipes égales : pas de sens à figer TeamSize
    /// sans exiger que chaque équipe s'y conforme.
    /// </summary>
    public bool RequiresEqualSizes => TeamSize is not null || RequireEqualTeams;
}

/// <summary>
/// Configuration d'une partie : conditions de fin (TargetScore/MaxRounds), contraintes de
/// setup (Min/MaxPlayers, Teams), mécanique de premier joueur et préférence de saisie du score.
/// </summary>
public record GameRules(
    int? TargetScore = null,
    int? MaxRounds = null,
    FirstPlayerMechanic FirstPlayerMechanic = FirstPlayerMechanic.Sequential,
    int? MaxPlayers = null,
    int? MinPlayers = null,
    TeamRules? Teams = null,
    ScoreIncrementMode ScoreIncrement = ScoreIncrementMode.OneAndTen);

/// <summary>
/// Représente le modèle de base d'un jeu (ses règles, son type de score).
/// </summary>
public record GameTemplate(
    Guid Id,
    string Name,
    ScoreType ScoreType,
    GameRules? Rules = null);
