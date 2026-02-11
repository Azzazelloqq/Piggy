using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Code.Game.Root;
using Piggy.Code.StateMachine;

namespace Code.Game.Loading
{
    public readonly struct LoadingStateContext : IGameStateContext
    {
        public UIContext UIContext { get; }
        public Func<IProgress<float>, CancellationToken, UniTask> LoadOperation { get; }
        public int TotalSteps { get; }
        public Func<ILoadingStepReporter, CancellationToken, UniTask> StepsOperation { get; }

        public LoadingStateContext(
            UIContext uiContext,
            Func<IProgress<float>, CancellationToken, UniTask> loadOperation = null,
            int totalSteps = 0,
            Func<ILoadingStepReporter, CancellationToken, UniTask> stepsOperation = null)
        {
            UIContext = uiContext;
            LoadOperation = loadOperation;
            TotalSteps = totalSteps;
            StepsOperation = stepsOperation;
        }
    }
}
