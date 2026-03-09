using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class CharacterStatRowModelBase : Model
{
    public abstract CharacterStatRowData Data { get; }
    public abstract void SetData(CharacterStatRowData data);
    public abstract UniTask RequestIncreaseAsync();
    public abstract UniTask RequestDecreaseAsync();
}
}
