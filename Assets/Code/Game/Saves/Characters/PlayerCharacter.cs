using System;

namespace Code.Game.Saves.Characters
{
public readonly struct PlayerCharacter
{
    public string CharacterId { get; }
    public string CharacterName { get; }
    public int CharacterLevel { get; }
    public CharacterState State { get; }
    public CharacterSkill[] Skills { get; }
    public CharacterInventory Inventory { get; }
    public CharacterStats Stats { get; }
    public CharacterTags Tags { get; }
    public CharacterView CharacterView { get; }

    public PlayerCharacter(
        string characterId,
        string characterName,
        int characterLevel,
        CharacterState characterState,
        CharacterSkill[] skills,
        CharacterInventory inventory,
        CharacterStats stats,
        CharacterTags tags,
        CharacterView characterView)
    {
        CharacterId = characterId ?? string.Empty;
        CharacterName = characterName ?? string.Empty;
        State = characterState;
        CharacterLevel = characterLevel;
        Skills = skills ?? Array.Empty<CharacterSkill>();
        Inventory = inventory;
        Stats = stats;
        Tags = tags;
        CharacterView = characterView;
    }
}
}