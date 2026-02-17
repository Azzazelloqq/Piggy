using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.MainMenu.Window
{
    public sealed class MainMenuScreenTransitionView
    {
        public MainMenuScreenTransitionView(
            MainMenuViewBase.LayoutData layout,
            PanelHandle menuPanel,
            PanelHandle settingsPanel,
            PanelHandle exitPanel)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
            MenuPanel = menuPanel;
            SettingsPanel = settingsPanel;
            ExitPanel = exitPanel;
        }

        public MainMenuViewBase.LayoutData Layout { get; }
        public PanelHandle MenuPanel { get; }
        public PanelHandle SettingsPanel { get; }
        public PanelHandle ExitPanel { get; }

        public readonly struct PanelHandle
        {
            public PanelHandle(
                RectTransform panel,
                Action show,
                Action hide,
                Action<bool> setInteractable,
                IReadOnlyList<RectTransform> elements)
            {
                Panel = panel;
                _show = show;
                _hide = hide;
                _setInteractable = setInteractable;
                Elements = elements;
            }

            public RectTransform Panel { get; }
            public IReadOnlyList<RectTransform> Elements { get; }
            private readonly Action _show;
            private readonly Action _hide;
            private readonly Action<bool> _setInteractable;

            public void Show()
            {
                _show?.Invoke();
            }

            public void Hide()
            {
                _hide?.Invoke();
            }

            public void SetInteractable(bool isInteractable)
            {
                _setInteractable?.Invoke(isInteractable);
            }
        }
    }
}
