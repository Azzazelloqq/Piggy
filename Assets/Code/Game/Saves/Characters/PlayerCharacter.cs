using System;
using LocalSaveSystem;

namespace Code.Game.Saves.Characters
{
[SaveModel]
[SaveVersion(1)]
public struct PlayerCharacter
{
    public string CharacterId { get; set; }
    public string CharacterName { get; set; }
    public int CharacterLevel { get; set; }
    public CharacterState State { get; set; }
    public CharacterSkill[] Skills { get; set; }
    public CharacterInventory Inventory { get; set; }
    public CharacterStats Stats { get; set; }
    public CharacterTags Tags { get; set; }
    public CharacterView CharacterView { get; set; }
}
}