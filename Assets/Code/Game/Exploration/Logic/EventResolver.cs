using System;
using System.Collections.Generic;
using Code.Config.Pages.Exploration;
using Code.Game.Exploration.Domain;
using Code.Game.Exploration.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Game.Exploration.Logic
{
public sealed class EventResolver
{
    private readonly ConditionEvaluator _conditionEvaluator;

    public EventResolver(ConditionEvaluator conditionEvaluator)
    {
        _conditionEvaluator = conditionEvaluator ?? throw new ArgumentNullException(nameof(conditionEvaluator));
    }

    public bool TryResolve(
        EventTriggerType triggerType,
        EventConfig[] events,
        WorldRuntimeState worldState,
        LocationRuntimeState locationState,
        string nodeId,
        string entityId,
        out EventConfig resolved)
    {
        if (worldState == null)
        {
            throw new ArgumentNullException(nameof(worldState));
        }

        if (locationState == null)
        {
            throw new ArgumentNullException(nameof(locationState));
        }

        resolved = default;
        if (events == null || events.Length == 0)
        {
            return false;
        }

        var candidates = new List<EventConfig>();
        var highestPriority = int.MinValue;
        foreach (var eventConfig in events)
        {
            if (!IsCandidate(triggerType, eventConfig, worldState, locationState, nodeId, entityId))
            {
                continue;
            }

            if (eventConfig.Priority > highestPriority)
            {
                highestPriority = eventConfig.Priority;
                candidates.Clear();
            }

            if (eventConfig.Priority == highestPriority)
            {
                candidates.Add(eventConfig);
            }
        }

        if (candidates.Count == 0)
        {
            return false;
        }

        resolved = SelectByWeight(candidates);
        return true;
    }

    public void MarkTriggered(EventConfig eventConfig, WorldRuntimeState worldState, LocationRuntimeState locationState)
    {
        if (worldState == null)
        {
            throw new ArgumentNullException(nameof(worldState));
        }

        if (locationState == null)
        {
            throw new ArgumentNullException(nameof(locationState));
        }

        if (string.IsNullOrWhiteSpace(eventConfig.Id))
        {
            throw new InvalidOperationException("Event config must have a non-empty id.");
        }

        if (!locationState.Events.TryGetValue(eventConfig.Id, out var state))
        {
            state = new EventRuntimeState(eventConfig.Id);
            locationState.Events[eventConfig.Id] = state;
        }

        state.LastTriggeredTimeUnits = worldState.CurrentTimeUnits;
        state.TriggerCount += 1;
    }

    private bool IsCandidate(
        EventTriggerType triggerType,
        EventConfig eventConfig,
        WorldRuntimeState worldState,
        LocationRuntimeState locationState,
        string nodeId,
        string entityId)
    {
        if (string.IsNullOrWhiteSpace(eventConfig.Id))
        {
            throw new InvalidOperationException("Event config must have a non-empty id.");
        }

        if (eventConfig.Trigger != triggerType)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(eventConfig.TargetNodeId) &&
            !string.Equals(eventConfig.TargetNodeId, nodeId, StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(eventConfig.TargetEntityId) &&
            !string.Equals(eventConfig.TargetEntityId, entityId, StringComparison.Ordinal))
        {
            return false;
        }

        if (!_conditionEvaluator.AreMet(eventConfig.Conditions, worldState, locationState))
        {
            return false;
        }

        if (locationState.Events.TryGetValue(eventConfig.Id, out var state))
        {
            if (!eventConfig.IsRepeatable && state.TriggerCount > 0)
            {
                return false;
            }

            if (eventConfig.CooldownTimeUnits > 0 &&
                state.LastTriggeredTimeUnits >= 0 &&
                worldState.CurrentTimeUnits - state.LastTriggeredTimeUnits < eventConfig.CooldownTimeUnits)
            {
                return false;
            }
        }

        return true;
    }

    private static EventConfig SelectByWeight(IReadOnlyList<EventConfig> candidates)
    {
        var totalWeight = 0f;
        foreach (var candidate in candidates)
        {
            totalWeight += Mathf.Max(0.01f, candidate.Weight);
        }

        var roll = Random.value * totalWeight;
        var cumulative = 0f;
        foreach (var candidate in candidates)
        {
            cumulative += Mathf.Max(0.01f, candidate.Weight);
            if (roll <= cumulative)
            {
                return candidate;
            }
        }

        return candidates[candidates.Count - 1];
    }
}
}