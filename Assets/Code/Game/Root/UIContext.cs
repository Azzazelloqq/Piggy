using System;
using Disposable;
using UnityEngine;

namespace Code.Game.Root
{
[Serializable]
public struct UIContext
{
    [field: SerializeField]
    public RectTransform MainUIParent { get; private set; }
    
    [field: SerializeField]
    public RectTransform OvveridesParent { get; private set; }
    
    [field: SerializeField]
    public RectTransform HUDParent { get; private set; }
}
}