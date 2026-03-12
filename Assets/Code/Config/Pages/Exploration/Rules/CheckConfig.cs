using System;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
[Serializable]
public struct CheckConfig
{
    [SerializeField]
    private string _statId;

    [SerializeField]
    private int _dc;

    [SerializeField]
    private int _modifier;

    public string StatId => _statId;
    public int DC => _dc;
    public int Modifier => _modifier;
}
}
