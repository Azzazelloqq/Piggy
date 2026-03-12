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
        TimeService timeService)
    {
        if (actions == null || actions.Length == 0 || worldState == null)
        {
            return;
        }

        foreach (var action in actions)
        {
            Execute(action, worldState, locationState, timeService);
        }
    }

    public void Execute(
        ActionConfig action,
        WorldRuntimeState worldState,
        LocationRuntimeState locationState,
        TimeService timeService)
    {
        switch (action.Type)
        {
            case ActionType.SpendTime:
                ApplyTime(action.TimeCost, worldState, timeService);
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

    private static void ApplyTime(int timeCost, WorldRuntimeState worldState, TimeService timeService)
    {
        if (timeCost <= 0)
        {
            return;
        }

        worldState.CurrentTimeUnits += timeCost;
        timeService?.AddUnits(timeCost);
    }

    private static void ApplyFlag(string flagId, bool value, WorldRuntimeState worldState)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            return;
        }

        worldState.Flags[flagId] = value;
    }

    private static void SetEntityKnowledge(
        string entityId,
        KnowledgeState knowledgeState,
        LocationRuntimeState locationState)
    {
        if (locationState == null || string.IsNullOrWhiteSpace(entityId))
        {
            return;
        }

        if (locationState.Entities.TryGetValue(entityId, out var entity))
        {
            entity.KnowledgeState = knowledgeState;
        }
    }

    private static void ResolveEntity(string entityId, LocationRuntimeState locationState)
    {
        if (locationState == null || string.IsNullOrWhiteSpace(entityId))
        {
            return;
        }

        if (locationState.Entities.TryGetValue(entityId, out var entity))
        {
            entity.KnowledgeState = KnowledgeState.Resolved;
            entity.IsConsumed = true;
        }
    }

    private static void MoveToLocation(string locationId, string nodeId, WorldRuntimeState worldState)
    {
        if (!string.IsNullOrWhiteSpace(locationId))
        {
            worldState.CurrentLocationId = locationId;
        }

        MoveToNode(nodeId, worldState);
    }

    private static void MoveToNode(string nodeId, WorldRuntimeState worldState)
    {
        if (!string.IsNullOrWhiteSpace(nodeId))
        {
            worldState.CurrentNodeId = nodeId;
        }
    }
}
}