using System;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
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
    private TimeAdvanceMode _timeAdvanceMode;

    [SerializeField]
    private TimeFlowConfig _timeFlow;

    [SerializeField]
    private CheckConfig _check;

    [SerializeField]
    private ConditionConfig[] _conditions;

    [SerializeField]
    private ActionConfig[] _actions;

    public string Id => _id;
    public string DisplayName => _displayName;
    public int TimeCost => _timeCost;
    public TimeAdvanceMode TimeAdvanceMode => _timeAdvanceMode;
    public TimeFlowConfig TimeFlow => _timeFlow;
    public CheckConfig Check => _check;
    public ConditionConfig[] Conditions => _conditions;
    public ActionConfig[] Actions => _actions;
}
}
