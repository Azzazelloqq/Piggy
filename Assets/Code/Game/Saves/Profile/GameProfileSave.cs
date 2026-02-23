using System;
using Code.Game.Saves.Characters;
using LocalSaveSystem;

namespace Code.Game.Saves.Profile
{
[SaveModel]
[SaveVersion(1)]
public struct GameProfileSave
{
    public PlayerCharacter[] PlayerCharacters { get; set; }
    public int ActiveCharacterIndex { get; set; }

    public GameProfileSave(PlayerCharacter[] playerCharacters, int activeCharacterIndex)
    {
        PlayerCharacters = playerCharacters ?? Array.Empty<PlayerCharacter>();
        ActiveCharacterIndex = activeCharacterIndex;
    }
}
}