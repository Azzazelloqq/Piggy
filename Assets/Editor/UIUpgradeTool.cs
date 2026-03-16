using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Code.UI.Animations;
using TMPro;

public class UIUpgradeTool : EditorWindow
{
    [MenuItem("Tools/Upgrade UI Prefabs")]
    public static void UpgradeUIPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Content/UI" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UpgradePrefab(path);
        }
        
        Debug.Log("UI Upgrade Complete for all prefabs in Assets/Content/UI!");
    }

    private static void UpgradePrefab(string path)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
        if (prefabRoot == null)
        {
            Debug.LogError($"Could not load prefab at {path}");
            return;
        }

        bool modified = false;

        // 1. Ensure CanvasScaler if there is a Canvas (Usually these are just panels, but let's check)
        Canvas canvas = prefabRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            CanvasScaler scaler = prefabRoot.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = prefabRoot.AddComponent<CanvasScaler>();
                modified = true;
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            modified = true;
        }

        // 2. Upgrade Layout Groups
        VerticalLayoutGroup[] verticalGroups = prefabRoot.GetComponentsInChildren<VerticalLayoutGroup>(true);
        foreach (var vlg in verticalGroups)
        {
            if (vlg.spacing == 0)
            {
                vlg.spacing = 20f; // Add some breathing room
                modified = true;
            }
            // Ensure child alignment is sensible
            if (vlg.childAlignment == TextAnchor.UpperLeft)
            {
                vlg.childAlignment = TextAnchor.UpperCenter;
                modified = true;
            }
        }

        HorizontalLayoutGroup[] horizontalGroups = prefabRoot.GetComponentsInChildren<HorizontalLayoutGroup>(true);
        foreach (var hlg in horizontalGroups)
        {
            if (hlg.spacing == 0)
            {
                hlg.spacing = 20f;
                modified = true;
            }
            if (hlg.childAlignment == TextAnchor.UpperLeft)
            {
                hlg.childAlignment = TextAnchor.MiddleCenter;
                modified = true;
            }
        }

        // 3. Add UIWindowAnimator to root or main panels ONLY if it's a Window/View
        bool isWindow = path.EndsWith("View.prefab") || path.EndsWith("Window.prefab") || canvas != null;
        if (isWindow)
        {
            CanvasGroup canvasGroup = prefabRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                // Try to find a main panel
                Transform background = prefabRoot.transform.Find("Background") ?? prefabRoot.transform.Find("MainMenuPanel");
                if (background != null)
                {
                    canvasGroup = background.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = background.gameObject.AddComponent<CanvasGroup>();
                        modified = true;
                    }
                    
                    UIWindowAnimator windowAnim = background.GetComponent<UIWindowAnimator>();
                    if (windowAnim == null)
                    {
                        windowAnim = background.gameObject.AddComponent<UIWindowAnimator>();
                        modified = true;
                    }
                }
            }
            else
            {
                UIWindowAnimator windowAnim = prefabRoot.GetComponent<UIWindowAnimator>();
                if (windowAnim == null)
                {
                    windowAnim = prefabRoot.AddComponent<UIWindowAnimator>();
                    modified = true;
                }
            }
        }

        // 4. Add UIButtonAnimator to all Buttons
        Button[] buttons = prefabRoot.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            UIButtonAnimator btnAnim = btn.GetComponent<UIButtonAnimator>();
            if (btnAnim == null)
            {
                btnAnim = btn.gameObject.AddComponent<UIButtonAnimator>();
                modified = true;
            }
            
            // Ensure button transition is set to None so it doesn't conflict with DOTween color/scale
            if (btn.transition != Selectable.Transition.None)
            {
                btn.transition = Selectable.Transition.None;
                modified = true;
            }
        }

        // 5. Polish TextMeshPro Fonts
        TextMeshProUGUI[] texts = prefabRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in texts)
        {
            // Set ink color
            txt.color = new Color(0.24f, 0.15f, 0.13f, 1f); // #3E2723
            
            if (!txt.richText)
            {
                txt.richText = true;
                modified = true;
            }
            
            // Example: Make sure title texts are prominent
            if (txt.gameObject.name.Contains("Title") || txt.fontSize > 40)
            {
                txt.fontStyle |= FontStyles.Bold;
                modified = true;
            }
        }

        if (modified)
        {
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            Debug.Log($"Upgraded {path}");
        }
        
        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }
}
