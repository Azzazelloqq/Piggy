using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Game.Flow
{
    public interface IGameFlowService
    {
        int ActiveSlotIndex { get; }
        UniTask StartGameAsync(int slotIndex, CancellationToken cancellationToken = default);
    }
}
