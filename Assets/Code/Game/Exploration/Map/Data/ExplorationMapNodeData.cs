using System;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public readonly struct ExplorationMapNodeData
{
    public ExplorationMapNodeData(string nodeId, Vector3 worldPosition, string[] connections)
    {
        NodeId = nodeId ?? string.Empty;
        WorldPosition = worldPosition;
        Connections = connections ?? Array.Empty<string>();
    }

    public string NodeId { get; }
    public Vector3 WorldPosition { get; }
    public string[] Connections { get; }
}
}
