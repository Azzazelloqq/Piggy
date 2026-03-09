using System;
using UnityEngine;

namespace Code.Config.Pages.CharactersPage
{
[Serializable]
public struct CharacterTagsConfig
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _localisationKey;
    
    [Multiline]
    [SerializeField]
    private string _description;

    public string Id => _id;
    public string LocalisationKey => _localisationKey;
    public string Description => _description;
}
}