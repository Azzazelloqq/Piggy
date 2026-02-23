using System;
using Code.Game.Saves.Characters;
using Code.Game.Saves.Profile;
using LocalSaveSystem;

namespace Code.Game.Saves
{
    public static class GameSaveKeys
    {
        public static readonly SaveKey<GameProfileSave> GameProfile = new("game_profile", CreateDefaultProfile);

        public static readonly ISaveKey[] All =
        {
            GameProfile
        };

        private static GameProfileSave CreateDefaultProfile()
        {
            return new GameProfileSave(Array.Empty<PlayerCharacter>(), 0);
        }
    }
}
