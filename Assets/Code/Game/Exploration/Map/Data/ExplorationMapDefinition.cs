using System;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public sealed class ExplorationMapDefinition
{
    public ExplorationMapDefinition(
        string currentNodeId,
        Vector3 startWorldPosition,
        ExplorationMapNodeData[] nodes,
        ExplorationMapPointData[] points)
    {
        CurrentNodeId = currentNodeId ?? string.Empty;
        StartWorldPosition = startWorldPosition;
        Nodes = nodes ?? Array.Empty<ExplorationMapNodeData>();
        Points = points ?? Array.Empty<ExplorationMapPointData>();
    }

    public string CurrentNodeId { get; }
    public Vector3 StartWorldPosition { get; }
    public ExplorationMapNodeData[] Nodes { get; }
    public ExplorationMapPointData[] Points { get; }
}
}
