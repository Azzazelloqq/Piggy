using System;
using Code.Game.Saves.Characters;

namespace Code.Game.Saves.Profile
{
public readonly struct GameProfileSave
{
    public PlayerCharacter[] PlayerCharacters { get; }
    public int ActiveCharacterIndex { get; }

    public GameProfileSave(PlayerCharacter[] playerCharacters, int activeCharacterIndex)
    {
        PlayerCharacters = playerCharacters ?? Array.Empty<PlayerCharacter>();
        ActiveCharacterIndex = activeCharacterIndex;
    }
}
}