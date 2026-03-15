using System;
using System.Collections.Generic;
using Code.Config.Pages.Exploration;
using Code.Game.Exploration.Authoring;
using Code.Game.Exploration.Domain;
using Code.Game.Exploration.Runtime;

namespace Code.Game.Exploration.Map
{
public static class ExplorationMapDefinitionBuilder
{
    public static ExplorationMapDefinition Build(ExplorationSession session, LocationRuntimeDescriptor runtimeDescriptor)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (runtimeDescriptor == null)
        {
            throw new ArgumentNullException(nameof(runtimeDescriptor));
        }

        var nodes = BuildNodes(runtimeDescriptor);
        var nodeLookup = BuildNodeLookup(nodes);
        var points = BuildPoints(session, runtimeDescriptor);

        if (!nodeLookup.TryGetValue(session.WorldState.CurrentNodeId, out var currentNode))
        {
            throw new InvalidOperationException(
                $"Map runtime does not contain the current node '{session.WorldState.CurrentNodeId}'.");
        }

        return new ExplorationMapDefinition(
            session.WorldState.CurrentNodeId,
            currentNode.WorldPosition,
            nodes,
            points);
    }

    private static ExplorationMapNodeData[] BuildNodes(LocationRuntimeDescriptor runtimeDescriptor)
    {
        var sourceNodes = runtimeDescriptor.Nodes ?? Array.Empty<NodeDescriptor>();
        if (sourceNodes.Length == 0)
        {
            return Array.Empty<ExplorationMapNodeData>();
        }

        var result = new ExplorationMapNodeData[sourceNodes.Length];
        for (var i = 0; i < sourceNodes.Length; i++)
        {
            var node = sourceNodes[i];
            result[i] = new ExplorationMapNodeData(node.NodeId, node.Position, node.Connections);
        }

        return result;
    }

    private static ExplorationMapPointData[] BuildPoints(ExplorationSession session, LocationRuntimeDescriptor runtimeDescriptor)
    {
        var configEntities = session.CurrentLocationConfig.Entities ?? Array.Empty<EntityConfig>();
        if (configEntities.Length == 0)
        {
            return Array.Empty<ExplorationMapPointData>();
        }

        var descriptorLookup = new Dictionary<string, EntityDescriptor>(StringComparer.Ordinal);
        foreach (var entityDescriptor in runtimeDescriptor.Entities ?? Array.Empty<EntityDescriptor>())
        {
            if (string.IsNullOrWhiteSpace(entityDescriptor.EntityId))
            {
                continue;
            }

            descriptorLookup[entityDescriptor.EntityId] = entityDescriptor;
        }

        var result = new List<ExplorationMapPointData>(configEntities.Length);
        foreach (var entityConfig in configEntities)
        {
            if (string.IsNullOrWhiteSpace(entityConfig.Id))
            {
                continue;
            }

            if (!descriptorLookup.TryGetValue(entityConfig.Id, out var descriptor))
            {
                continue;
            }

            if (!session.CurrentLocationState.Entities.TryGetValue(entityConfig.Id, out var entityState))
            {
                continue;
            }

            var isVisible = entityState.KnowledgeState != KnowledgeState.Unknown && !entityState.IsConsumed;
            if (!isVisible)
            {
                continue;
            }

            var nodeId = !string.IsNullOrWhiteSpace(entityConfig.NodeId) ? entityConfig.NodeId : descriptor.NodeId;
            var displayName = !string.IsNullOrWhiteSpace(entityConfig.DisplayName) ? entityConfig.DisplayName : entityConfig.Id;
            result.Add(new ExplorationMapPointData(
                entityConfig.Id,
                displayName,
                nodeId,
                entityConfig.Type,
                descriptor.Position,
                isVisible: true,
                isInteractable: entityState.IsAvailable));
        }

        return result.ToArray();
    }

    private static Dictionary<string, ExplorationMapNodeData> BuildNodeLookup(ExplorationMapNodeData[] nodes)
    {
        var lookup = new Dictionary<string, ExplorationMapNodeData>(StringComparer.Ordinal);
        foreach (var node in nodes ?? Array.Empty<ExplorationMapNodeData>())
        {
            if (string.IsNullOrWhiteSpace(node.NodeId))
            {
                continue;
            }

            lookup[node.NodeId] = node;
        }

        return lookup;
    }
}
}
