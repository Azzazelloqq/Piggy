using System;
using UnityEngine;

namespace Code.Config.Pages.CharactersPage
{
[Serializable]
public struct CharacterAvatarConfig
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private Sprite _portrait;

    public string Id => _id;
    public Sprite Portrait => _portrait;
    
}
}
