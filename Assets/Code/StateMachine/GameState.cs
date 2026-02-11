using System.Threading;
using Cysharp.Threading.Tasks;

namespace Piggy.Code.StateMachine
{
    public abstract class GameState : IGameState
    {
        protected StateMachine SubStateMachine { get; } = new StateMachine(true);

        protected virtual UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken cancellationToken)
            where T : struct, IGameStateContext
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnExitAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        StateMachine IGameState.SubStateMachine => SubStateMachine;

        UniTask IGameState.EnterAsync<T>(T gameStateContext, CancellationToken cancellationToken)
        {
            return OnEnterAsync(gameStateContext, cancellationToken);
        }

        UniTask IGameState.ExitAsync(CancellationToken cancellationToken)
        {
            return OnExitAsync(cancellationToken);
        }
    }
}