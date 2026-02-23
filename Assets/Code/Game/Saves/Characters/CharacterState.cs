using LocalSaveSystem;

namespace Code.Game.Saves.Characters
{
[SaveModel]
[SaveVersion(1)]
public struct CharacterState
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int CurrentMana { get; set; }
}
}