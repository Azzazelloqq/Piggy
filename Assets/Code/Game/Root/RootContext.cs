using System;
using UnityEngine;

namespace Code.Game.Root
{
[Serializable]
public struct RootContext
{
    [field: SerializeField]
    public UIContext UIContext { get; private set; }

    [field: SerializeField]
    public Transform GameplayRoot { get; private set; }

    [field: SerializeField]
    public Camera GameplayCamera { get; private set; }

    [field: SerializeField]
    public Camera UICamera { get; private set; }
}
}