using System;
using UnityEngine;

namespace Code.Config.Pages.CharactersPage
{
[Serializable]
public struct CharacterStatConfig
{
    [SerializeField]
    private CharacterStatType _type;

    [SerializeField]
    private string _localisationKey;

    [SerializeField]
    private string _fallbackLabel;

    public CharacterStatType Type => _type;
    public string LocalisationKey => _localisationKey;
    public string FallbackLabel => _fallbackLabel;
}
}
