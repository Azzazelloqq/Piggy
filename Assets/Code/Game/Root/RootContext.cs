using Disposable;
using UnityEngine;

namespace Code.Game.Root
{
public class RootContext : MonoBehaviourDisposable
{
    [field: SerializeField]
    public UIContext UIContext { get; private set; }
}
}