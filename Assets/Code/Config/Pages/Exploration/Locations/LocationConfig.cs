using UnityEngine;

namespace Code.Config.Pages.Exploration
{
[CreateAssetMenu(menuName = "Config/Exploration/LocationConfig", fileName = "LocationConfig")]
public sealed class LocationConfig : ScriptableObject
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private string _defaultNodeId;

    [SerializeField]
    private string _prefabAddress;

    [SerializeField]
    private NodeConfig[] _nodes;

    [SerializeField]
    private EntityConfig[] _entities;

    [SerializeField]
    private EventConfig[] _events;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string DefaultNodeId => _defaultNodeId;
    public string PrefabAddress => _prefabAddress;
    public NodeConfig[] Nodes => _nodes;
    public EntityConfig[] Entities => _entities;
    public EventConfig[] Events => _events;
}
}
