using System;
using Code.Game.Exploration.Runtime;
using UnityEngine;

namespace Code.Game.Exploration.View
{
public sealed class ExplorationTimeOverlayView : MonoBehaviour
{
    [SerializeField]
    private Vector2 _screenOffset = new(16f, 16f);

    [SerializeField]
    private Vector2 _panelSize = new(260f, 42f);

    [SerializeField]
    private int _fontSize = 18;

    [SerializeField]
    private Color _textColor = Color.white;

    [SerializeField]
    private Color _backgroundColor = new(0f, 0f, 0f, 0.72f);

    private TimeService _timeService;
    private ExplorationTimeController _timeController;
    private string _cachedText = string.Empty;
    private GUIStyle _backgroundStyle;
    private GUIStyle _labelStyle;

    public void Bind(TimeService timeService, ExplorationTimeController timeController)
    {
        _timeService = timeService ?? throw new ArgumentNullException(nameof(timeService));

        if (_timeController != null)
        {
            _timeController.TimeAdvanced -= RefreshText;
        }

        _timeController = timeController ?? throw new ArgumentNullException(nameof(timeController));
        _timeController.TimeAdvanced += RefreshText;
        RefreshText();
    }

    public void Unbind()
    {
        if (_timeController != null)
        {
            _timeController.TimeAdvanced -= RefreshText;
        }

        _timeController = null;
        _timeService = null;
        _cachedText = string.Empty;
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void OnGUI()
    {
        if (_timeService == null)
        {
            return;
        }

        EnsureStyles();

        var rect = new Rect(_screenOffset.x, _screenOffset.y, _panelSize.x, _panelSize.y);
        var previousColor = GUI.color;
        GUI.color = _backgroundColor;
        GUI.Box(rect, GUIContent.none, _backgroundStyle);
        GUI.color = previousColor;

        GUI.Label(rect, _cachedText, _labelStyle);
    }

    private void RefreshText()
    {
        if (_timeService == null)
        {
            _cachedText = string.Empty;
            return;
        }

        _cachedText = $"Time: {_timeService.FormatCurrentTime()}";
    }

    private void EnsureStyles()
    {
        if (_backgroundStyle == null)
        {
            _backgroundStyle = new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = Texture2D.whiteTexture
                }
            };
        }

        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = _fontSize,
                padding = new RectOffset(12, 12, 8, 8)
            };
            _labelStyle.normal.textColor = _textColor;
        }
    }
}
}
