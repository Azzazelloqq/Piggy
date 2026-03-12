using System;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
[Serializable]
public struct EntityConfig
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private MapEntityType _type;

    [SerializeField]
    private string _nodeId;

    [SerializeField]
    private DiscoveryConfig _discovery;

    [SerializeField]
    private ActivityConfig _activity;

    [SerializeField]
    private TransitionConfig _transition;

    [SerializeField]
    private EventConfig[] _events;

    public string Id => _id;
    public string DisplayName => _displayName;
    public MapEntityType Type => _type;
    public string NodeId => _nodeId;
    public DiscoveryConfig Discovery => _discovery;
    public ActivityConfig Activity => _activity;
    public TransitionConfig Transition => _transition;
    public EventConfig[] Events => _events;
}
}
