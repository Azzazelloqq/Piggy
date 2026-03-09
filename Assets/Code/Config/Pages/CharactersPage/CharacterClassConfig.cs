using System;
using UnityEngine;

namespace Code.Config.Pages.CharactersPage
{
[Serializable]
public struct CharacterClassConfig
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _localisationKey;

    [SerializeField]
    private string _avatarId;

    public string Id => _id;
    public string LocalisationKey => _localisationKey;
    public string AvatarId => _avatarId;
}
}
