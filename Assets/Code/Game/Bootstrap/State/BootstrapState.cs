using System.Threading;
using Cysharp.Threading.Tasks;
using Piggy.Code.StateMachine;

namespace Code.Game.Bootstrap.State
{
    public sealed class BootstrapState : GameState
    {
        protected override async UniTask OnEnterAsync<TSceneContext>(
            TSceneContext gameStateContext,
            CancellationToken cancellationToken)
        {
            await base.OnEnterAsync(gameStateContext, cancellationToken);
            
            if (gameStateContext is not BootstrapStateContext bootstrapStateContext)
            {
                return;
            }
            
            
        }
    }
}
