using LocalSaveSystem;

namespace Code.Game.Saves.Characters
{
[SaveModel]
[SaveVersion(1)]
public struct CharacterTags
{
    public string[] TraitsIds { get; set; }
}
}