using System;
using System.Collections.Generic;
using Code.Config.Pages.CharactersPage;

namespace Code.Game.MainMenu.Window
{
public readonly struct CharacterCreationModelConfig
{
    public IReadOnlyList<CharacterStatType> StatOrder { get; }

    public IReadOnlyList<string> TraitIds { get; }

    public IReadOnlyList<string> AvatarIds { get; }

    public int MaxPoints { get; }
    public int MaxTraits { get; }
    public int DefaultStatValue { get; }
    
    public CharacterCreationModelConfig(
        IReadOnlyList<CharacterStatType> statOrder,
        IReadOnlyList<string> traitIds,
        IReadOnlyList<string> avatarIds,
        int maxPoints,
        int maxTraits,
        int defaultStatValue)
    {
        if (statOrder == null)
        {
            throw new ArgumentNullException(nameof(statOrder));
        }

        if (statOrder.Count == 0)
        {
            throw new ArgumentException("Stat order is empty.", nameof(statOrder));
        }

        if (traitIds == null)
        {
            throw new ArgumentNullException(nameof(traitIds));
        }

        if (avatarIds == null)
        {
            throw new ArgumentNullException(nameof(avatarIds));
        }

        if (defaultStatValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultStatValue), "Default stat value cannot be negative.");
        }

        StatOrder = Copy(statOrder);
        TraitIds = Copy(traitIds);
        AvatarIds = Copy(avatarIds);
        MaxPoints = maxPoints;
        MaxTraits = maxTraits;
        DefaultStatValue = defaultStatValue;
    }

    private static IReadOnlyList<T> Copy<T>(IReadOnlyList<T> source)
    {
        var copy = new T[source.Count];
        for (var i = 0; i < source.Count; i++)
        {
            copy[i] = source[i];
        }

        return Array.AsReadOnly(copy);
    }
}
}
