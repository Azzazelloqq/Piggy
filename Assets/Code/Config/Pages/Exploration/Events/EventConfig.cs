using System;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
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
}
