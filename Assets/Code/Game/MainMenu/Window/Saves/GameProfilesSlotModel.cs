using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Code.Game.MainMenu.Window
{
public sealed class GameProfilesSlotModel : GameProfilesSlotModelBase
{
    private GameProfilesSlotData _data;

    public override GameProfilesSlotData Data => _data;

    public override void SetData(GameProfilesSlotData data)
    {
        _data = data;
    }

    public override UniTask RequestSelectAsync()
    {
        return UniTask.CompletedTask;
    }

    protected override void OnInitialize()
    {
        _data = default;
    }

    protected override ValueTask OnInitializeAsync(CancellationToken token)
    {
        _data = default;
        return default;
    }

    protected override void OnDispose()
    {
    }

    protected override ValueTask OnDisposeAsync(CancellationToken token)
    {
        return default;
    }
}
}
