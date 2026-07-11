using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MenuRootVerticalBuilder
{
    private const string MenuRootPrefabPath = "Assets/NaninovelData/Resources/UI/MenuRoot.prefab";

    [MenuItem("Tools/MoshiReRe/Rebuild MenuRoot As Vertical UI")]
    public static void RebuildMenuRootAsVerticalUI()
    {
        var root = PrefabUtility.LoadPrefabContents(MenuRootPrefabPath);
        try
        {
            Rebuild(root);
            PrefabUtility.SaveAsPrefabAsset(root, MenuRootPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MenuRootVerticalBuilder] Rebuilt MenuRoot.prefab as a vertical layout.");
    }

    private static void Rebuild(GameObject root)
    {
        var rect = root.GetComponent<RectTransform>();
        var canvas = root.GetComponent<Canvas>();
        var scaler = root.GetComponent<CanvasScaler>();
        var canvasGroup = root.GetComponent<CanvasGroup>();
        var raycaster = root.GetComponent<GraphicRaycaster>();
        var menuRootUI = root.GetComponent<MenuRootUI>() ?? root.AddComponent<MenuRootUI>();

        ClearChildren(root.transform);
        ConfigureRoot(rect, canvas, scaler, canvasGroup, raycaster);

        var pageTop = CreateImageRoot("PageTop", rect, new Color(0.03f, 0.04f, 0.05f, 0.94f));
        Stretch(pageTop.rectTransform, Vector2.zero, Vector2.zero);
        BuildBackdrop(pageTop.rectTransform);

        var contentFrame = CreateImageRoot("ContentFrame", rect, new Color(0.08f, 0.09f, 0.11f, 0.97f));
        Stretch(contentFrame.rectTransform, new Vector2(52f, 170f), new Vector2(-52f, -68f));

        var tabsRoot = CreateTabsRoot(contentFrame.rectTransform);
        var statusButton = CreateTabButton("StatusTab", tabsRoot, "Status");
        var itemsButton = CreateTabButton("ItemsTab", tabsRoot, "Items");
        var charactersButton = CreateTabButton("CharactersTab", tabsRoot, "Characters");
        var mapButton = CreateTabButton("MapTab", tabsRoot, "Map");

        var pageArea = CreateRect("PageArea", contentFrame.rectTransform);
        Stretch(pageArea, new Vector2(28f, 148f), new Vector2(-28f, -140f));

        var statusPage = BuildStatusPage(pageArea);
        var inventoryPage = BuildInventoryPage(pageArea);
        var characterPage = BuildCharacterPage(pageArea);
        var mapPage = BuildMapPage(pageArea);
        var adviceTrigger = BuildMenuReRe(contentFrame.rectTransform);
        BuildWanderingReReSprite(contentFrame.rectTransform);
        ConfigureStatusPageParameterHub(statusPage, adviceTrigger);

        inventoryPage.gameObject.SetActive(false);
        characterPage.gameObject.SetActive(false);
        mapPage.gameObject.SetActive(false);

        ConfigureMenuRoot(menuRootUI, pageTop.gameObject, inventoryPage, characterPage, statusPage, mapPage,
            statusButton, itemsButton, charactersButton, mapButton, adviceTrigger);
    }

    private static void ConfigureRoot(RectTransform rect, Canvas canvas, CanvasScaler scaler, CanvasGroup canvasGroup, GraphicRaycaster raycaster)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(900f, 1600f);
        rect.localScale = Vector3.one;

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 1f;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        raycaster.ignoreReversedGraphics = true;
    }

    private static void BuildBackdrop(RectTransform parent)
    {
        var glow = CreateImageRoot("PhoneGlow", parent, new Color(0.17f, 0.41f, 0.37f, 0.18f));
        Stretch(glow.rectTransform, new Vector2(24f, 24f), new Vector2(-24f, -24f));

        var title = CreateTMPText("MenuTitle", parent, "MENU", 66f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Stretch(title.rectTransform, new Vector2(72f, 48f), new Vector2(-72f, -1490f));
        title.color = new Color(0.97f, 0.95f, 0.89f, 1f);

        var subtitle = CreateTMPText("MenuSubtitle", parent, "status / items / characters / map", 22f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        Stretch(subtitle.rectTransform, new Vector2(74f, 118f), new Vector2(-74f, -1438f));
        subtitle.color = new Color(0.73f, 0.80f, 0.78f, 1f);
    }

    private static StatusPage BuildStatusPage(RectTransform parent)
    {
        var page = CreateRect("PageStatus", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        var statusPage = page.gameObject.AddComponent<StatusPage>();

        var portraitCard = CreateImageRoot("PortraitCard", page, new Color(0.18f, 0.20f, 0.24f, 1f));
        SetRect(portraitCard.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -12f), new Vector2(250f, 390f));

        var portrait = CreateImageRoot("ProtagonistPortrait", portraitCard.rectTransform, new Color(0.82f, 0.84f, 0.88f, 1f));
        Stretch(portrait.rectTransform, new Vector2(16f, 16f), new Vector2(-16f, -16f));
        AttachStatusTarget(portrait.gameObject, MenuStatusAdviceType.Portrait);

        var portraitLabel = CreateTMPText("PortraitLabel", portrait.rectTransform, "DUMMY\nPORTRAIT", 34f, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(portraitLabel.rectTransform, Vector2.zero, Vector2.zero);
        portraitLabel.color = new Color(0.17f, 0.20f, 0.24f, 1f);
        portraitLabel.enableWordWrapping = false;

        var radarCard = CreateImageRoot("RadarCard", page, new Color(0.17f, 0.19f, 0.23f, 1f));
        SetRect(radarCard.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-12f, -12f), new Vector2(470f, 470f));

        var radar = CreateRect("StatusRadar", radarCard.rectTransform);
        Stretch(radar, new Vector2(46f, 46f), new Vector2(-46f, -46f));
        var radarChart = radar.gameObject.AddComponent<RadarChart>();
        radarChart.color = new Color(0.40f, 0.83f, 0.76f, 0.90f);

        var gutsText = CreateStatusVertex(radarCard.rectTransform, "GutsVertex", new Vector2(0.5f, 1f), new Vector2(0f, 36f), MenuStatusAdviceType.Guts);
        var intelligenceText = CreateStatusVertex(radarCard.rectTransform, "IntelligenceVertex", new Vector2(1f, 0.72f), new Vector2(72f, 0f), MenuStatusAdviceType.Intelligence);
        var attentionText = CreateStatusVertex(radarCard.rectTransform, "AttentionVertex", new Vector2(0.82f, 0f), new Vector2(66f, -26f), MenuStatusAdviceType.Attention);
        var techniqueText = CreateStatusVertex(radarCard.rectTransform, "TechniqueVertex", new Vector2(0.18f, 0f), new Vector2(-66f, -26f), MenuStatusAdviceType.Technique);
        var strengthText = CreateStatusVertex(radarCard.rectTransform, "StrengthVertex", new Vector2(0f, 0.72f), new Vector2(-72f, 0f), MenuStatusAdviceType.Strength);

        var infoCard = CreateImageRoot("StatusInfoCard", page, new Color(0.11f, 0.12f, 0.14f, 0.96f));
        SetRect(infoCard.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(-24f, 240f));
        infoCard.rectTransform.offsetMin = new Vector2(12f, 18f);
        infoCard.rectTransform.offsetMax = new Vector2(-12f, 258f);

        var infoHeader = CreateTMPText("StatusInfoHeader", infoCard.rectTransform, "ReRe Advice", 34f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Stretch(infoHeader.rectTransform, new Vector2(22f, 18f), new Vector2(-22f, -180f));
        infoHeader.color = new Color(0.97f, 0.95f, 0.89f, 1f);

        var infoBody = CreateTMPText("StatusInfoBody", infoCard.rectTransform, "各ステータス名や立ち絵をタップすると、ReRe が状態の見方を教えてくれる。", 24f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        Stretch(infoBody.rectTransform, new Vector2(22f, 72f), new Vector2(-22f, -20f));
        infoBody.color = new Color(0.81f, 0.84f, 0.88f, 1f);

        var hubCard = CreateImageRoot("ParameterHubCard", page, new Color(0.13f, 0.15f, 0.18f, 0.96f));
        Stretch(hubCard.rectTransform, new Vector2(12f, 500f), new Vector2(-12f, -288f));

        var hubTitle = CreateTMPText("ParameterHubTitle", hubCard.rectTransform, "PARAMETER HUB", 30f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Stretch(hubTitle.rectTransform, new Vector2(22f, 18f), new Vector2(-22f, -250f));
        hubTitle.color = new Color(0.95f, 0.94f, 0.88f, 1f);

        var shortcutRoot = CreateRect("ParameterShortcuts", hubCard.rectTransform);
        Stretch(shortcutRoot, new Vector2(22f, 72f), new Vector2(-22f, -162f));
        var shortcutLayout = shortcutRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        shortcutLayout.spacing = 14f;
        shortcutLayout.childControlWidth = true;
        shortcutLayout.childControlHeight = true;
        shortcutLayout.childForceExpandWidth = true;
        shortcutLayout.childForceExpandHeight = true;

        var itemsShortcut = CreateTabButton("ItemsShortcutButton", shortcutRoot, "ITEMS");
        var charactersShortcut = CreateTabButton("CharactersShortcutButton", shortcutRoot, "CHARACTERS");
        var mapShortcut = CreateTabButton("MapShortcutButton", shortcutRoot, "MAP");

        var iconStrip = CreateImageRoot("CharacterIconStrip", hubCard.rectTransform, new Color(0.09f, 0.10f, 0.12f, 0.88f));
        Stretch(iconStrip.rectTransform, new Vector2(22f, 156f), new Vector2(-22f, -22f));

        var iconRoot = CreateRect("StatusCharacterIconRoot", iconStrip.rectTransform);
        Stretch(iconRoot, new Vector2(14f, 12f), new Vector2(-14f, -12f));
        var iconLayout = iconRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        iconLayout.spacing = 12f;
        iconLayout.childControlWidth = false;
        iconLayout.childControlHeight = false;
        iconLayout.childForceExpandWidth = false;
        iconLayout.childForceExpandHeight = false;
        iconLayout.childAlignment = TextAnchor.MiddleLeft;

        var iconTemplate = CreateParameterCharacterIconTemplate("StatusCharacterIconTemplate", page);
        iconTemplate.SetActive(false);

        statusPage.Configure(radarChart, portrait.GetComponent<Image>(), gutsText, intelligenceText, attentionText, techniqueText, strengthText);
        statusPage.ConfigureParameterHub(itemsShortcut, charactersShortcut, mapShortcut, iconRoot, iconTemplate,
            FindFirstAssetOfType<CharacterDatabase>(), null);
        return statusPage;
    }

    private static InventoryPage BuildInventoryPage(RectTransform parent)
    {
        var page = CreateRect("PageItem", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        var inventoryPage = page.gameObject.AddComponent<InventoryPage>();

        var header = CreateTMPText("ItemsHeader", page, "ITEMS", 42f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Stretch(header.rectTransform, new Vector2(16f, 16f), new Vector2(-16f, -1000f));
        header.color = new Color(0.95f, 0.94f, 0.88f, 1f);

        var gridPanel = CreateImageRoot("ItemsPanel", page, new Color(0.11f, 0.12f, 0.14f, 0.96f));
        Stretch(gridPanel.rectTransform, new Vector2(16f, 92f), new Vector2(-16f, -16f));

        var scrollViewport = CreateImageRoot("ItemsViewport", gridPanel.rectTransform, new Color(0.15f, 0.17f, 0.20f, 1f));
        Stretch(scrollViewport.rectTransform, new Vector2(20f, 20f), new Vector2(-20f, -260f));

        var gridRoot = CreateRect("GridItemsRoot", scrollViewport.rectTransform);
        Stretch(gridRoot, new Vector2(16f, 16f), new Vector2(-16f, -16f));
        var layout = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(180f, 180f);
        layout.spacing = new Vector2(18f, 18f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;

        var detailPanel = CreateImageRoot("ItemDetailPanel", gridPanel.rectTransform, new Color(0.18f, 0.20f, 0.24f, 1f));
        SetRect(detailPanel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(-40f, 220f));
        detailPanel.rectTransform.offsetMin = new Vector2(20f, 20f);
        detailPanel.rectTransform.offsetMax = new Vector2(-20f, 240f);

        var detailImage = CreateImageRoot("ItemDetailImage", detailPanel.rectTransform, new Color(0.81f, 0.84f, 0.88f, 1f));
        SetRect(detailImage.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(140f, 140f));

        var title = CreateTMPText("ItemDetailTitle", detailPanel.rectTransform, "Select Item", 30f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Stretch(title.rectTransform, new Vector2(190f, 24f), new Vector2(-120f, -138f));
        title.color = new Color(0.95f, 0.94f, 0.88f, 1f);

        var description = CreateTMPText("ItemDetailDescription", detailPanel.rectTransform, "Details will appear here.", 24f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        Stretch(description.rectTransform, new Vector2(190f, 76f), new Vector2(-120f, -26f));
        description.color = new Color(0.80f, 0.83f, 0.87f, 1f);

        var closeButton = CreateActionButton("ItemDetailCloseButton", detailPanel.rectTransform, "Close");
        SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(110f, 52f));

        var cellTemplate = CreateCellTemplate("ItemCellTemplate", page, "ITEM");
        cellTemplate.SetActive(false);

        ConfigureInventoryPage(inventoryPage, gridRoot, cellTemplate, detailPanel.gameObject, detailImage.GetComponent<Image>(), title, description, closeButton);
        return inventoryPage;
    }

    private static CharacterPage BuildCharacterPage(RectTransform parent)
    {
        var page = CreateRect("PageCharacters", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        var characterPage = page.gameObject.AddComponent<CharacterPage>();

        var header = CreateTMPText("CharactersHeader", page, "CHARACTERS", 42f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Stretch(header.rectTransform, new Vector2(16f, 16f), new Vector2(-16f, -1000f));
        header.color = new Color(0.95f, 0.94f, 0.88f, 1f);

        var gridPanel = CreateImageRoot("CharactersPanel", page, new Color(0.11f, 0.12f, 0.14f, 0.96f));
        Stretch(gridPanel.rectTransform, new Vector2(16f, 92f), new Vector2(-16f, -16f));

        var scrollViewport = CreateImageRoot("CharactersViewport", gridPanel.rectTransform, new Color(0.15f, 0.17f, 0.20f, 1f));
        Stretch(scrollViewport.rectTransform, new Vector2(20f, 20f), new Vector2(-20f, -320f));

        var gridRoot = CreateRect("GridCharactersRoot", scrollViewport.rectTransform);
        Stretch(gridRoot, new Vector2(16f, 16f), new Vector2(-16f, -16f));
        var layout = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(180f, 220f);
        layout.spacing = new Vector2(18f, 18f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;

        var detailPanel = CreateImageRoot("CharacterDetailPanel", gridPanel.rectTransform, new Color(0.18f, 0.20f, 0.24f, 1f));
        SetRect(detailPanel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(-40f, 280f));
        detailPanel.rectTransform.offsetMin = new Vector2(20f, 20f);
        detailPanel.rectTransform.offsetMax = new Vector2(-20f, 300f);

        var portrait = CreateImageRoot("CharacterPortrait", detailPanel.rectTransform, new Color(0.81f, 0.84f, 0.88f, 1f));
        SetRect(portrait.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(170f, 220f));

        var nameText = CreateTMPText("CharacterNameText", detailPanel.rectTransform, "Select Character", 30f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Stretch(nameText.rectTransform, new Vector2(220f, 28f), new Vector2(-120f, -188f));
        nameText.color = new Color(0.95f, 0.94f, 0.88f, 1f);

        var description = CreateTMPText("CharacterDescriptionText", detailPanel.rectTransform, "Character details will appear here.", 24f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        Stretch(description.rectTransform, new Vector2(220f, 82f), new Vector2(-120f, -28f));
        description.color = new Color(0.80f, 0.83f, 0.87f, 1f);

        var closeButton = CreateActionButton("CharacterDetailCloseButton", detailPanel.rectTransform, "Close");
        SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(110f, 52f));

        var cellTemplate = CreateCharacterCellTemplate("CharacterCellTemplate", page);
        cellTemplate.SetActive(false);

        ConfigureCharacterPage(characterPage, gridRoot, cellTemplate, detailPanel.gameObject, portrait.GetComponent<Image>(), nameText, description, closeButton);
        return characterPage;
    }

    private static MapPage BuildMapPage(RectTransform parent)
    {
        var page = CreateRect("PageMap", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        var mapPage = page.gameObject.AddComponent<MapPage>();

        var mapCard = CreateImageRoot("MapCard", page, new Color(0.11f, 0.12f, 0.14f, 0.96f));
        Stretch(mapCard.rectTransform, new Vector2(16f, 16f), new Vector2(-16f, -16f));

        var title = CreateTMPText("MapHeader", mapCard.rectTransform, "MAP", 42f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Stretch(title.rectTransform, new Vector2(26f, 18f), new Vector2(-26f, -1260f));
        title.color = new Color(0.95f, 0.94f, 0.88f, 1f);

        var placeholder = CreateImageRoot("MapPlaceholder", mapCard.rectTransform, new Color(0.21f, 0.24f, 0.27f, 1f));
        Stretch(placeholder.rectTransform, new Vector2(26f, 92f), new Vector2(-26f, -26f));

        var label = CreateTMPText("MapPlaceholderLabel", placeholder.rectTransform, "MAP IMAGE PLACEHOLDER", 34f, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
        label.color = new Color(0.90f, 0.92f, 0.94f, 1f);
        label.enableWordWrapping = false;

        return mapPage;
    }

    private static AdviceClickTrigger BuildMenuReRe(RectTransform parent)
    {
        var root = CreateRect("MenuReRe", parent);
        SetRect(root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -26f), new Vector2(220f, 220f));

        var bubble = CreateImageRoot("AdviceBubble", root, new Color(0.14f, 0.16f, 0.20f, 0.97f));
        SetRect(bubble.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-10f, -120f), new Vector2(360f, 200f));

        var bubbleText = CreateTMPText("AdviceText", bubble.rectTransform, "", 22f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        Stretch(bubbleText.rectTransform, new Vector2(22f, 20f), new Vector2(-22f, -20f));
        bubbleText.color = new Color(0.95f, 0.96f, 0.98f, 1f);

        var bubbleGroup = bubble.gameObject.AddComponent<CanvasGroup>();
        var bubbleComponent = bubble.gameObject.AddComponent<AdviceBubble>();
        ConfigureAdviceBubble(bubbleComponent, bubbleGroup, bubble.rectTransform, bubbleText);

        var button = CreateActionButton("ReReButton", root, "ReRe");
        SetRect(button.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(140f, 72f));

        var trigger = button.gameObject.AddComponent<AdviceClickTrigger>();
        ConfigureAdviceTrigger(trigger, bubbleComponent, 6f);

        var advisor = root.gameObject.AddComponent<MenuReReAdvisor>();
        ConfigureMenuReReAdvisor(advisor, trigger, button);

        return trigger;
    }

    private static void BuildWanderingReReSprite(RectTransform parent)
    {
        var root = new GameObject("WanderingReReSprite", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MenuReReSpriteAnimator));
        root.transform.SetParent(parent, false);
        root.transform.SetAsLastSibling();

        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(150f, 430f);

        var image = root.GetComponent<Image>();
        image.sprite = GetDefaultSprite();
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.enabled = false;
    }

    private static TMP_Text CreateStatusVertex(RectTransform parent, string name, Vector2 anchor, Vector2 anchoredPosition, MenuStatusAdviceType adviceType)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MenuStatusAdviceTarget));
        root.transform.SetParent(parent, false);

        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(150f, 72f);
        rect.localScale = Vector3.one;

        var image = root.GetComponent<Image>();
        image.sprite = GetDefaultSprite();
        image.color = new Color(0f, 0f, 0f, 0.001f);

        AttachStatusTarget(root, adviceType);

        var text = CreateTMPText("Label", rect, "", 24f, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(text.rectTransform, Vector2.zero, Vector2.zero);
        text.color = new Color(0.95f, 0.94f, 0.88f, 1f);
        text.enableWordWrapping = false;
        return text;
    }

    private static void AttachStatusTarget(GameObject target, MenuStatusAdviceType adviceType)
    {
        var component = target.GetComponent<MenuStatusAdviceTarget>() ?? target.AddComponent<MenuStatusAdviceTarget>();
        var so = new SerializedObject(component);
        var property = so.FindProperty("adviceType");
        if (property != null) property.enumValueIndex = (int)adviceType;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(component);
    }

    private static GameObject CreateCellTemplate(string name, RectTransform parent, string label)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(180f, 180f);

        var image = root.GetComponent<Image>();
        image.sprite = GetDefaultSprite();
        image.color = new Color(0.32f, 0.35f, 0.40f, 1f);

        var icon = CreateImageRoot("Icon", rect, new Color(0.84f, 0.87f, 0.91f, 1f));
        Stretch(icon.rectTransform, new Vector2(16f, 16f), new Vector2(-16f, -54f));

        var text = CreateTMPText("Label", rect, label, 24f, FontStyles.Bold, TextAlignmentOptions.Bottom);
        Stretch(text.rectTransform, new Vector2(10f, 126f), new Vector2(-10f, -10f));
        text.color = Color.white;
        text.enableWordWrapping = false;

        return root;
    }

    private static GameObject CreateCharacterCellTemplate(string name, RectTransform parent)
    {
        var root = CreateCellTemplate(name, parent, "CHARACTER");
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(180f, 220f);
        return root;
    }

    private static GameObject CreateParameterCharacterIconTemplate(string name, RectTransform parent)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        root.transform.SetParent(parent, false);

        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(92f, 92f);

        var layout = root.GetComponent<LayoutElement>();
        layout.minWidth = 92f;
        layout.preferredWidth = 92f;
        layout.minHeight = 92f;
        layout.preferredHeight = 92f;

        var image = root.GetComponent<Image>();
        image.sprite = GetDefaultSprite();
        image.color = new Color(0.33f, 0.36f, 0.41f, 1f);

        var icon = CreateImageRoot("Icon", rect, new Color(0.84f, 0.87f, 0.91f, 1f));
        Stretch(icon.rectTransform, new Vector2(10f, 8f), new Vector2(-10f, -28f));

        var text = CreateTMPText("Label", rect, "", 16f, FontStyles.Bold, TextAlignmentOptions.Bottom);
        Stretch(text.rectTransform, new Vector2(6f, 64f), new Vector2(-6f, -4f));
        text.color = Color.white;
        text.enableWordWrapping = false;

        return root;
    }

    private static Button CreateTabButton(string name, RectTransform parent, string label)
    {
        var button = CreateActionButton(name, parent, label);
        var rect = button.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 88f);
        var layoutElement = button.gameObject.AddComponent<LayoutElement>();
        layoutElement.minWidth = 180f;
        layoutElement.preferredWidth = 180f;
        layoutElement.flexibleWidth = 1f;
        return button;
    }

    private static Button CreateActionButton(string name, RectTransform parent, string label)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);
        var image = root.GetComponent<Image>();
        image.sprite = GetDefaultSprite();
        image.color = new Color(0.19f, 0.21f, 0.25f, 1f);

        var button = root.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        colors.pressedColor = new Color(0.76f, 0.76f, 0.76f, 1f);
        button.colors = colors;

        var text = CreateTMPText("Label", root.GetComponent<RectTransform>(), label, 26f, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(text.rectTransform, Vector2.zero, Vector2.zero);
        text.color = new Color(0.95f, 0.94f, 0.88f, 1f);
        text.enableWordWrapping = false;

        return button;
    }

    private static RectTransform CreateTabsRoot(RectTransform parent)
    {
        var tabs = CreateRect("TopTabs", parent);
        tabs.anchorMin = new Vector2(0f, 1f);
        tabs.anchorMax = new Vector2(1f, 1f);
        tabs.pivot = new Vector2(0.5f, 1f);
        tabs.offsetMin = new Vector2(24f, -112f);
        tabs.offsetMax = new Vector2(-24f, -24f);
        tabs.localScale = Vector3.one;

        var layout = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.childAlignment = TextAnchor.MiddleCenter;

        return tabs;
    }

    private static void ConfigureMenuRoot(MenuRootUI menuRootUI, GameObject pageTop, InventoryPage inventoryPage, CharacterPage characterPage,
        StatusPage statusPage, MapPage mapPage, Button statusButton, Button itemsButton, Button charactersButton, Button mapButton,
        AdviceClickTrigger sharedAdviceTrigger)
    {
        var so = new SerializedObject(menuRootUI);
        SetObjectReference(so, "pageTop", pageTop);
        SetObjectReference(so, "pageItems", inventoryPage);
        SetObjectReference(so, "pageCharacters", characterPage);
        SetObjectReference(so, "pageStatus", statusPage);
        SetObjectReference(so, "pageMap", mapPage);
        SetObjectReference(so, "statusTabButton", statusButton);
        SetObjectReference(so, "itemsTabButton", itemsButton);
        SetObjectReference(so, "charactersTabButton", charactersButton);
        SetObjectReference(so, "mapTabButton", mapButton);
        SetObjectReference(so, "sharedAdviceTrigger", sharedAdviceTrigger);
        SetBool(so, "allowRuntimeFallbackBuild", false);
        SetBool(so, "keepPageTopAsBackground", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(menuRootUI);
    }

    private static void ConfigureStatusPageParameterHub(StatusPage statusPage, AdviceClickTrigger sharedAdviceTrigger)
    {
        var so = new SerializedObject(statusPage);
        SetObjectReference(so, "sharedAdviceTrigger", sharedAdviceTrigger);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(statusPage);
    }

    private static void ConfigureInventoryPage(InventoryPage page, Transform gridRoot, GameObject cellTemplate, GameObject detailPanel,
        Image detailImage, TMP_Text title, TMP_Text description, Button closeButton)
    {
        var so = new SerializedObject(page);
        SetObjectReference(so, "gridItemsRoot", gridRoot);
        SetObjectReference(so, "itemCellPrefab", cellTemplate);
        SetObjectReference(so, "itemDetailPanel", detailPanel);
        SetObjectReference(so, "itemDetailImage", detailImage);
        SetObjectReference(so, "itemDetailTitle", title);
        SetObjectReference(so, "itemDetailDescription", description);
        SetObjectReference(so, "itemDetailCloseButton", closeButton);
        SetObjectReference(so, "inventoryDB", FindFirstAssetOfType<InventoryDatabase>());
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(page);
    }

    private static void ConfigureCharacterPage(CharacterPage page, Transform gridRoot, GameObject cellTemplate, GameObject detailPanel,
        Image portraitImage, TMP_Text nameText, TMP_Text descriptionText, Button closeButton)
    {
        var so = new SerializedObject(page);
        SetObjectReference(so, "gridCharactersRoot", gridRoot);
        SetObjectReference(so, "characterCellPrefab", cellTemplate);
        SetObjectReference(so, "characterDetailPanel", detailPanel);
        SetObjectReference(so, "characterPortraitImage", portraitImage);
        SetObjectReference(so, "characterNameText", nameText);
        SetObjectReference(so, "characterDescriptionText", descriptionText);
        SetObjectReference(so, "characterDetailCloseButton", closeButton);
        SetObjectReference(so, "characterDB", FindFirstAssetOfType<CharacterDatabase>());
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(page);
    }

    private static void ConfigureAdviceBubble(AdviceBubble adviceBubble, CanvasGroup group, RectTransform bubbleRoot, TextMeshProUGUI text)
    {
        var so = new SerializedObject(adviceBubble);
        SetObjectReference(so, "canvasGroup", group);
        SetObjectReference(so, "bubbleRoot", bubbleRoot);
        SetObjectReference(so, "text", text);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(adviceBubble);
    }

    private static void ConfigureAdviceTrigger(AdviceClickTrigger trigger, AdviceBubble bubble, float delay)
    {
        var so = new SerializedObject(trigger);
        SetObjectReference(so, "adviceBubble", bubble);
        var property = so.FindProperty("autoHideDelay");
        if (property != null) property.floatValue = delay;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(trigger);
    }

    private static void ConfigureMenuReReAdvisor(MenuReReAdvisor advisor, AdviceClickTrigger trigger, Button button)
    {
        var so = new SerializedObject(advisor);
        SetObjectReference(so, "adviceTrigger", trigger);
        SetObjectReference(so, "reReButton", button);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(advisor);
    }

    private static T FindFirstAssetOfType<T>() where T : Object
    {
        var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private static void SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null) property.objectReferenceValue = value;
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null) property.boolValue = value;
    }

    private static Image CreateImageRoot(string name, RectTransform parent, Color color)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(parent, false);
        var image = root.GetComponent<Image>();
        image.sprite = GetDefaultSprite();
        image.color = color;
        return image;
    }

    private static RectTransform CreateRect(string name, RectTransform parent)
    {
        var root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(parent, false);
        return root.GetComponent<RectTransform>();
    }

    private static TextMeshProUGUI CreateTMPText(string name, RectTransform parent, string value, float size, FontStyles style, TextAlignmentOptions alignment)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        root.transform.SetParent(parent, false);
        var text = root.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.font = TMP_Settings.defaultFontAsset;
        return text;
    }

    private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        rect.localScale = Vector3.one;
    }

    private static Sprite GetDefaultSprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }
}

