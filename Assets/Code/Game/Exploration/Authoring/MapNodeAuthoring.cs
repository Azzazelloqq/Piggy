using UnityEngine;

namespace Code.Game.Exploration.Authoring
{
public sealed class MapNodeAuthoring : MonoBehaviour
{
    [SerializeField]
    private string _nodeId;

    [SerializeField]
    private MapNodeAuthoring[] _connections;

    public string NodeId => _nodeId;
    public MapNodeAuthoring[] Connections => _connections;
}
}
