using System.Threading;
using Cysharp.Threading.Tasks;

namespace Piggy.Code.StateMachine
{
    /// <summary>
    /// Base class for all game states. Override <see cref="OnEnterAsync{T}"/> and
    /// <see cref="OnExitAsync"/> to implement state behavior.
    /// </summary>
    public abstract class GameState : IGameState
    {
        /// <summary>
        /// Gets the sub-state machine owned by this state.
        /// </summary>
        protected StateMachine SubStateMachine { get; } = new StateMachine(true);

        /// <summary>
        /// Called when the state is entered with a context value.
        /// </summary>
        /// <typeparam name="T">Context type.</typeparam>
        /// <param name="gameStateContext">Context value for the transition.</param>
        /// <param name="token">Token used to cancel the transition.</param>
        /// <returns>A task that completes when the enter logic finishes.</returns>
        protected virtual UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken token)
            where T : struct, IGameStateContext
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Called when the state is exiting.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the transition.</param>
        /// <returns>A task that completes when the exit logic finishes.</returns>
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