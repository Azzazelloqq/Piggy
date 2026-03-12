using System;
using System.Collections.Generic;
using Code.Config.Pages.Exploration;

namespace Code.Game.Exploration.Runtime
{
public sealed class ExplorationSession
{
    public ExplorationSession(
        WorldRuntimeState worldState,
        IReadOnlyDictionary<string, LocationConfig> locationConfigs,
        TimeService timeService,
        ExplorationRuntimeServices runtimeServices)
    {
        WorldState = worldState ?? throw new ArgumentNullException(nameof(worldState));
        _locationConfigs = locationConfigs ?? throw new ArgumentNullException(nameof(locationConfigs));
        TimeService = timeService ?? throw new ArgumentNullException(nameof(timeService));
        RuntimeServices = runtimeServices ?? throw new ArgumentNullException(nameof(runtimeServices));
        RefreshCurrentLocation();
    }

    public WorldRuntimeState WorldState { get; }
    public LocationConfig CurrentLocationConfig { get; private set; }
    public LocationRuntimeState CurrentLocationState { get; private set; }
    public TimeService TimeService { get; }
    public ExplorationRuntimeServices RuntimeServices { get; }

    private readonly IReadOnlyDictionary<string, LocationConfig> _locationConfigs;

    public void RefreshCurrentLocation()
    {
        if (string.IsNullOrWhiteSpace(WorldState.CurrentLocationId))
        {
            throw new InvalidOperationException("Current location id is missing from the world state.");
        }

        if (!_locationConfigs.TryGetValue(WorldState.CurrentLocationId, out var currentLocationConfig))
        {
            throw new InvalidOperationException($"Location '{WorldState.CurrentLocationId}' is not registered in the exploration session.");
        }

        if (!WorldState.Locations.TryGetValue(WorldState.CurrentLocationId, out var currentLocationState))
        {
            throw new InvalidOperationException($"Runtime state for location '{WorldState.CurrentLocationId}' is missing.");
        }

        if (string.IsNullOrWhiteSpace(WorldState.CurrentNodeId))
        {
            throw new InvalidOperationException("Current node id is missing from the world state.");
        }

        var nodes = currentLocationConfig.Nodes;
        if (nodes == null || nodes.Length == 0)
        {
            throw new InvalidOperationException($"Location '{currentLocationConfig.Id}' does not define any nodes.");
        }

        var nodeExists = false;
        for (var i = 0; i < nodes.Length; i++)
        {
            if (string.Equals(nodes[i].Id, WorldState.CurrentNodeId, StringComparison.Ordinal))
            {
                nodeExists = true;
                break;
            }
        }

        if (!nodeExists)
        {
            throw new InvalidOperationException(
                $"Location '{currentLocationConfig.Id}' does not contain node '{WorldState.CurrentNodeId}'.");
        }

        CurrentLocationConfig = currentLocationConfig;
        CurrentLocationState = currentLocationState;
    }
}
}