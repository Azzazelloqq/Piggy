using LocalSaveSystem;

namespace Code.Game.Saves.Characters
{
[SaveModel]
[SaveVersion(1)]
public struct CharacterView 
{
    public string AvatarId { get; set; }
}
}