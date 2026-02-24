using System;
using LocalSaveSystem;

namespace Code.Game.Saves.Profile
{
[SaveModel]
[SaveVersion(1)]
public struct PlayerProfilesListSave
{
    public GameProfileSave[] GameProfileSaves { get; set; }

    public PlayerProfilesListSave(GameProfileSave[] gameProfileSaves)
    {
        GameProfileSaves = gameProfileSaves ?? Array.Empty<GameProfileSave>();
    }
}
}