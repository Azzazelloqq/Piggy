using System.Threading;
using Cysharp.Threading.Tasks;

namespace Piggy.Code.StateMachine
{
    /// <summary>
    /// Internal contract used by the state machine to enter and exit states.
    /// </summary>
    internal interface IGameState : IState
    {
        /// <summary>
        /// Gets the sub-state machine owned by the state.
        /// </summary>
        StateMachine SubStateMachine { get; }

        /// <summary>
        /// Enters the state with a typed context.
        /// </summary>
        UniTask EnterAsync<T>(T gameStateContext, CancellationToken cancellationToken)
            where T : struct, IGameStateContext;

        /// <summary>
        /// Exits the state.
        /// </summary>
        UniTask ExitAsync(CancellationToken cancellationToken);
    }
}