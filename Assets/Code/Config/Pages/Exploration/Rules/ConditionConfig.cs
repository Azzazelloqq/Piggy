using System;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
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
}
