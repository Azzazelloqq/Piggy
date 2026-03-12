using System;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
[Serializable]
public struct NodeConfig
{
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private string[] _connectedNodeIds;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string[] ConnectedNodeIds => _connectedNodeIds;
}
}
