using System;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
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
    private TimeAdvanceMode _timeAdvanceMode;

    [SerializeField]
    private TimeFlowConfig _timeFlow;

    [SerializeField]
    private CheckConfig _check;

    [SerializeField]
    private ConditionConfig[] _conditions;

    [SerializeField]
    private ActionConfig[] _actions;

    public string TargetLocationId => _targetLocationId;
    public string TargetNodeId => _targetNodeId;
    public int TimeCost => _timeCost;
    public TimeAdvanceMode TimeAdvanceMode => _timeAdvanceMode;
    public TimeFlowConfig TimeFlow => _timeFlow;
    public CheckConfig Check => _check;
    public ConditionConfig[] Conditions => _conditions;
    public ActionConfig[] Actions => _actions;
}
}
