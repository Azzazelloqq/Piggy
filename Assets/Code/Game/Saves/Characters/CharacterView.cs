namespace Code.Game.Saves.Characters
{
public readonly struct CharacterView 
{
    public string AvatarId { get; }

    public CharacterView(string avatarId)
    {
        AvatarId = avatarId ?? string.Empty;
    }
}
}