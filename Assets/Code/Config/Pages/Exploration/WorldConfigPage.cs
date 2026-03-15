using Azzazelloqq.Config;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
[CreateAssetMenu(menuName = "Config/Exploration/WorldConfigPage", fileName = "WorldConfigPage")]
public sealed class WorldConfigPage : ScriptableObject, IConfigPage
{
    [SerializeField]
    private string _defaultStartLocationId;

    [SerializeField]
    private string _defaultStartNodeId;

    [SerializeField]
    private int _timeUnitMinutes = 10;

    [SerializeField]
    private int _startTimeUnits;

    [SerializeField]
    private float _defaultFlowUnitsPerSecond = 1f;

    public string DefaultStartLocationId => _defaultStartLocationId;
    public string DefaultStartNodeId => _defaultStartNodeId;
    public int TimeUnitMinutes => _timeUnitMinutes;
    public int StartTimeUnits => _startTimeUnits;
    public float DefaultFlowUnitsPerSecond => _defaultFlowUnitsPerSecond;
}
}
