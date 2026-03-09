using Azzazelloqq.Config;
using UnityEngine;

namespace Code.Config.Pages.CharactersPage
{
[CreateAssetMenu(menuName = "Config/CharactersConfigPage", fileName = "CharactersConfigPage")]
public class CharactersConfigPage : ScriptableObject, IConfigPage
{
    [SerializeField]
    private CharacterTagsConfig[] _allTraitsIds;

    [SerializeField]
    private StatsConfig _statsConfig;

    [SerializeField]
    private int _maxTraits = 2;

    [SerializeField]
    private CharacterClassConfig[] _classes;

    [SerializeField]
    private CharacterAvatarConfig[] _avatars;

    public CharacterTagsConfig[] AllTraitsIds => _allTraitsIds;
    public StatsConfig StatsConfig => _statsConfig;
    public int MaxTraits => _maxTraits;
    public CharacterClassConfig[] Classes => _classes;
    public CharacterAvatarConfig[] Avatars => _avatars;
}
}