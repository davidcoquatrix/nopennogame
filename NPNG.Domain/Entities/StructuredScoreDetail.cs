using System.Collections.Immutable;

namespace NPNG.Domain.Entities;

/// <summary>
/// Détail du score de fin de partie pour un jeu structuré (ex: Akropolis, Yams), indexé par
/// <see cref="CategoryDefinition.Key"/>. Persisté (pas seulement le total) afin de rester éditable
/// via Time Travel.
/// </summary>
/// <remarks>
/// Stocké en <see cref="ImmutableArray{T}"/> de paires plutôt qu'en <c>ImmutableDictionary</c> :
/// System.Text.Json construit le convertisseur d'<c>ImmutableDictionary</c> via une méthode
/// (<c>CreateRange</c>) résolue par réflexion, que le trimming du publish Blazor WASM élague — la
/// sérialisation lève alors un <c>NotSupportedException</c> au runtime (marche en debug, casse en
/// publié). <c>ImmutableArray</c> n'a pas ce problème, et c'est déjà ce que le reste du Domain utilise
/// (ex: <see cref="Session.Scores"/>).
/// </remarks>
public record StructuredScoreDetail(ImmutableArray<KeyValuePair<string, CategoryValue>> Categories)
{
    public static StructuredScoreDetail Empty { get; } = new(ImmutableArray<KeyValuePair<string, CategoryValue>>.Empty);

    public CategoryValue GetValue(string categoryKey) =>
        Categories.FirstOrDefault(kv => kv.Key == categoryKey).Value;

    public StructuredScoreDetail WithValue(string categoryKey, CategoryValue value)
    {
        var withoutExisting = Categories.RemoveAll(kv => kv.Key == categoryKey);
        return this with { Categories = withoutExisting.Add(new KeyValuePair<string, CategoryValue>(categoryKey, value)) };
    }
}
