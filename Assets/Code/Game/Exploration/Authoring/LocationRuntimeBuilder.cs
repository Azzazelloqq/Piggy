using System;
using System.Collections.Generic;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Game.Exploration.Authoring
{
public static class LocationRuntimeBuilder
{
    public static LocationRuntimeDescriptor Build(LocationAuthoring authoring)
    {
        if (authoring == null)
        {
            return null;
        }

        var nodes = BuildNodes(authoring.Nodes);
        var entities = BuildEntities(authoring.Entities);
        return new LocationRuntimeDescriptor(authoring.LocationId, nodes, entities);
    }

    public static bool Validate(LocationAuthoring authoring, out string message)
    {
        if (authoring == null)
        {
            message = "LocationAuthoring is missing.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(authoring.LocationId))
        {
            message = "LocationAuthoring has empty LocationId.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private static NodeDescriptor[] BuildNodes(MapNodeAuthoring[] nodes)
    {
        if (nodes == null || nodes.Length == 0)
        {
            return Array.Empty<NodeDescriptor>();
        }

        var result = new NodeDescriptor[nodes.Length];
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node == null)
            {
                continue;
            }

            var connections = ResolveConnections(node);
            result[i] = new NodeDescriptor(node.NodeId, node.transform.position, connections);
        }

        return result;
    }

    private static string[] ResolveConnections(MapNodeAuthoring node)
    {
        if (node.Connections == null || node.Connections.Length == 0)
        {
            return Array.Empty<string>();
        }

        var result = new List<string>(node.Connections.Length);
        for (var i = 0; i < node.Connections.Length; i++)
        {
            var connection = node.Connections[i];
            if (connection == null || string.IsNullOrWhiteSpace(connection.NodeId))
            {
                continue;
            }

            result.Add(connection.NodeId);
        }

        return result.ToArray();
    }

    private static EntityDescriptor[] BuildEntities(MapEntityAuthoring[] entities)
    {
        if (entities == null || entities.Length == 0)
        {
            return Array.Empty<EntityDescriptor>();
        }

        var result = new EntityDescriptor[entities.Length];
        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            if (entity == null)
            {
                continue;
            }

            var nodeId = entity.Node != null ? entity.Node.NodeId : string.Empty;
            result[i] = new EntityDescriptor(entity.EntityId, nodeId, entity.Type, entity.transform.position);
        }

        return result;
    }
}

public sealed class LocationRuntimeDescriptor
{
    public LocationRuntimeDescriptor(string locationId, NodeDescriptor[] nodes, EntityDescriptor[] entities)
    {
        LocationId = locationId ?? string.Empty;
        Nodes = nodes ?? Array.Empty<NodeDescriptor>();
        Entities = entities ?? Array.Empty<EntityDescriptor>();
    }

    public string LocationId { get; }
    public NodeDescriptor[] Nodes { get; }
    public EntityDescriptor[] Entities { get; }
}

public readonly struct NodeDescriptor
{
    public NodeDescriptor(string nodeId, Vector3 position, string[] connections)
    {
        NodeId = nodeId ?? string.Empty;
        Position = position;
        Connections = connections ?? Array.Empty<string>();
    }

    public string NodeId { get; }
    public Vector3 Position { get; }
    public string[] Connections { get; }
}

public readonly struct EntityDescriptor
{
    public EntityDescriptor(string entityId, string nodeId, MapEntityType type, Vector3 position)
    {
        EntityId = entityId ?? string.Empty;
        NodeId = nodeId ?? string.Empty;
        Type = type;
        Position = position;
    }

    public string EntityId { get; }
    public string NodeId { get; }
    public MapEntityType Type { get; }
    public Vector3 Position { get; }
}
}