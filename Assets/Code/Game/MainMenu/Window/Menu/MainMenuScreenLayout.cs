namespace Code.Game.MainMenu.Window
{
public sealed partial class MainMenuPresenter
{
    private readonly struct MainMenuScreenLayout
    {
        public MainMenuScreenLayout(
            MainMenuPosition menu,
            MainMenuSettingsPosition settings,
            MainMenuExitConfirmPosition exit)
        {
            Menu = menu;
            Settings = settings;
            Exit = exit;
        }

        public MainMenuPosition Menu { get; }
        public MainMenuSettingsPosition Settings { get; }
        public MainMenuExitConfirmPosition Exit { get; }
    }
}
}