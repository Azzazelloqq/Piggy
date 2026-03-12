using UnityEngine;

namespace Code.Game.Exploration.Authoring
{
public sealed class TransitionAuthoring : MonoBehaviour
{
    [SerializeField]
    private string _targetLocationId;

    [SerializeField]
    private string _targetNodeId;

    public string TargetLocationId => _targetLocationId;
    public string TargetNodeId => _targetNodeId;
}
}
