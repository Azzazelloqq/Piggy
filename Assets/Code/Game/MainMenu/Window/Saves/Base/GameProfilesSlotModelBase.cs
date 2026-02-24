using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class GameProfilesSlotModelBase : Model
{
    public abstract GameProfilesSlotData Data { get; }
    public abstract void SetData(GameProfilesSlotData data);
    public abstract UniTask RequestSelectAsync();
}
}
