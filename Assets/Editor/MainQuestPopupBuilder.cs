using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MainQuestPopupBuilder
{
    private const string PrefabPath = "Assets/NaninovelData/Resources/UI/MainQuestPopup.prefab";
    private const string FontPath = "Assets/Font/PixelMplus12-Regular SDF.asset";
    private static readonly Color Ink = new Color(0.14f, 0.09f, 0.20f, 0.94f);
    private static readonly Color Lavender = new Color(0.45f, 0.32f, 0.65f, 0.92f);
    private static readonly Color Window = new Color(0.08f, 0.06f, 0.14f, 0.84f);

    [MenuItem("Tools/MoshiReRe/Build Main Quest Popup Prefab")]
    public static void BuildPrefab()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (!font) throw new InvalidOperationException($"Pixel font was not found: {FontPath}");

        var root = new GameObject("MainQuestPopup", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(MainQuestPopup));
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        var frame = Image("PixelFrame", root.transform, Ink);
        SetTopLeft(frame.rectTransform, new Vector2(-650f, -120f), new Vector2(590f, 210f));
        var panel = Image("Panel", frame.transform, Window);
        Stretch(panel.rectTransform, new Vector2(5f, 5f), new Vector2(-5f, -5f));
        var accent = Image("Accent", panel.transform, Lavender);
        SetTopLeft(accent.rectTransform, new Vector2(14f, -14f), new Vector2(8f, 168f));
        var label = Text("NEW MAIN QUEST", panel.transform, font, 14f, FontStyles.Bold, Lavender, TextAlignmentOptions.Left);
        SetTopLeft(label.rectTransform, new Vector2(42f, -14f), new Vector2(510f, 24f));
        var title = Text("", panel.transform, font, 30f, FontStyles.Bold, Color.white, TextAlignmentOptions.Left);
        SetTopLeft(title.rectTransform, new Vector2(42f, -46f), new Vector2(510f, 44f));
        var objective = Text("", panel.transform, font, 20f, FontStyles.Normal, new Color(0.9f, 0.86f, 0.98f, 1f), TextAlignmentOptions.Left);
        SetTopLeft(objective.rectTransform, new Vector2(42f, -94f), new Vector2(510f, 34f));
        var deadline = Text("", panel.transform, font, 24f, FontStyles.Bold, Color.white, TextAlignmentOptions.Left);
        SetTopLeft(deadline.rectTransform, new Vector2(42f, -136f), new Vector2(510f, 32f));

        var serialized = new SerializedObject(root.GetComponent<MainQuestPopup>());
        SetObject(serialized, "canvasGroup", root.GetComponent<CanvasGroup>());
        SetObject(serialized, "popupRect", frame.rectTransform);
        SetObject(serialized, "titleText", title);
        SetObject(serialized, "objectiveText", objective);
        SetObject(serialized, "deadlineText", deadline);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log("[MainQuestPopupBuilder] Built MainQuestPopup prefab.");
    }

    private static Image Image(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI Text(string value, Transform parent, TMP_FontAsset font, float size, FontStyles style, Color color, TextAlignmentOptions alignment)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }

    private static void SetTopLeft(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = min;
        rect.offsetMax = max;
    }

    private static void SetObject(SerializedObject serialized, string propertyName, UnityEngine.Object value)
    {
        var property = serialized.FindProperty(propertyName);
        if (property != null) property.objectReferenceValue = value;
    }
}
