using System;
using UnityEngine;

namespace Code.Config.Pages.CharactersPage
{
[Serializable]
public struct StatsConfig
{
    [SerializeField]
    private int _maxSumCharacterPoints;

    [SerializeField]
    private int _defaultStatValue;

    [SerializeField]
    private CharacterStatConfig[] _stats;
    
    public int MaxSumCharacterPoints => _maxSumCharacterPoints;
    public int DefaultStatValue => _defaultStatValue;
    public CharacterStatConfig[] Stats => _stats;
}
}