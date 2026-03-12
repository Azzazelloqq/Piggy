using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Exploration.Authoring
{
public sealed class LocationAuthoring : MonoBehaviour
{
    [SerializeField]
    private string _locationId;

    [SerializeField]
    private MapNodeAuthoring[] _nodes;

    [SerializeField]
    private MapEntityAuthoring[] _entities;

    public string LocationId => _locationId;
    public MapNodeAuthoring[] Nodes => _nodes;
    public MapEntityAuthoring[] Entities => _entities;

#if UNITY_EDITOR
    private void OnValidate()
    {
        CollectChildren();
        ValidateIds();
    }
#endif

    [ContextMenu("Collect Children")]
    private void CollectChildren()
    {
        _nodes = GetComponentsInChildren<MapNodeAuthoring>(true);
        _entities = GetComponentsInChildren<MapEntityAuthoring>(true);
    }

    [ContextMenu("Validate Location Ids")]
    private void ValidateIds()
    {
        ValidateIds(_nodes, node => node.NodeId, "Node");
        ValidateIds(_entities, entity => entity.EntityId, "Entity");
    }

    private void ValidateIds<T>(T[] items, System.Func<T, string> idSelector, string label)
    {
        if (items == null)
        {
            return;
        }

        var seen = new HashSet<string>();
        foreach (var item in items)
        {
            var id = idSelector(item);
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning($"{label} has empty id on {name}.", this);
                continue;
            }

            if (!seen.Add(id))
            {
                Debug.LogWarning($"{label} id '{id}' is duplicated in {name}.", this);
            }
        }
    }
}
}
