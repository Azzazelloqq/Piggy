namespace Code.Game.Saves.Characters
{
public readonly struct CharacterStats
{
    public int Strength { get; }
    public int Dexterity { get; }
    public int Constitution { get; }
    public int Wisdom { get; }
    public int Charisma { get; }
    public int Intelligence { get; }

    public CharacterStats(
        int strength,
        int dexterity,
        int constitution,
        int wisdom,
        int charisma,
        int intelligence)
    {
        Strength = strength;
        Dexterity = dexterity;
        Constitution = constitution;
        Wisdom = wisdom;
        Charisma = charisma;
        Intelligence = intelligence;
    }
}
}