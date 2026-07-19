using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Rebuilds the small HUD chrome while preserving gameplay component references.</summary>
public static class HudPrefabPolisher
{
    private const string LocationPath = "Assets/NaninovelData/Resources/UI/LocationHUD.prefab";
    private const string MoneyPath = "Assets/NaninovelData/Resources/UI/MoneyUI.prefab";
    private const string ReRePath = "Assets/NaninovelData/Resources/UI/ReReButton.prefab";
    private static readonly Color Navy = new Color32(8, 20, 44, 242);
    private static readonly Color Cyan = new Color32(66, 232, 255, 255);
    private static readonly Color Warm = new Color32(255, 116, 93, 255);

    [MenuItem("MoshiReRe/Rebuild HUD Prefabs")]
    public static void Rebuild()
    {
        RebuildLocation();
        RebuildMoney();
        RebuildReRe();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[HudPrefabPolisher] HUD prefabs rebuilt.");
    }

    private static void RebuildLocation()
    {
        var root = PrefabUtility.LoadPrefabContents(LocationPath);
        try
        {
            var label = root.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label) label.transform.SetParent(root.transform, false);
            Clear(root.transform, "HudDecor");
            var panel = Panel(root.transform, "HudDecor", new Vector2(24, -24), new Vector2(376, 78), TextAnchor.UpperLeft, Navy);
            var tab = Panel(panel.transform, "LocTab", new Vector2(14, -12), new Vector2(66, 24), TextAnchor.UpperLeft, Cyan);
            Text(tab.transform, "LOC", 14, new Color32(8, 20, 44, 255), TextAlignmentOptions.Center);
            var icon = Text(panel.transform, "\u25A6", 28, Cyan, TextAlignmentOptions.Center);
            Place(icon.rectTransform, new Vector2(86, -13), new Vector2(28, 34), TextAnchor.UpperLeft);
            if (label)
            {
                label.transform.SetParent(panel.transform, false);
                label.fontSize = 28;
                label.color = Color.white;
                label.alignment = TextAlignmentOptions.Left;
                Place(label.rectTransform, new Vector2(122, -17), new Vector2(240, 44), TextAnchor.UpperLeft);
            }
            Save(root, LocationPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void RebuildMoney()
    {
        var root = PrefabUtility.LoadPrefabContents(MoneyPath);
        try
        {
            var money = FindDeep(root.transform, "MoneyText").GetComponent<TextMeshProUGUI>();
            var floating = FindDeep(root.transform, "FloatingText");
            money.transform.SetParent(root.transform, false);
            floating.SetParent(root.transform, false);
            Clear(root.transform, "HudDecor");
            var panel = Panel(root.transform, "HudDecor", new Vector2(-24, -24), new Vector2(376, 84), TextAnchor.UpperRight, Navy);
            var tab = Panel(panel.transform, "WalletTab", new Vector2(-15, -12), new Vector2(96, 24), TextAnchor.UpperRight, Cyan);
            Text(tab.transform, "WALLET", 13, new Color32(8, 20, 44, 255), TextAlignmentOptions.Center);
            money.transform.SetParent(panel.transform, false);
            money.fontSize = 30;
            money.color = Color.white;
            money.alignment = TextAlignmentOptions.Center;
            Place(money.rectTransform, new Vector2(-18, -31), new Vector2(340, 38), TextAnchor.UpperRight);
            floating.SetParent(panel.transform, false);
            Place(floating.GetComponent<RectTransform>(), new Vector2(-18, -68), new Vector2(340, 28), TextAnchor.UpperRight);
            Save(root, MoneyPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void RebuildReRe()
    {
        var root = PrefabUtility.LoadPrefabContents(ReRePath);
        try
        {
            var screen = FindDeep(root.transform, "ReReButtonImage");
            var marker = FindDeep(root.transform, "MarkerImage");
            var advice = FindDeep(root.transform, "AdviceBubble");
            screen.SetParent(root.transform, false);
            marker.SetParent(root.transform, false);
            advice.SetParent(root.transform, false);
            Clear(root.transform, "PhoneShell");
            var shell = Panel(root.transform, "PhoneShell", new Vector2(-28, 28), new Vector2(174, 250), TextAnchor.LowerRight, Cyan);
            var body = Panel(shell.transform, "PhoneBody", new Vector2(5, -5), new Vector2(164, 240), TextAnchor.UpperLeft, Navy);
            Panel(body.transform, "Speaker", new Vector2(57, -10), new Vector2(50, 5), TextAnchor.UpperLeft, Cyan);
            screen.SetParent(body.transform, false);
            Place(screen.GetComponent<RectTransform>(), new Vector2(12, -28), new Vector2(140, 176), TextAnchor.UpperLeft);
            var idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/ReReSprites/rere_chibi_idle.png");
            var hoverSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UIConcepts/Common/Cropped/rere_happy.png");
            var reactionSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UIConcepts/Common/Cropped/rere_idea.png");
            var screenImage = screen.GetComponent<Image>();
            screenImage.sprite = idleSprite;
            screenImage.color = Color.white;
            marker.SetParent(shell.transform, false);
            var markerImage = marker.GetComponent<Image>();
            markerImage.sprite = BuiltinSprite();
            markerImage.color = Warm;
            markerImage.type = Image.Type.Sliced;
            Place(markerImage.rectTransform, new Vector2(-6, -6), new Vector2(34, 34), TextAnchor.UpperRight);
            Clear(marker, "BadgeText");
            var badgeText = Text(marker, "!", 22, Color.white, TextAlignmentOptions.Center);
            badgeText.name = "BadgeText";
            Stretch(badgeText.rectTransform);
            advice.SetParent(root.transform, false);
            Place(advice.GetComponent<RectTransform>(), new Vector2(-212, 32), new Vector2(174, 110), TextAnchor.LowerRight);

            var controller = root.GetComponent<ReReButtonController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("screenRect").objectReferenceValue = screen.GetComponent<RectTransform>();
            serialized.FindProperty("reactionSprite").objectReferenceValue = reactionSprite;
            serialized.FindProperty("notificationSprite").objectReferenceValue = BuiltinSprite();
            serialized.FindProperty("hoverSprite").objectReferenceValue = hoverSprite;
            var idleFrames = serialized.FindProperty("idleFrames");
            idleFrames.arraySize = 1;
            idleFrames.GetArrayElementAtIndex(0).objectReferenceValue = idleSprite;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Save(root, ReRePath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static Image Panel(Transform parent, string name, Vector2 position, Vector2 size, TextAnchor anchor, Color color)
    {
        var child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        child.transform.SetParent(parent, false);
        var image = child.GetComponent<Image>();
        image.sprite = BuiltinSprite();
        image.type = Image.Type.Sliced;
        image.color = color;
        Place(child.GetComponent<RectTransform>(), position, size, anchor);
        return image;
    }

    private static TextMeshProUGUI Text(Transform parent, string value, float size, Color color, TextAlignmentOptions alignment)
    {
        var child = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        child.transform.SetParent(parent, false);
        var text = child.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        Stretch(text.rectTransform);
        return text;
    }

    private static void Place(RectTransform rect, Vector2 position, Vector2 size, TextAnchor anchor)
    {
        Vector2 pivot;
        switch (anchor)
        {
            case TextAnchor.UpperLeft: pivot = new Vector2(0, 1); break;
            case TextAnchor.UpperRight: pivot = new Vector2(1, 1); break;
            case TextAnchor.LowerRight: pivot = new Vector2(1, 0); break;
            default: pivot = new Vector2(0, 0); break;
        }
        rect.anchorMin = pivot;
        rect.anchorMax = pivot;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite BuiltinSprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    private static void Clear(Transform parent, string childName)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child.name == childName) Object.DestroyImmediate(child.gameObject);
        }
    }

    private static Transform FindDeep(Transform parent, string childName)
    {
        foreach (var child in parent.GetComponentsInChildren<Transform>(true))
            if (child.name == childName) return child;
        return null;
    }

    private static void Save(GameObject root, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(root, path);
    }
}
