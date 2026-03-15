using System;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public readonly struct ExplorationMapRoutePlan
{
    public ExplorationMapRoutePlan(string pointId, string targetNodeId, Vector3[] waypoints, bool requiresMovement)
    {
        PointId = pointId ?? string.Empty;
        TargetNodeId = targetNodeId ?? string.Empty;
        Waypoints = waypoints ?? Array.Empty<Vector3>();
        RequiresMovement = requiresMovement;
    }

    public string PointId { get; }
    public string TargetNodeId { get; }
    public Vector3[] Waypoints { get; }
    public bool RequiresMovement { get; }
}
}
