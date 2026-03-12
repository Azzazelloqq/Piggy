using System;
using Code.Game.Exploration.Logic;

namespace Code.Game.Exploration.Runtime
{
public sealed class ExplorationRuntimeServices
{
    public ExplorationRuntimeServices(
        ActionExecutor actionExecutor,
        ConditionEvaluator conditionEvaluator,
        EventResolver eventResolver,
        D20CheckService checkService)
    {
        ActionExecutor = actionExecutor ?? throw new ArgumentNullException(nameof(actionExecutor));
        ConditionEvaluator = conditionEvaluator ?? throw new ArgumentNullException(nameof(conditionEvaluator));
        EventResolver = eventResolver ?? throw new ArgumentNullException(nameof(eventResolver));
        CheckService = checkService ?? throw new ArgumentNullException(nameof(checkService));
    }

    public ActionExecutor ActionExecutor { get; }
    public ConditionEvaluator ConditionEvaluator { get; }
    public EventResolver EventResolver { get; }
    public D20CheckService CheckService { get; }
}
}
