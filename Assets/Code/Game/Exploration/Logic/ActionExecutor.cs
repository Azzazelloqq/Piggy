using System;
using Code.Config.Pages.Exploration;
using Code.Game.Exploration.Domain;
using Code.Game.Exploration.Runtime;

namespace Code.Game.Exploration.Logic
{
public sealed class ActionExecutor
{
    public void Execute(
        ActionConfig[] actions,
        WorldRuntimeState worldState,
        LocationRuntimeState locationState,
        ExplorationTimeController timeController)
    {
        if (worldState == null)
        {
            throw new ArgumentNullException(nameof(worldState));
        }

        if (locationState == null)
        {
            throw new ArgumentNullException(nameof(locationState));
        }

        if (timeController == null)
        {
            throw new ArgumentNullException(nameof(timeController));
        }

        if (actions == null || actions.Length == 0)
        {
            return;
        }

        foreach (var action in actions)
        {
            Execute(action, worldState, locationState, timeController);
        }
    }

    public void Execute(
        ActionConfig action,
        WorldRuntimeState worldState,
        LocationRuntimeState locationState,
        ExplorationTimeController timeController)
    {
        if (worldState == null)
        {
            throw new ArgumentNullException(nameof(worldState));
        }

        if (locationState == null)
        {
            throw new ArgumentNullException(nameof(locationState));
        }

        if (timeController == null)
        {
            throw new ArgumentNullException(nameof(timeController));
        }

        switch (action.Type)
        {
            case ActionType.SpendTime:
                ApplyTime(action.TimeCost, timeController);
                break;
            case ActionType.SetFlag:
                ApplyFlag(action.FlagId, action.FlagValue, worldState);
                break;
            case ActionType.ClearFlag:
                ApplyFlag(action.FlagId, false, worldState);
                break;
            case ActionType.RevealEntity:
                SetEntityKnowledge(action.EntityId, KnowledgeState.Revealed, locationState);
                break;
            case ActionType.HideEntity:
                SetEntityKnowledge(action.EntityId, KnowledgeState.Unknown, locationState);
                break;
            case ActionType.ResolveEntity:
                ResolveEntity(action.EntityId, locationState);
                break;
            case ActionType.MoveToLocation:
                MoveToLocation(action.TargetLocationId, action.TargetNodeId, worldState);
                break;
            case ActionType.MoveToNode:
                MoveToNode(action.TargetNodeId, worldState);
                break;
            case ActionType.ChangeSuspicion:
                worldState.SuspicionLevel += action.SuspicionDelta;
                break;
        }
    }

    private static void ApplyTime(int timeCost, ExplorationTimeController timeController)
    {
        if (timeCost <= 0)
        {
            return;
        }

        timeController.SpendUnits(timeCost);
    }

    private static void ApplyFlag(string flagId, bool value, WorldRuntimeState worldState)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            throw new InvalidOperationException("Flag action requires a non-empty flag id.");
        }

        worldState.Flags[flagId] = value;
    }

    private static void SetEntityKnowledge(
        string entityId,
        KnowledgeState knowledgeState,
        LocationRuntimeState locationState)
    {
        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new InvalidOperationException("Entity knowledge action requires a non-empty entity id.");
        }

        if (locationState.Entities.TryGetValue(entityId, out var entity))
        {
            entity.KnowledgeState = knowledgeState;
            return;
        }

        throw new InvalidOperationException($"Entity '{entityId}' is missing from the current location state.");
    }

    private static void ResolveEntity(string entityId, LocationRuntimeState locationState)
    {
        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new InvalidOperationException("Resolve entity action requires a non-empty entity id.");
        }

        if (locationState.Entities.TryGetValue(entityId, out var entity))
        {
            entity.KnowledgeState = KnowledgeState.Resolved;
            entity.IsConsumed = true;
            return;
        }

        throw new InvalidOperationException($"Entity '{entityId}' is missing from the current location state.");
    }

    private static void MoveToLocation(string locationId, string nodeId, WorldRuntimeState worldState)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            throw new InvalidOperationException("Move-to-location action requires a target location id.");
        }

        worldState.CurrentLocationId = locationId;

        MoveToNode(nodeId, worldState);
    }

    private static void MoveToNode(string nodeId, WorldRuntimeState worldState)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            throw new InvalidOperationException("Move-to-node action requires a target node id.");
        }

        worldState.CurrentNodeId = nodeId;
    }
}
}