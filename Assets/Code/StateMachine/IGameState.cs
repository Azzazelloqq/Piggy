using System.Threading;
using Cysharp.Threading.Tasks;

namespace Piggy.Code.StateMachine
{
    internal interface IGameState
    {
        StateMachine SubStateMachine { get; }
        UniTask EnterAsync<T>(T gameStateContext, CancellationToken cancellationToken)
            where T : struct, IGameStateContext;
        UniTask ExitAsync(CancellationToken cancellationToken);
    }
}