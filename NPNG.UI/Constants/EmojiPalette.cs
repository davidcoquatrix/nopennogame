namespace NPNG.UI.Constants;

public record EmojiCategory(string Label, IReadOnlyList<string> Emojis);

/// <summary>
/// Choix d'emojis proposés dans les pickers (joueurs vs jeux), regroupés par catégorie
/// pour rester parcourables malgré un choix plus large que l'ancienne liste unique.
/// Le premier tag de chaque set reprend l'ancienne liste "à plat" pour ne pas changer
/// les emojis déjà utilisés par des joueurs/jeux existants.
/// </summary>
public static class EmojiPalette
{
    public static readonly IReadOnlyList<EmojiCategory> PlayerCategories = new List<EmojiCategory>
    {
        new("Classiques", new[] { "🎯", "🦊", "🐉", "🌈", "⚡", "🍀", "🎸", "🎮", "🌟", "🔥", "💜", "🦄", "🚀", "🍕" }),
        new("Animaux", new[] { "🐶", "🐱", "🐼", "🦁", "🐸", "🐢", "🦉", "🐷", "🐵", "🦖" }),
        new("Visages", new[] { "😎", "😂", "🤓", "🥳", "🧐", "😴", "🤠", "👻", "👽", "🤖" }),
        new("Nature", new[] { "☀️", "🌙", "❄️", "🌊", "🍄", "🌵", "🌸", "⭐", "💎", "🎈" }),
    };

    public static readonly IReadOnlyList<EmojiCategory> GameCategories = new List<EmojiCategory>
    {
        new("Classiques", new[] { "⚙️", "🎯", "🦊", "🐉", "🌈", "⚡", "🍀", "🎸", "🎮", "🌟", "🔥", "💜", "🦄", "🚀", "🍕" }),
        new("Jeux & hasard", new[] { "🎲", "🃏", "🀄", "♟️", "🧩", "🕹️", "🎳", "🎰" }),
        new("Compétition", new[] { "🏆", "🥇", "🥈", "🥉", "🏅", "⚔️", "🎖️", "👑" }),
        new("Ambiance", new[] { "🤝", "🍻", "🎉", "🍿", "🎊", "🥂", "🎵", "🍷", "🧃" }),
    };
}
