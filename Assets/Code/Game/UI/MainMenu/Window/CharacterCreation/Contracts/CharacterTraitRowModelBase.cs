using Cysharp.Threading.Tasks;
using MVP;

namespace Code.Game.MainMenu.Window
{
public abstract class CharacterTraitRowModelBase : Model
{
    public abstract CharacterTraitRowData Data { get; }
    public abstract void SetData(CharacterTraitRowData data);
    public abstract UniTask RequestToggleAsync(bool isSelected);
}
}
