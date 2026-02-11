using System.Threading;
using Cysharp.Threading.Tasks;
using Piggy.Code.StateMachine;

namespace Code.Game.MainMenu.States
{
    public sealed class MainMenuState : GameState
    {
        protected override UniTask OnEnterAsync<T>(T gameStateContext, CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }
    }
}
