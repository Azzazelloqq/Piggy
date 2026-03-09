using Code.Game.Saves.Profile;

namespace Code.Game.MainMenu.Window
{
public readonly struct GameProfilesSlotData
{
    public GameProfilesSlotData(int index, bool hasSave, GameProfileSave save)
    {
        Index = index;
        HasSave = hasSave;
        Save = save;
    }

    public int Index { get; }
    public bool HasSave { get; }
    public GameProfileSave Save { get; }
}
}
