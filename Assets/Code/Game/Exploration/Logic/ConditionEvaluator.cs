using Code.Config.Pages.Exploration;
using Code.Game.Exploration.Domain;
using Code.Game.Exploration.Runtime;

namespace Code.Game.Exploration.Logic
{
public sealed class ConditionEvaluator
{
    public bool AreMet(
        ConditionConfig[] conditions,
        WorldRuntimeState worldState,
        LocationRuntimeState locationState)
    {
        if (conditions == null || conditions.Length == 0)
        {
            return true;
        }

        foreach (var condition in conditions)
        {
            if (!IsMet(condition, worldState, locationState))
            {
                return false;
            }
        }

        return true;
    }

    public bool IsMet(
        ConditionConfig condition,
        WorldRuntimeState worldState,
        LocationRuntimeState locationState)
    {
        var met = condition.Type switch
        {
            ConditionType.HasFlag => ResolveFlag(condition, worldState, locationState),
            ConditionType.TimeRange => ResolveTimeRange(condition, worldState),
            ConditionType.KnowledgeState => ResolveKnowledgeState(condition, locationState),
            _ => false
        };

        return condition.Invert ? !met : met;
    }

    private static bool ResolveFlag(
        ConditionConfig condition,
        WorldRuntimeState worldState,
        LocationRuntimeState locationState)
    {
        if (string.IsNullOrWhiteSpace(condition.FlagId))
        {
            return false;
        }

        if (locationState != null && locationState.LocalFlags.TryGetValue(condition.FlagId, out var localValue))
        {
            return localValue == condition.FlagValue;
        }

        if (worldState != null && worldState.Flags.TryGetValue(condition.FlagId, out var worldValue))
        {
            return worldValue == condition.FlagValue;
        }

        return false;
    }

    private static bool ResolveTimeRange(ConditionConfig condition, WorldRuntimeState worldState)
    {
        if (worldState == null)
        {
            return false;
        }

        var time = worldState.CurrentTimeUnits;
        if (condition.MinTimeUnits > 0 && time < condition.MinTimeUnits)
        {
            return false;
        }

        if (condition.MaxTimeUnits > 0 && time > condition.MaxTimeUnits)
        {
            return false;
        }

        return true;
    }

    private static bool ResolveKnowledgeState(
        ConditionConfig condition,
        LocationRuntimeState locationState)
    {
        if (locationState == null || string.IsNullOrWhiteSpace(condition.EntityId))
        {
            return false;
        }

        if (!locationState.Entities.TryGetValue(condition.EntityId, out var entityState))
        {
            return false;
        }

        return entityState.KnowledgeState == condition.KnowledgeState;
    }
}
}