using System;
using Code.Game.Saves.Profile;
using LocalSaveSystem;

namespace Code.Game.Saves
{
    public static class GameSaveKeys
    {
        public static readonly SaveKey<PlayerProfilesListSave> GameProfiles = new("game_profiles", CreateDefaultProfile);

        public static readonly ISaveKey[] All =
        {
            GameProfiles
        };

        private static PlayerProfilesListSave CreateDefaultProfile()
        {
            return new PlayerProfilesListSave(Array.Empty<GameProfileSave>());
        }
    }
}
