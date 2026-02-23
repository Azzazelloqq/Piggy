using LocalSaveSystem;

namespace Code.Game.Saves.Characters
{
[SaveModel]
[SaveVersion(1)]
public struct CharacterSkill
{
    public string SkillId { get; set; }
    public int SkillLevel { get; set; }
}
}