using System;
using UnityEngine;

namespace Code.Game.Root
{
[Serializable]
public struct RootContext
{
    [field: SerializeField]
    public UIContext UIContext { get; private set; }
}
}