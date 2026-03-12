using System;
using System.Collections.Generic;
using Code.Config.Pages.Exploration;
using Code.Game.Exploration.Domain;
using Code.Game.Saves.World;

namespace Code.Game.Exploration.Runtime
{
public static class WorldStateMapper
{
    public static WorldRuntimeState ToRuntime(
        WorldStateSave save,
        WorldConfigPage worldConfig,
        LocationsConfigPage locationsConfig)
    {
        var currentLocationId = ResolveCurrentLocationId(save, worldConfig, locationsConfig);
        var currentNodeId = ResolveCurrentNodeId(save, worldConfig, locationsConfig, currentLocationId);
        var currentTimeUnits = ResolveCurrentTimeUnits(save, worldConfig);

        var worldState = new WorldRuntimeState(currentLocationId, currentNodeId, currentTimeUnits)
            {
                SuspicionLevel = save.SuspicionLevel
            };
        ApplyFlags(save.WorldFlags, worldState.Flags);

        var locationLookup = BuildLocationLookup(locationsConfig);
        foreach (var pair in locationLookup)
        {
            var locationConfig = pair.Value;
            var locationState = BuildLocationState(locationConfig, save);
            worldState.Locations[locationState.LocationId] = locationState;
        }

        if (!string.IsNullOrWhiteSpace(currentLocationId) &&
            !worldState.Locations.ContainsKey(currentLocationId))
        {
            worldState.Locations[currentLocationId] = new LocationRuntimeState(currentLocationId);
        }

        return worldState;
    }

    public static WorldStateSave ToSave(WorldRuntimeState runtime)
    {
        var locations = new List<LocationStateSave>();
        foreach (var locationPair in runtime.Locations)
        {
            var location = locationPair.Value;
            var locationSave = new LocationStateSave
            {
                LocationId = location.LocationId,
                Nodes = BuildNodeSaves(location.Nodes),
                Entities = BuildEntitySaves(location.Entities),
                LocalFlags = BuildFlagSaves(location.LocalFlags),
                Events = BuildEventSaves(location.Events)
            };
            locations.Add(locationSave);
        }

        return new WorldStateSave
        {
            CurrentTimeUnits = runtime.CurrentTimeUnits,
            CurrentLocationId = runtime.CurrentLocationId,
            CurrentNodeId = runtime.CurrentNodeId,
            SuspicionLevel = runtime.SuspicionLevel,
            WorldFlags = BuildFlagSaves(runtime.Flags),
            Locations = locations.ToArray()
        };
    }

    private static string ResolveCurrentLocationId(
        WorldStateSave save,
        WorldConfigPage worldConfig,
        LocationsConfigPage locationsConfig)
    {
        if (!string.IsNullOrWhiteSpace(save.CurrentLocationId))
        {
            return save.CurrentLocationId;
        }

        if (worldConfig != null && !string.IsNullOrWhiteSpace(worldConfig.DefaultStartLocationId))
        {
            return worldConfig.DefaultStartLocationId;
        }

        var locations = locationsConfig?.Locations;
        if (locations != null && locations.Length > 0 && locations[0] != null)
        {
            return locations[0].Id;
        }

        return string.Empty;
    }

    private static string ResolveCurrentNodeId(
        WorldStateSave save,
        WorldConfigPage worldConfig,
        LocationsConfigPage locationsConfig,
        string locationId)
    {
        if (!string.IsNullOrWhiteSpace(save.CurrentNodeId))
        {
            return save.CurrentNodeId;
        }

        if (worldConfig != null && !string.IsNullOrWhiteSpace(worldConfig.DefaultStartNodeId))
        {
            return worldConfig.DefaultStartNodeId;
        }

        if (string.IsNullOrWhiteSpace(locationId))
        {
            return string.Empty;
        }

        var location = locationsConfig?.FindLocation(locationId);
        if (location != null && !string.IsNullOrWhiteSpace(location.DefaultNodeId))
        {
            return location.DefaultNodeId;
        }

        return string.Empty;
    }

    private static int ResolveCurrentTimeUnits(WorldStateSave save, WorldConfigPage worldConfig)
    {
        if (save.CurrentTimeUnits > 0)
        {
            return save.CurrentTimeUnits;
        }

        return worldConfig != null ? worldConfig.StartTimeUnits : 0;
    }

    private static Dictionary<string, LocationConfig> BuildLocationLookup(LocationsConfigPage locationsConfig)
    {
        var lookup = new Dictionary<string, LocationConfig>(StringComparer.Ordinal);
        var locations = locationsConfig?.Locations;
        if (locations == null)
        {
            return lookup;
        }

        for (var i = 0; i < locations.Length; i++)
        {
            var location = locations[i];
            if (location == null || string.IsNullOrWhiteSpace(location.Id))
            {
                continue;
            }

            lookup[location.Id] = location;
        }

        return lookup;
    }

    private static LocationRuntimeState BuildLocationState(LocationConfig config, WorldStateSave save)
    {
        var locationState = new LocationRuntimeState(config.Id);
        var locationSave = FindLocationSave(save.Locations, config.Id);

        ApplyFlags(locationSave.LocalFlags, locationState.LocalFlags);
        ApplyNodes(config.Nodes, locationSave.Nodes, locationState.Nodes);
        ApplyEntities(config.Entities, locationSave.Entities, locationState.Entities);
        ApplyEvents(config.Events, locationSave.Events, locationState.Events);

        if (config.Entities != null)
        {
            for (var i = 0; i < config.Entities.Length; i++)
            {
                var entity = config.Entities[i];
                if (entity.Events == null)
                {
                    continue;
                }

                ApplyEvents(entity.Events, locationSave.Events, locationState.Events);
            }
        }

        return locationState;
    }

    private static void ApplyFlags(FlagStateSave[] saves, Dictionary<string, bool> flags)
    {
        if (saves == null)
        {
            return;
        }

        foreach (var save in saves)
        {
            if (string.IsNullOrWhiteSpace(save.FlagId))
            {
                continue;
            }

            flags[save.FlagId] = save.Value;
        }
    }

    private static void ApplyNodes(
        NodeConfig[] nodes,
        NodeStateSave[] saves,
        Dictionary<string, NodeRuntimeState> runtimeNodes)
    {
        if (nodes == null)
        {
            return;
        }

        var saveLookup = BuildNodeLookup(saves);
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (string.IsNullOrWhiteSpace(node.Id))
            {
                continue;
            }

            var runtime = new NodeRuntimeState(node.Id);
            if (saveLookup.TryGetValue(node.Id, out var nodeSave))
            {
                runtime.IsVisited = nodeSave.IsVisited;
            }

            runtimeNodes[node.Id] = runtime;
        }
    }

    private static void ApplyEntities(
        EntityConfig[] entities,
        EntityStateSave[] saves,
        Dictionary<string, EntityRuntimeState> runtimeEntities)
    {
        if (entities == null)
        {
            return;
        }

        var saveLookup = BuildEntityLookup(saves);
        foreach (var entity in entities)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                continue;
            }

            var knowledgeState = entity.Discovery.InitialState;
            var runtime = new EntityRuntimeState(entity.Id, knowledgeState);
            if (saveLookup.TryGetValue(entity.Id, out var entitySave))
            {
                runtime.KnowledgeState = entitySave.KnowledgeState;
                runtime.IsAvailable = entitySave.IsAvailable;
                runtime.IsConsumed = entitySave.IsConsumed;
            }

            runtimeEntities[entity.Id] = runtime;
        }
    }

    private static void ApplyEvents(
        EventConfig[] events,
        EventStateSave[] saves,
        Dictionary<string, EventRuntimeState> runtimeEvents)
    {
        if (events == null)
        {
            return;
        }

        var saveLookup = BuildEventLookup(saves);
        foreach (var eventConfig in events)
        {
            if (string.IsNullOrWhiteSpace(eventConfig.Id))
            {
                continue;
            }

            var runtime = new EventRuntimeState(eventConfig.Id);
            if (saveLookup.TryGetValue(eventConfig.Id, out var eventSave))
            {
                runtime.LastTriggeredTimeUnits = eventSave.LastTriggeredTimeUnits;
                runtime.TriggerCount = eventSave.TriggerCount;
            }

            runtimeEvents[eventConfig.Id] = runtime;
        }
    }

    private static LocationStateSave FindLocationSave(LocationStateSave[] saves, string locationId)
    {
        if (saves == null || string.IsNullOrWhiteSpace(locationId))
        {
            return default;
        }

        foreach (var save in saves)
        {
            if (string.Equals(save.LocationId, locationId, StringComparison.Ordinal))
            {
                return save;
            }
        }

        return default;
    }

    private static Dictionary<string, NodeStateSave> BuildNodeLookup(NodeStateSave[] saves)
    {
        var lookup = new Dictionary<string, NodeStateSave>(StringComparer.Ordinal);
        if (saves == null)
        {
            return lookup;
        }

        foreach (var save in saves)
        {
            if (string.IsNullOrWhiteSpace(save.NodeId))
            {
                continue;
            }

            lookup[save.NodeId] = save;
        }

        return lookup;
    }

    private static Dictionary<string, EntityStateSave> BuildEntityLookup(EntityStateSave[] saves)
    {
        var lookup = new Dictionary<string, EntityStateSave>(StringComparer.Ordinal);
        if (saves == null)
        {
            return lookup;
        }

        foreach (var save in saves)
        {
            if (string.IsNullOrWhiteSpace(save.EntityId))
            {
                continue;
            }

            lookup[save.EntityId] = save;
        }

        return lookup;
    }

    private static Dictionary<string, EventStateSave> BuildEventLookup(EventStateSave[] saves)
    {
        var lookup = new Dictionary<string, EventStateSave>(StringComparer.Ordinal);
        if (saves == null)
        {
            return lookup;
        }

        foreach (var save in saves)
        {
            if (string.IsNullOrWhiteSpace(save.EventId))
            {
                continue;
            }

            lookup[save.EventId] = save;
        }

        return lookup;
    }

    private static FlagStateSave[] BuildFlagSaves(Dictionary<string, bool> flags)
    {
        if (flags == null || flags.Count == 0)
        {
            return Array.Empty<FlagStateSave>();
        }

        var result = new FlagStateSave[flags.Count];
        var index = 0;
        foreach (var pair in flags)
        {
            result[index++] = new FlagStateSave
            {
                FlagId = pair.Key,
                Value = pair.Value
            };
        }

        return result;
    }

    private static NodeStateSave[] BuildNodeSaves(Dictionary<string, NodeRuntimeState> nodes)
    {
        if (nodes == null || nodes.Count == 0)
        {
            return Array.Empty<NodeStateSave>();
        }

        var result = new NodeStateSave[nodes.Count];
        var index = 0;
        foreach (var pair in nodes)
        {
            var node = pair.Value;
            result[index++] = new NodeStateSave
            {
                NodeId = node.NodeId,
                IsVisited = node.IsVisited
            };
        }

        return result;
    }

    private static EntityStateSave[] BuildEntitySaves(Dictionary<string, EntityRuntimeState> entities)
    {
        if (entities == null || entities.Count == 0)
        {
            return Array.Empty<EntityStateSave>();
        }

        var result = new EntityStateSave[entities.Count];
        var index = 0;
        foreach (var pair in entities)
        {
            var entity = pair.Value;
            result[index++] = new EntityStateSave
            {
                EntityId = entity.EntityId,
                KnowledgeState = entity.KnowledgeState,
                IsAvailable = entity.IsAvailable,
                IsConsumed = entity.IsConsumed
            };
        }

        return result;
    }

    private static EventStateSave[] BuildEventSaves(Dictionary<string, EventRuntimeState> events)
    {
        if (events == null || events.Count == 0)
        {
            return Array.Empty<EventStateSave>();
        }

        var result = new EventStateSave[events.Count];
        var index = 0;
        foreach (var pair in events)
        {
            var runtimeEvent = pair.Value;
            result[index++] = new EventStateSave
            {
                EventId = runtimeEvent.EventId,
                LastTriggeredTimeUnits = runtimeEvent.LastTriggeredTimeUnits,
                TriggerCount = runtimeEvent.TriggerCount
            };
        }

        return result;
    }
}
}