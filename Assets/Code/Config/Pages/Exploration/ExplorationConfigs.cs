using System;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
[CreateAssetMenu(menuName = "Config/Exploration/LocationConfig", fileName = "LocationConfig")]
public sealed class LocationConfig : ScriptableObject
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private string _defaultNodeId;

    [SerializeField]
    private NodeConfig[] _nodes;

    [SerializeField]
    private EntityConfig[] _entities;

    [SerializeField]
    private EventConfig[] _events;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string DefaultNodeId => _defaultNodeId;
    public NodeConfig[] Nodes => _nodes;
    public EntityConfig[] Entities => _entities;
    public EventConfig[] Events => _events;
}

[Serializable]
public struct NodeConfig
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private string[] _connectedNodeIds;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string[] ConnectedNodeIds => _connectedNodeIds;
}

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

[Serializable]
public struct ActivityConfig
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private ActivityOptionConfig[] _options;

    public string Id => _id;
    public string DisplayName => _displayName;
    public ActivityOptionConfig[] Options => _options;
}

[Serializable]
public struct ActivityOptionConfig
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private int _timeCost;

    [SerializeField]
    private CheckConfig _check;

    [SerializeField]
    private ConditionConfig[] _conditions;

    [SerializeField]
    private ActionConfig[] _actions;

    public string Id => _id;
    public string DisplayName => _displayName;
    public int TimeCost => _timeCost;
    public CheckConfig Check => _check;
    public ConditionConfig[] Conditions => _conditions;
    public ActionConfig[] Actions => _actions;
}

[Serializable]
public struct TransitionConfig
{
    [SerializeField]
    private string _targetLocationId;

    [SerializeField]
    private string _targetNodeId;

    [SerializeField]
    private int _timeCost;

    [SerializeField]
    private CheckConfig _check;

    [SerializeField]
    private ConditionConfig[] _conditions;

    [SerializeField]
    private ActionConfig[] _actions;

    public string TargetLocationId => _targetLocationId;
    public string TargetNodeId => _targetNodeId;
    public int TimeCost => _timeCost;
    public CheckConfig Check => _check;
    public ConditionConfig[] Conditions => _conditions;
    public ActionConfig[] Actions => _actions;
}

[Serializable]
public struct EventConfig
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private EventTriggerType _trigger;

    [SerializeField]
    private int _priority;

    [SerializeField]
    private float _weight;

    [SerializeField]
    private bool _isRepeatable;

    [SerializeField]
    private int _cooldownTimeUnits;

    [SerializeField]
    private string _targetNodeId;

    [SerializeField]
    private string _targetEntityId;

    [SerializeField]
    private ConditionConfig[] _conditions;

    [SerializeField]
    private ActionConfig[] _actions;

    public string Id => _id;
    public string DisplayName => _displayName;
    public EventTriggerType Trigger => _trigger;
    public int Priority => _priority;
    public float Weight => _weight;
    public bool IsRepeatable => _isRepeatable;
    public int CooldownTimeUnits => _cooldownTimeUnits;
    public string TargetNodeId => _targetNodeId;
    public string TargetEntityId => _targetEntityId;
    public ConditionConfig[] Conditions => _conditions;
    public ActionConfig[] Actions => _actions;
}

[Serializable]
public struct ConditionConfig
{
    [SerializeField]
    private ConditionType _type;

    [SerializeField]
    private string _flagId;

    [SerializeField]
    private bool _flagValue;

    [SerializeField]
    private string _entityId;

    [SerializeField]
    private KnowledgeState _knowledgeState;

    [SerializeField]
    private int _minTimeUnits;

    [SerializeField]
    private int _maxTimeUnits;

    [SerializeField]
    private bool _invert;

    public ConditionType Type => _type;
    public string FlagId => _flagId;
    public bool FlagValue => _flagValue;
    public string EntityId => _entityId;
    public KnowledgeState KnowledgeState => _knowledgeState;
    public int MinTimeUnits => _minTimeUnits;
    public int MaxTimeUnits => _maxTimeUnits;
    public bool Invert => _invert;
}

[Serializable]
public struct ActionConfig
{
    [SerializeField]
    private ActionType _type;

    [SerializeField]
    private string _flagId;

    [SerializeField]
    private bool _flagValue;

    [SerializeField]
    private string _entityId;

    [SerializeField]
    private KnowledgeState _knowledgeState;

    [SerializeField]
    private int _timeCost;

    [SerializeField]
    private string _targetLocationId;

    [SerializeField]
    private string _targetNodeId;

    [SerializeField]
    private int _suspicionDelta;

    public ActionType Type => _type;
    public string FlagId => _flagId;
    public bool FlagValue => _flagValue;
    public string EntityId => _entityId;
    public KnowledgeState KnowledgeState => _knowledgeState;
    public int TimeCost => _timeCost;
    public string TargetLocationId => _targetLocationId;
    public string TargetNodeId => _targetNodeId;
    public int SuspicionDelta => _suspicionDelta;
}

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
