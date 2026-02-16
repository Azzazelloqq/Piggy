using Code.Game.MainMenu.Window;
using Code.Game.MainMenu.Window;
using Piggy.Code.StateMachine;

namespace Code.Game.MainMenu.States
{
    public readonly struct MainMenuSubStateContext : IGameStateContext
    {
        public MainMenuSubStateContext(MainMenuScreen screen)
        {
            Screen = screen;
        }

        public MainMenuScreen Screen { get; }
    }
}
