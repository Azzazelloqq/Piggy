using System;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
[Serializable]
public struct DiscoveryConfig
{
    [SerializeField]
    private KnowledgeState _initialState;

    [SerializeField]
    private DiscoveryMode _mode;

    [SerializeField]
    private int _dc;

    [SerializeField]
    private string _requiredFlagId;

    public KnowledgeState InitialState => _initialState;
    public DiscoveryMode Mode => _mode;
    public int DC => _dc;
    public string RequiredFlagId => _requiredFlagId;
}
}
