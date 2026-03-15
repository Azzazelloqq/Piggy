using System;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
[Serializable]
public struct TimeFlowConfig
{
    [SerializeField]
    private float _unitsPerSecond;

    public float UnitsPerSecond => _unitsPerSecond;
}
}
