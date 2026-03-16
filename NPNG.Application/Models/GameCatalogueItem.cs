using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

namespace NPNG.Application.Models;

/// <summary>
/// DTO représentant un jeu dans le catalogue, incluant les métadonnées UI (Emoji, Couleur, etc.)
/// ainsi que les règles du domaine (GameTemplate).
/// </summary>
public record GameCatalogueItem(
    Guid Id, 
    string Emoji, 
    string Name, 
    string Description, 
    string Badge, 
    string Color, 
    ScoreType ScoreType, 
    GameRules? Rules = null, 
    bool IsCustom = false)
{
    public GameTemplate ToGameTemplate()
    {
        return new GameTemplate(Id, Name, ScoreType, Description, Rules);
    }
}
