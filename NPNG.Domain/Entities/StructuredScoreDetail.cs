using System.Collections.Immutable;

namespace NPNG.Domain.Entities;

/// <summary>
/// Détail du score de fin de partie pour un jeu structuré (ex: Akropolis, Yams), indexé par
/// <see cref="CategoryDefinition.Key"/>. Persisté (pas seulement le total) afin de rester éditable
/// via Time Travel.
/// </summary>
public record StructuredScoreDetail(ImmutableDictionary<string, CategoryValue> Categories)
{
    public static StructuredScoreDetail Empty { get; } = new(ImmutableDictionary<string, CategoryValue>.Empty);

    public CategoryValue GetValue(string categoryKey) => Categories.GetValueOrDefault(categoryKey);
}
