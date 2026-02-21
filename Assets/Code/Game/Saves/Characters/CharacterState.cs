namespace Code.Game.Saves.Characters
{
public readonly struct CharacterState
{
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public int CurrentMana { get; }

    public CharacterState(int currentHealth, int maxHealth, int currentMana)
    {
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        CurrentMana = currentMana;
    }
}
}