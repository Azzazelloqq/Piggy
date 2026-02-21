namespace Code.Game.Saves.Characters
{
public readonly struct CharacterSkill
{
    public string SkillId { get; }
    public int SkillLevel { get; }

    public CharacterSkill(string skillId, int skillLevel)
    {
        SkillId = skillId ?? string.Empty;
        SkillLevel = skillLevel;
    }
}
}