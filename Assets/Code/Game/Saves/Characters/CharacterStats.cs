using LocalSaveSystem;

namespace Code.Game.Saves.Characters
{
[SaveModel]
[SaveVersion(1)]
public struct CharacterStats
{
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
    public int Intelligence { get; set; }
}
}