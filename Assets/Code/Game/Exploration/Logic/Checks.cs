using Code.Config.Pages.Exploration;
using Code.Game.Exploration.Domain;
using UnityEngine;

namespace Code.Game.Exploration.Logic
{
public readonly struct CheckResult
{
    public CheckResult(InteractionResult result, int roll, int total, int dc)
    {
        Result = result;
        Roll = roll;
        Total = total;
        DC = dc;
    }

    public InteractionResult Result { get; }
    public int Roll { get; }
    public int Total { get; }
    public int DC { get; }
}

public sealed class D20CheckService
{
    private readonly int _partialSuccessMargin;

    public D20CheckService(int partialSuccessMargin = 2)
    {
        _partialSuccessMargin = Mathf.Max(0, partialSuccessMargin);
    }

    public CheckResult Roll(CheckConfig config, int statValue, int extraModifier = 0, int? forcedRoll = null)
    {
        var dc = config.DC;
        var modifiers = config.Modifier + extraModifier;
        return Roll(statValue, modifiers, dc, forcedRoll);
    }

    public CheckResult Roll(int statValue, int modifiers, int dc, int? forcedRoll = null)
    {
        var roll = forcedRoll ?? Random.Range(1, 21);
        var total = roll + statValue + modifiers;
        var result = ResolveResult(roll, total, dc);
        return new CheckResult(result, roll, total, dc);
    }

    private InteractionResult ResolveResult(int roll, int total, int dc)
    {
        if (roll >= 20)
        {
            return InteractionResult.CriticalSuccess;
        }

        if (roll <= 1)
        {
            return InteractionResult.CriticalFailure;
        }

        if (total >= dc)
        {
            return InteractionResult.Success;
        }

        if (total >= dc - _partialSuccessMargin)
        {
            return InteractionResult.PartialSuccess;
        }

        return InteractionResult.Failure;
    }
}
}