using System.Text.Json;
using NPNG.Domain.Enums;

namespace NPNG.Domain.Entities;

/// <summary>
/// Représente le modèle de base d'un jeu (ses règles, son type de score).
/// </summary>
public record GameTemplate(
    Guid Id,
    string Name,
    ScoreType ScoreType,
    string? Description = null,
    JsonElement? Rules = null);
