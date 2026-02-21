using System;

namespace Code.Game.Saves.Characters
{
public readonly struct CharacterTags
{
    public string[] TraitsIds { get; }

    public CharacterTags(string[] traitsIds)
    {
        TraitsIds = traitsIds ?? Array.Empty<string>();
    }
}
}