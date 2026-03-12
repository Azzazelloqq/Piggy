using System;
using Code.Game.Saves.Characters;
using Code.Game.Saves.World;
using LocalSaveSystem;

namespace Code.Game.Saves.Profile
{
[SaveModel]
[SaveVersion(1)]
public struct GameProfileSave
{
    public PlayerCharacter[] PlayerCharacters { get; set; }
    public int ActiveCharacterIndex { get; set; }
    public WorldStateSave WorldState { get; set; }
    public string LastLocationName { get; set; }
    public string ExitTimeText { get; set; }
    public string InGameTimeText { get; set; }

    public GameProfileSave(
        PlayerCharacter[] playerCharacters,
        int activeCharacterIndex,
        WorldStateSave worldState = default)
    {
        PlayerCharacters = playerCharacters ?? Array.Empty<PlayerCharacter>();
        ActiveCharacterIndex = activeCharacterIndex;
        WorldState = worldState;
        LastLocationName = string.Empty;
        ExitTimeText = string.Empty;
        InGameTimeText = string.Empty;
    }
}
}