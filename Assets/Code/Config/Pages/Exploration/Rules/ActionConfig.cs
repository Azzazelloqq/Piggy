using System;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
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
}
