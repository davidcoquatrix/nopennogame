using NPNG.Domain.Enums;

namespace NPNG.Domain.Entities;

/// <summary>
/// Définition des règles de fin de partie.
/// </summary>
public record GameRules(int? TargetScore = null, int? MaxRounds = null);

/// <summary>
/// Représente le modèle de base d'un jeu (ses règles, son type de score).
/// </summary>
public record GameTemplate(
    Guid Id,
    string Name,
    ScoreType ScoreType,
    string? Description = null,
    GameRules? Rules = null);
