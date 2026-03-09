namespace Code.Game.MainMenu.Window
{
public enum MainMenuScreenTransitionDirection
{
    Up,
    Down,
    Left,
    Right
}

public readonly struct MainMenuScreenTransitionEntry
{
    public MainMenuScreenTransitionEntry(
        MainMenuScreen screen,
        MainMenuScreenTransitionDirection enterDirection)
    {
        Screen = screen;
        EnterDirection = enterDirection;
    }

    public MainMenuScreen Screen { get; }
    public MainMenuScreenTransitionDirection EnterDirection { get; }
}
}
