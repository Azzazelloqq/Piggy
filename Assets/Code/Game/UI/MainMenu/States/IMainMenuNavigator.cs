using Code.Game.MainMenu.Window;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.States
{
public interface IMainMenuNavigator
{
    UniTask NavigateAsync(MainMenuScreen screen);
}
}