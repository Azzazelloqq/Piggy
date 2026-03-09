using System.Threading;
using Cysharp.Threading.Tasks;
using Piggy.Code.StateMachine;

namespace Code.Game.Bootstrap.State
{
    public sealed class BootstrapState : GameState
    {
        protected override async UniTask OnEnterAsync<TSceneContext>(
            TSceneContext gameStateContext,
            CancellationToken token)
        {
            await base.OnEnterAsync(gameStateContext, token);
            
            if (gameStateContext is not BootstrapStateContext bootstrapStateContext)
            {
                return;
            }
        }
    }
}
