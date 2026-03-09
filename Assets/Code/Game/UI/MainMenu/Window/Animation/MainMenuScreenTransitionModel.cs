using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
public sealed class MainMenuScreenTransitionModel
{
    public Vector2 MenuShownPosition { get; set; }
    public Vector2 SettingsShownPosition { get; set; }
    public Vector2 ExitShownPosition { get; set; }
    public Vector2 SavesShownPosition { get; set; }
    public Vector2 CharacterCreationShownPosition { get; set; }
    public bool LayoutCaptured { get; set; }
    public bool IsTransitioning { get; set; }
    public MainMenuScreen CurrentScreen { get; set; }
    public List<MainMenuScreenTransitionEntry> ScreenStack { get; } = new();
}
}