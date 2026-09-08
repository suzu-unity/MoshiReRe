using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static partial class MenuRootV2Builder
{
    private static readonly Color SuppliedTextColor = new Color(.035f, .035f, .34f, 1f);
    private const string SuppliedPrefabFolder = "Assets/Prefabs/Menu/Supplied";

    private static GameObject BuildMenuRoot()
    {
        SuppliedMenuAssetLibrary.Import();
        Directory.CreateDirectory(SuppliedPrefabFolder);
        var root = new GameObject("MenuRootV2", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(MenuRootV2UI), typeof(MenuRootV2OrientationTransition), typeof(MenuPhoneNavigation));
        var rootRect = (RectTransform)root.transform;
        Stretch(rootRect, Vector2.zero, Vector2.zero);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 210;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;
        var shade = ImageRoot("SceneDim", rootRect, new Color(0, 0, 0, .12f));
        Stretch(shade.rectTransform, Vector2.zero, Vector2.zero);
        shade.raycastTarget = false;

        var landscape = RectRoot("SharedLandscapeShell", rootRect);
        SetRect(landscape, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, -20), new Vector2(1672, 941));
        var ui = root.GetComponent<MenuRootV2UI>();
        var actions = new UnityAction[] { ui.ShowTop, ui.ShowStatus, ui.ShowItems, ui.ShowCharacters, ui.ShowQuest, ui.ShowMap, ui.ShowSave, ui.ShowSettings };
        var home = BuildSuppliedHome(rootRect, actions, out var portrait);
        var pages = new[] { home, SuppliedDress(landscape), SuppliedItems(landscape), SuppliedCharacters(landscape), SuppliedQuest(landscape), SuppliedMap(landscape), SuppliedSave(landscape), SuppliedSettings(landscape) };
        var pageFields = new[] { "pageTop", "pageStatus", "pageItems", "pageCharacters", "pageQuest", "pageMap", "pageSave", "pageSettings" };
        var so = new SerializedObject(ui);
        SetObject(so, "standardPhoneLayer", landscape.gameObject);
        SetObject(so, "orientationTransition", root.GetComponent<MenuRootV2OrientationTransition>());
        for (var i = 0; i < pages.Length; i++) { SetObject(so, pageFields[i], pages[i].gameObject); pages[i].gameObject.SetActive(i == 0); }
        so.ApplyModifiedPropertiesWithoutUndo();
        BuildSuppliedNavigation(root, landscape, actions);
        BuildSuppliedLandscapeHud(landscape);
        Wire(root.AddComponent<MenuDaySaveBridge>(), ("dayHud", landscape.GetComponent<MenuTopHudState>()));
        ConfigureOrientation(root.GetComponent<MenuRootV2OrientationTransition>(), portrait, landscape,
            pages[1].gameObject, pages[2].gameObject, pages[3].gameObject, pages[4].gameObject, pages[5].gameObject, pages[6].gameObject, pages[7].gameObject);
        landscape.gameObject.SetActive(false);
        return root;
    }

    private static Image SuppliedImage(Transform parent, string name, Sprite sprite, Rect rect)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = sprite ? Color.white : Color.clear;
        image.preserveAspect = true;
        image.raycastTarget = false;
        Place(go.transform, rect.x, rect.y, rect.width, rect.height);
        return image;
    }

    private static TextMeshProUGUI SuppliedLabel(Transform parent, string name, string value, Rect rect, float size = 26, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        Place(go.transform, rect.x, rect.y, rect.width, rect.height);
        var label = go.GetComponent<TextMeshProUGUI>();
        label.font = FindPixelFontAsset();
        label.fontSize = size;
        label.color = SuppliedTextColor;
        label.text = value;
        label.alignment = alignment;
        label.raycastTarget = false;
        return label;
    }

    private static Button SuppliedButton(Transform parent, string name, Sprite sprite, Rect rect, UnityAction click = null, Sprite hover = null)
    {
        var image = SuppliedImage(parent, name, sprite, rect);
        image.raycastTarget = true;
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        if (hover)
        {
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState { highlightedSprite = hover, pressedSprite = hover, selectedSprite = hover, disabledSprite = sprite };
        }
        var feedback = new SerializedObject(image.gameObject.AddComponent<MenuUIButtonHover>());
        SetFloat(feedback, "hoverScale", 1.025f);
        SetFloat(feedback, "pressScale", .98f);
        feedback.ApplyModifiedPropertiesWithoutUndo();
        if (click != null) UnityEventTools.AddPersistentListener(button.onClick, click);
        return button;
    }

    private static RectTransform SuppliedPage(Transform parent, string name, string background)
    {
        var page = RectRoot(name, (RectTransform)parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        var maskRoot = RectRoot("PhoneArtworkClip", page);
        Stretch(maskRoot, Vector2.zero, Vector2.zero);
        maskRoot.gameObject.AddComponent<MenuPhoneArtworkMask>().raycastTarget = false;
        maskRoot.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        SuppliedImage(maskRoot, "SuppliedBackground", SuppliedMenuAssetLibrary.Get(background), new Rect(0, 0, 1672, 941));
        return page;
    }

    private static Image SuppliedEmptyPanel(Transform parent, string name, Rect rect)
    {
        var sprite = SuppliedMenuAssetLibrary.Slice("settings_final", "empty_panel_sliced", new Rect(1030, 825, 224, 97), new Vector4(14, 14, 47, 18));
        var panel = SuppliedImage(parent, name, sprite, rect);
        panel.type = Image.Type.Sliced;
        return panel;
    }

    private static void Wire(UnityEngine.Object component, params (string field, UnityEngine.Object value)[] fields)
    {
        var so = new SerializedObject(component);
        foreach (var field in fields) SetObject(so, field.field, field.value);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireArray(UnityEngine.Object component, string name, UnityEngine.Object[] values)
    {
        var so = new SerializedObject(component);
        var array = so.FindProperty(name);
        array.arraySize = values.Length;
        for (var i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildSuppliedNavigation(GameObject root, RectTransform parent, UnityAction[] actions)
    {
        var nav = RectRoot("PersistentNav", parent);
        Stretch(nav, Vector2.zero, Vector2.zero);
        var navClip=RectRoot("NavigationArtworkClip", nav); Stretch(navClip, Vector2.zero, Vector2.zero);
        navClip.gameObject.AddComponent<MenuPhoneArtworkMask>().raycastTarget=false;
        navClip.gameObject.AddComponent<Mask>().showMaskGraphic=false;
        SuppliedImage(navClip, "NavigationSurface", SuppliedMenuAssetLibrary.Slice("quest_background", "common_navigation_artwork", new Rect(42, 78, 200, 790)), new Rect(42, 78, 200, 790));
        var ids = new[] { "home", "dress", "items", "characters", "quest", "map", "save", "settings" };
        var labels = new[] { "", "着替え", "持ち物", "人物", "クエスト", "マップ", "", "" };
        var fields = new[] { "topButton", "statusButton", "itemsButton", "charactersButton", "questButton", "mapButton", "saveButton", "settingsButton" };
        var icons = new Image[8]; var expanded = new GameObject[8]; var marks = new GameObject[8];
        for (var i = 0; i < 8; i++)
        {
            var y = 158 + i * 78;
            var labelRoot = SuppliedImage(nav, "SelectedLabel" + i, SuppliedMenuAssetLibrary.Get("nav.selected"), new Rect(82, y, 165, 78));
            SuppliedLabel(labelRoot.transform, "PageName", labels[i], new Rect(66, 16, 97, 46), 24, TextAlignmentOptions.Center);
            expanded[i] = labelRoot.gameObject;
            var button = SuppliedButton(nav, char.ToUpper(fields[i][0]) + fields[i].Substring(1), SuppliedMenuAssetLibrary.Get("nav." + ids[i]), new Rect(88, y, 84, 78));
            icons[i] = button.image;
            marks[i] = RectRoot("MenuPageActiveMark", (RectTransform)button.transform).gameObject;
            Wire(root.GetComponent<MenuRootV2UI>(), (fields[i], button));
        }
        var navigation = root.GetComponent<MenuPhoneNavigation>();
        WireArray(navigation, "icons", icons);
        WireArray(navigation, "selectedLabels", expanded);
        WireArray(navigation, "selectedMarks", marks);
        navigation.Select(0);
    }

    private static void BuildSuppliedLandscapeHud(RectTransform parent)
    {
        SuppliedImage(parent, "DayFrame", SuppliedMenuAssetLibrary.Slice("quest_background", "day_frame", new Rect(823, 89, 188, 74)), new Rect(823, 89, 188, 74));
        SuppliedImage(parent, "DebtFrame", SuppliedMenuAssetLibrary.Slice("quest_background", "debt_frame", new Rect(1031, 89, 220, 74)), new Rect(1031, 89, 220, 74));
        SuppliedImage(parent, "MoneyFrame", SuppliedMenuAssetLibrary.Slice("quest_background", "money_frame", new Rect(1269, 89, 264, 74)), new Rect(1269, 89, 264, 74));
        var day = SuppliedLabel(parent, "DayValue", "DAY\n03", new Rect(830, 96, 170, 64), 27, TextAlignmentOptions.Center);
        var deadline = SuppliedLabel(parent, "DebtValue", "返済期限\nあと7日", new Rect(1040, 96, 200, 64), 27, TextAlignmentOptions.Center);
        var money = SuppliedLabel(parent, "MoneyValue", "¥ 000,000", new Rect(1276, 96, 248, 64), 27, TextAlignmentOptions.Center);
        Wire(parent.gameObject.AddComponent<MenuTopHudState>(), ("dayText", day), ("debtDaysText", deadline));
        Wire(money.gameObject.AddComponent<MoneyUI>(), ("moneyText", money));
    }
}
