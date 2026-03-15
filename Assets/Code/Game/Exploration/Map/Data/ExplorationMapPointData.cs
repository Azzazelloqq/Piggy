using System;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Game.Exploration.Map
{
public readonly struct ExplorationMapPointData
{
    public ExplorationMapPointData(
        string entityId,
        string displayName,
        string nodeId,
        MapEntityType entityType,
        Vector3 worldPosition,
        bool isVisible,
        bool isInteractable)
    {
        EntityId = entityId ?? string.Empty;
        DisplayName = displayName ?? string.Empty;
        NodeId = nodeId ?? string.Empty;
        EntityType = entityType;
        WorldPosition = worldPosition;
        IsVisible = isVisible;
        IsInteractable = isInteractable;
    }

    public string EntityId { get; }
    public string DisplayName { get; }
    public string NodeId { get; }
    public MapEntityType EntityType { get; }
    public Vector3 WorldPosition { get; }
    public bool IsVisible { get; }
    public bool IsInteractable { get; }
}
}
