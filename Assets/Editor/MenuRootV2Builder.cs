using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MenuRootV2Builder
{
    private const string PrefabPath = "Assets/NaninovelData/Resources/UI/MenuRootV2.prefab";
    private const string PreviewScenePath = "Assets/Scenes/MenuRootV2Preview.unity";
    private const string TopArtworkPath = "Assets/Art/UIConcepts/menu_top_phone_unified_v2.png";
    private const string CommonUiCropFolder = "Assets/Art/UIConcepts/Common/Cropped";
    private const string DressArtworkPath = "Assets/Art/UIConcepts/dress_room_phone_concept.png";
    private const string NeutralDressArtworkPath = "Assets/Art/UIConcepts/dress_room_phone_concept_neutral.png";
    private const string PixelFillSpritePath = "Assets/Art/UIConcepts/pixel_ui_fill.png";
    private const string ReReSpeechBubblePath = "Assets/Art/UIConcepts/rere_pixel_speech_bubble.png";
    private const string DressCommentBubblePath = "Assets/Art/UIConcepts/dress_comment_bubble_tail.png";
    private const string TopReReSpritePath = "Assets/Art/ReReSprites/rere_chibi_idle.png";
    private const string TopIdleVideoFolder = "Assets/Art/ReReSprites/TopIdleVideo";
    private const string TopPreviewBackgroundPath = "Assets/Art/ScenarioExploration/Backgrounds/11-4_office_night&light.png";
    private const string TopReReWalkFolder = "Assets/Art/ReReSprites/Actions/walk_right";
    private const string TopReReNoticeFolder = "Assets/Art/ReReSprites/Actions/notice_idle";
    private const string TopReReSitYawnFolder = "Assets/Art/ReReSprites/Actions/sit_yawn";
    private const string TopReReStretchFolder = "Assets/Art/ReReSprites/Actions/stretch";
    private const string TopReReReadBookFolder = "Assets/Art/ReReSprites/Actions/read_book";
    private const string TopReReUsePhoneFolder = "Assets/Art/ReReSprites/Actions/use_phone";
    private const string TopReReWaveFolder = "Assets/Art/ReReSprites/Actions/wave";
    private const string TopReReHairAdjustFolder = "Assets/Art/ReReSprites/Actions/hair_adjust";
    private const string TopReReSitSwingFolder = "Assets/Art/ReReSprites/Actions/sit_swing";
    private const string TopReReDozeFolder = "Assets/Art/ReReSprites/Actions/doze";
    private const string TopReReTalkFolder = "Assets/Art/ReReSprites/Actions/talk";
    private const string TopReReClickTalkFolder = "Assets/Art/ReReSprites/Actions/click_talk";
    private const string TopReReClickTalkWinkFolder = "Assets/Art/ReReSprites/Actions/click_talk_wink";
    private const string TopReReClickTalkProudFolder = "Assets/Art/ReReSprites/Actions/click_talk_proud";
    private const string TopReReClickTalkSecretFolder = "Assets/Art/ReReSprites/Actions/click_talk_secret";
    private const string DressTalkFolder = "Assets/Art/ReReSprites/DressTalk";
    private const string DressBodyMotionFolder = "Assets/Art/ReReSprites/DressBodyMotions";
    private const string DressChangeFolder = "Assets/Art/HeroineSprites/DressChange";
    private const string DressCurtainFolder = "Assets/Art/HeroineSprites/DressChangeCurtains";
    private const string ItemBagOpenSpritePath = "Assets/Art/UIConcepts/ItemBag/moshire_bag_open.png";
    private const string ItemBagClosedSpritePath = "Assets/Art/UIConcepts/ItemBag/moshire_bag_closed.png";
    private const string ItemBagStatesSheetPath = "Assets/Art/UIConcepts/ItemBag/moshire_bag_states_sheet.png";
    private const string ItemBagHookSpritePath = "Assets/Art/UIConcepts/ItemBag/moshire_bag_zipper_hook.png";
    private const string ItemBagReReZipFolder = "Assets/Art/UIConcepts/ItemBag/ZipReReAction/frames";
    private const string MapWideArtworkPath = "Assets/Art/UI/Map/menu_city_wide.png";
    private const string MapHoverAudioPath = "Assets/Audio/SFX/Title/title_cursor_move.mp3";

    private static readonly Color Cream = new Color(0.98f, 0.94f, 0.84f, 1f);
    private static readonly Color Ink = new Color(0.18f, 0.12f, 0.20f, 1f);
    private static readonly Color Mint = new Color(0.43f, 0.86f, 0.74f, 1f);
    private static readonly Color Coral = new Color(0.96f, 0.45f, 0.49f, 1f);
    private static readonly Color Lavender = new Color(0.67f, 0.58f, 0.91f, 1f);
    private static readonly Color Yellow = new Color(0.98f, 0.81f, 0.31f, 1f);
    private static readonly Color Cyan = new Color(0.45f, 0.78f, 0.92f, 1f);
    private static readonly Color Peach = new Color(0.98f, 0.68f, 0.52f, 1f);

    [MenuItem("Tools/MoshiReRe/Build MenuRoot V2 Preview")]
    public static void BuildPreview()
    {
        BuildPrefab();
        EnsureFolder("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
        instance.name = "MenuRootV2";
        var previewCamera = AddPreviewCamera();
        SetPreviewCanvasCamera(instance, previewCamera);
        ActivatePreviewPage(instance, "PageTop");
        SetPreviewVisibleOnAwake(instance, true);
        EditorSceneManager.SaveScene(scene, PreviewScenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MenuRootV2Builder] Built MenuRootV2 prefab and preview scene.");
    }

    [MenuItem("Tools/MoshiReRe/Build MenuRoot V2 Prefab")]
    public static void BuildPrefab()
    {
        EnsureFolder("Assets/NaninovelData/Resources/UI");

        var root = BuildMenuRoot();
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log("[MenuRootV2Builder] Built MenuRootV2 prefab.");
    }

    private static void ActivatePreviewPage(GameObject root, string pageName)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "SmartphoneLayer")
                child.gameObject.SetActive(pageName != "PageTop");
            else if (child.name.StartsWith("Page"))
                child.gameObject.SetActive(child.name == pageName);
        }
    }

    private static GameObject BuildMenuRoot()
    {
        var root = new GameObject("MenuRootV2", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(MenuRootV2UI), typeof(MenuRootV2InteractionController), typeof(MenuRootV2OrientationTransition));
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1920f, 1080f);
        rect.localScale = Vector3.one;

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 210;

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var dim = ImageRoot("SceneDim", rect, new Color(0.08f, 0.07f, 0.10f, 0f));
        Stretch(dim.rectTransform, Vector2.zero, Vector2.zero);
        dim.raycastTarget = false;

        var pageTop = BuildTopHudPage(rect, out var portraitPhoneFrame, out var topMascot, out var dressTileButton, out var statusTileButton,
            out var itemsTileButton, out var charactersTileButton, out var questTileButton, out var mapTileButton);
        BuildReReConversation(pageTop);

        var phone = ImageRoot("SmartphoneLayer", rect, new Color(0.13f, 0.08f, 0.17f, 0.96f));
        SetRect(phone.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1672f, 941f));
        PixelBorder(phone.rectTransform, "PhonePixelFrame", new Color(0.05f, 0.03f, 0.07f, 1f), 8f);
        var phoneBody = ImageRoot("SharedPhoneBody", phone.rectTransform, new Color(1f, 0.97f, 0.91f, 1f));
        phoneBody.raycastTarget = false;
        Stretch(phoneBody.rectTransform, new Vector2(48f, 48f), new Vector2(-48f, -48f));
        PixelBorder(phoneBody.rectTransform, "BodyPixelFrame", new Color(0.46f, 0.42f, 0.58f, 1f), 4f);

        var nav = BuildNavigation(phone.rectTransform, out var topButton, out var statusButton, out var itemsButton,
            out var charactersButton, out var questButton, out var mapButton, out var saveButton, out var settingsButton);

        var content = RectRoot("Content", phone.rectTransform);
        Stretch(content, new Vector2(273f, 54f), new Vector2(-273f, -135f));

        var pageStatus = BuildDressArtworkPage(rect, out var dressHomeButton, out var dressDressButton,
            out var dressStatusButton, out var dressItemsButton, out var dressMapButton);
        var pageItems = BuildItemsPage(rect, out var itemsHomeButton, out var itemsDressButton,
            out var itemsItemsButton, out var itemsCharactersButton, out var itemsQuestButton, out var itemsMapButton);
        var pageCharacters = BuildCharactersPage(rect, out var charactersHomeButton, out var charactersDressButton,
            out var charactersItemsButton, out var charactersCharactersButton, out var charactersQuestButton,
            out var charactersMapButton);
        var pageQuest = BuildQuestPage(content);
        var pageMap = BuildMapPage(content);
        var pageSave = BuildSavePage(content);
        var pageSettings = BuildSettingsPage(content);

        pageStatus.gameObject.SetActive(false);
        pageItems.gameObject.SetActive(false);
        pageCharacters.gameObject.SetActive(false);
        pageQuest.gameObject.SetActive(false);
        pageMap.gameObject.SetActive(false);
        pageSave.gameObject.SetActive(false);
        pageSettings.gameObject.SetActive(false);
        phone.gameObject.SetActive(false);

        ConfigureRoot(root.GetComponent<MenuRootV2UI>(), root.GetComponent<MenuRootV2OrientationTransition>(), phone.gameObject, pageTop.gameObject, pageStatus.gameObject, pageItems.gameObject,
            pageCharacters.gameObject, pageQuest.gameObject, pageMap.gameObject, pageSave.gameObject, pageSettings.gameObject, topButton, statusButton, itemsButton,
            charactersButton, questButton, mapButton, saveButton, settingsButton, topMascot, dressTileButton, statusTileButton, itemsTileButton,
            charactersTileButton, questTileButton, mapTileButton, dressHomeButton, dressDressButton, dressStatusButton,
            dressItemsButton, dressMapButton, charactersHomeButton, charactersDressButton, charactersItemsButton,
            charactersCharactersButton, charactersQuestButton, charactersMapButton, itemsHomeButton, itemsDressButton,
            itemsItemsButton, itemsCharactersButton, itemsQuestButton, itemsMapButton);
        ConfigureOrientation(root.GetComponent<MenuRootV2OrientationTransition>(), portraitPhoneFrame, phone.rectTransform,
            pageQuest.gameObject, pageMap.gameObject, pageSave.gameObject, pageSettings.gameObject);

        return root;
    }

    private static RectTransform BuildTopHudPage(RectTransform parent, out RectTransform portraitPhoneFrame, out MenuTopReReMascot topMascot,
        out Button dressTileButton, out Button statusTileButton, out Button itemsTileButton, out Button charactersTileButton,
        out Button questTileButton, out Button mapTileButton)
    {
        const float phoneWidth = 824f;
        const float phoneHeight = phoneWidth / (9f / 19.5f);
        var surface = new Color(0.94f, 0.97f, 1f, 0.97f);
        var surfaceBright = new Color(0.985f, 0.99f, 1f, 0.98f);
        var navy = new Color(0.055f, 0.085f, 0.16f, 1f);
        var accent = new Color(0.43f, 0.38f, 0.90f, 1f);
        var accentLight = new Color(0.70f, 0.70f, 1f, 0.82f);
        var cyanHighlight = new Color(0.56f, 0.88f, 0.96f, 0.90f);
        var coralAlert = new Color(0.96f, 0.34f, 0.40f, 1f);

        var page = RectRoot("PageTop", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        page.gameObject.AddComponent<CanvasGroup>();

        // Rim only: there is no body, screen, stage, or other central panel.
        // The tall shell continues below the 16:9 safe area instead of being shrunk to fit.
        portraitPhoneFrame = RectRoot("PortraitPhonePresentation", page);
        SetRect(portraitPhoneFrame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f),
            new Vector2(535f, 546f), new Vector2(phoneWidth, phoneHeight));
        // This is a clipped scene layer, not a phone body: the rim remains the only shell.
        // It gives the transparent HUD a temporary in-world stage while leaving no opaque panel
        // between the background and ReRe.
        var stage = RectRoot("TopTransparentStage", portraitPhoneFrame);
        SetRect(stage, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-42f, -218f), new Vector2(670f, 664f));
        stage.gameObject.AddComponent<RectMask2D>();

        var background = ImageRoot("TopPreviewBackground", stage, Color.white);
        background.sprite = LoadSprite(TopPreviewBackgroundPath);
        background.raycastTarget = false;
        SetRect(background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, stage.sizeDelta);
        var backgroundFitter = background.gameObject.AddComponent<AspectRatioFitter>();
        backgroundFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        backgroundFitter.aspectRatio = background.sprite && background.sprite.texture
            ? (float)background.sprite.texture.width / background.sprite.texture.height
            : 1f;

        // This nearly invisible target is intentionally below ReRe and every HUD action. It catches
        // only the open stage so the visible controls continue to receive their own clicks.
        var blankStageButton = ButtonRoot("TopStageBlankClick", stage, new Color(0.08f, 0.12f, 0.22f, 0.002f));
        Stretch(blankStageButton.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        blankStageButton.transition = Selectable.Transition.None;

        topMascot = BuildTopReReMascot(stage, new Vector2(-54f, 6f), new Vector2(430f, 650f),
            new Vector2(-170f, 414f), blankStageButton);

        BuildHudPhoneRim(portraitPhoneFrame, navy, accent, accentLight, cyanHighlight);

        var speaker = ImageRoot("PortraitPhoneTopSpeaker", portraitPhoneFrame, new Color(navy.r, navy.g, navy.b, 0.94f));
        speaker.raycastTarget = false;
        SetRect(speaker.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -47f), new Vector2(164f, 34f));
        PixelBorder(speaker.rectTransform, "SpeakerFrame", new Color(accent.r, accent.g, accent.b, 0.65f), 2f);

        var dayChip = HudInfoChip(portraitPhoneFrame, "HudDayChip", new Vector2(-287f, -132f), new Vector2(96f, 88f), surfaceBright, navy, accentLight);
        var debtChip = HudInfoChip(portraitPhoneFrame, "HudDebtChip", new Vector2(-115f, -132f), new Vector2(214f, 72f), surface, navy, accentLight);
        var moneyChip = HudInfoChip(portraitPhoneFrame, "HudMoneyChip", new Vector2(126f, -132f), new Vector2(230f, 72f), surface, navy, accentLight);
        var settingsButton = HudActionButton(portraitPhoneFrame, "HudSettingsButton", string.Empty, "settings",
            new Vector2(330f, -132f), new Vector2(76f, 76f), surfaceBright, navy, accent, cyanHighlight, false, true);

        var dayText = Text("DAY\n03", dayChip.rectTransform, 17f, FontStyles.Bold, TextAlignmentOptions.Center, navy,
            new Vector2(4f, 4f), new Vector2(-4f, -4f));
        dayText.name = "DayValue";
        var debtText = Text("7 DAYS", debtChip.rectTransform, 21f, FontStyles.Bold, TextAlignmentOptions.Center, navy,
            new Vector2(35f, 4f), new Vector2(-10f, -4f));
        debtText.name = "DebtDaysValue";
        var moneyText = Text("¥ 145,000", moneyChip.rectTransform, 19f, FontStyles.Bold, TextAlignmentOptions.Center, navy,
            new Vector2(16f, 4f), new Vector2(-8f, -4f));
        moneyText.name = "MoneyValue";

        var urgencyMark = ImageRoot("DebtUrgencyMark", debtChip.rectTransform, coralAlert);
        urgencyMark.raycastTarget = false;
        SetRect(urgencyMark.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(24f, 0f), new Vector2(14f, 14f));
        PixelBorder(urgencyMark.rectTransform, "UrgencyMarkFrame", navy, 1f);

        // Sparse right shortcuts: MAP then QUEST, both inside the 16:9 safe area.
        mapTileButton = HudActionButton(portraitPhoneFrame, "HudMapButton", "MAP", "map",
            new Vector2(280f, -432f), new Vector2(130f, 126f), surfaceBright, navy, accent, cyanHighlight, false, false);
        questTileButton = HudActionButton(portraitPhoneFrame, "HudQuestButton", "QUEST", "quest",
            new Vector2(280f, -626f), new Vector2(130f, 126f), surfaceBright, navy, accent, cyanHighlight, false, false);

        // The five controls float separately over the transparent center; HOME is the selected blue-violet action.
        dressTileButton = HudActionButton(portraitPhoneFrame, "HudDressButton", "DRESS", "dress",
            new Vector2(-266f, -978f), new Vector2(122f, 116f), surface, navy, accent, cyanHighlight, false, false);
        charactersTileButton = HudActionButton(portraitPhoneFrame, "HudCharactersButton", "CHAR", "char",
            new Vector2(-133f, -978f), new Vector2(122f, 116f), surface, navy, accent, cyanHighlight, false, false);
        var homeButton = HudActionButton(portraitPhoneFrame, "HudHomeButton", "HOME", "home",
            new Vector2(0f, -978f), new Vector2(136f, 128f), surfaceBright, navy, accent, cyanHighlight, true, false);
        itemsTileButton = HudActionButton(portraitPhoneFrame, "HudItemsButton", "ITEM", "item",
            new Vector2(137f, -978f), new Vector2(122f, 116f), surface, navy, accent, cyanHighlight, false, false);
        var saveButton = HudActionButton(portraitPhoneFrame, "HudSaveButton", "SAVE", "save",
            new Vector2(270f, -978f), new Vector2(122f, 116f), surface, navy, accent, cyanHighlight, false, false);
        statusTileButton = null;

        var dressBadge = AddDynamicNotificationBadge(dressTileButton, "DressNotificationBadge");
        var charactersBadge = AddDynamicNotificationBadge(charactersTileButton, "CharactersNotificationBadge");
        var homeBadge = AddDynamicNotificationBadge(homeButton, "HomeNotificationBadge");
        var itemsBadge = AddDynamicNotificationBadge(itemsTileButton, "ItemsNotificationBadge");
        var saveBadge = AddDynamicNotificationBadge(saveButton, "SaveNotificationBadge");
        var mapBadge = AddDynamicNotificationBadge(mapTileButton, "MapNotificationBadge");
        var questBadge = AddDynamicNotificationBadge(questTileButton, "QuestNotificationBadge", 1);
        var settingsBadge = AddDynamicNotificationBadge(settingsButton, "SettingsNotificationBadge");

        ConfigurePageNavigation(page.gameObject, homeButton, null, null, null, null, null, saveButton, settingsButton);
        ConfigureTopHudState(page.gameObject, dayText, debtText, moneyText, urgencyMark.gameObject,
            dressBadge, charactersBadge, homeBadge, itemsBadge, saveBadge, mapBadge, questBadge, settingsBadge);

        // ReRe is built above the scene background but below the rim and controls, preserving the
        // intended background -> character -> HUD draw order.
        return page;
    }

    private static void BuildReReConversation(RectTransform parent)
    {
        var root = RectRoot("ReReConversation", parent);
        SetRect(root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-48f, -724f), new Vector2(650f, 142f));

        var panel = ImageRoot("ConversationPanel", root, new Color(0.10f, 0.08f, 0.18f, 0.94f));
        Stretch(panel.rectTransform, Vector2.zero, Vector2.zero);
        PixelBorder(panel.rectTransform, "ConversationFrame", new Color(0.70f, 0.64f, 0.94f, 0.90f), 3f);

        var speech = ImageRoot("ReReSpeechBubble", root, new Color(1f, 0.96f, 0.88f, 1f));
        SetRect(speech.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -8f), new Vector2(-24f, 64f));
        speech.raycastTarget = false;
        var speechText = Text("ここにReReの返答が表示されるよ。", speech.rectTransform, 16f, FontStyles.Bold,
            TextAlignmentOptions.TopLeft, Ink, new Vector2(16f, 8f), new Vector2(-16f, -8f));
        speechText.name = "ResponseText";

        var inputGo = new GameObject("ReReInput", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
        inputGo.transform.SetParent(root, false);
        var inputRect = inputGo.GetComponent<RectTransform>();
        SetRect(inputRect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-41f, 10f), new Vector2(-106f, 44f));
        var inputImage = inputGo.GetComponent<Image>();
        inputImage.sprite = GetDefaultSprite();
        inputImage.color = new Color(0.97f, 0.94f, 1f, 1f);
        var inputText = Text("", inputRect, 17f, FontStyles.Normal, TextAlignmentOptions.Left, Ink,
            new Vector2(14f, 8f), new Vector2(-14f, -8f));
        inputText.name = "Text";
        inputText.raycastTarget = false;
        var placeholder = Text("ReReに相談… Enterで送信", inputRect, 17f, FontStyles.Normal,
            TextAlignmentOptions.Left, new Color(0.35f, 0.29f, 0.48f, 0.65f), new Vector2(14f, 8f), new Vector2(-14f, -8f));
        placeholder.name = "Placeholder";
        placeholder.raycastTarget = false;

        var inputField = inputGo.GetComponent<TMP_InputField>();
        inputField.textComponent = inputText;
        inputField.placeholder = placeholder;
        inputField.targetGraphic = inputImage;
        inputField.lineType = TMP_InputField.LineType.SingleLine;
        inputField.restoreOriginalTextOnEscape = false;

        var send = ButtonRoot("ReReSend", root, Mint);
        SetRect(send.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-10f, 10f), new Vector2(82f, 44f));
        PixelBorder(send.GetComponent<RectTransform>(), "SendFrame", Ink, 2f);
        Text("SEND", send.GetComponent<RectTransform>(), 14f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);

        var controller = root.gameObject.AddComponent<ReReConversationUI>();
        var so = new SerializedObject(controller);
        SetObject(so, "inputField", inputField);
        SetObject(so, "sendButton", send);
        SetObject(so, "speechBubbleRoot", speech.gameObject);
        SetObject(so, "speechText", speechText);
        SetObject(so, "speechCanvasGroup", speech.gameObject.AddComponent<CanvasGroup>());
        SetBool(so, "hideSpeechBubbleOnAwake", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);

        var contextProvider = root.gameObject.AddComponent<ReReConversationContextProvider>();
        var contextSo = new SerializedObject(contextProvider);
        SetObject(contextSo, "inventoryDatabase", FindFirstAssetOfType<InventoryDatabase>());
        contextSo.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(contextProvider);
    }

    private static void BuildHudPhoneRim(RectTransform parent, Color navy, Color accent, Color accentLight, Color cyanHighlight)
    {
        var outer = RectRoot("PortraitPhoneOuterRim", parent);
        Stretch(outer, Vector2.zero, Vector2.zero);
        PixelBorder(outer, "OuterNavyFrame", navy, 10f);

        var accentRim = RectRoot("PortraitPhoneAccentRim", parent);
        Stretch(accentRim, new Vector2(12f, 12f), new Vector2(-12f, -12f));
        PixelBorder(accentRim, "AccentFrame", accent, 3f);

        var innerRim = RectRoot("PortraitPhoneInnerRim", parent);
        Stretch(innerRim, new Vector2(23f, 23f), new Vector2(-23f, -23f));
        PixelBorder(innerRim, "InnerHighlightFrame", accentLight, 1.5f);

        var leftGlow = ImageRoot("LeftRimCyanHighlight", parent, new Color(cyanHighlight.r, cyanHighlight.g, cyanHighlight.b, 0.68f));
        leftGlow.raycastTarget = false;
        SetRect(leftGlow.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(30f, -194f), new Vector2(4f, 94f));
        var topGlow = ImageRoot("TopRimCyanHighlight", parent, new Color(cyanHighlight.r, cyanHighlight.g, cyanHighlight.b, 0.64f));
        topGlow.raycastTarget = false;
        SetRect(topGlow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-188f, -28f), new Vector2(78f, 3f));

        AddHudRimSpark(parent, "TopLeftSpark", new Vector2(38f, -52f), accentLight);
        AddHudRimSpark(parent, "TopRightSpark", new Vector2(-38f, -52f), accentLight, true);
        AddHudRimSpark(parent, "LowerLeftSpark", new Vector2(38f, -900f), cyanHighlight);
        AddHudRimSpark(parent, "LowerRightSpark", new Vector2(-38f, -900f), cyanHighlight, true);
    }

    private static void AddHudRimSpark(RectTransform parent, string name, Vector2 position, Color color, bool right = false)
    {
        var spark = ImageRoot(name, parent, color);
        spark.raycastTarget = false;
        var anchor = right ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
        var pivot = right ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f);
        SetRect(spark.rectTransform, anchor, anchor, pivot, position, new Vector2(14f, 4f));
        var upright = ImageRoot("Upright", spark.rectTransform, color);
        upright.raycastTarget = false;
        SetRect(upright.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4f, 14f));
    }

    private static Image HudInfoChip(RectTransform parent, string name, Vector2 position, Vector2 size, Color surface, Color navy, Color accentLight)
    {
        var chip = ImageRoot(name, parent, surface);
        chip.raycastTarget = false;
        SetRect(chip.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), position, size);
        PixelBorder(chip.rectTransform, "NavyFrame", navy, 3f);
        var edge = RectRoot("AccentEdge", chip.rectTransform);
        Stretch(edge, new Vector2(6f, 6f), new Vector2(-6f, -6f));
        PixelBorder(edge, "AccentFrame", accentLight, 1f);
        return chip;
    }

    private static Button HudActionButton(RectTransform parent, string name, string label, string iconId, Vector2 position, Vector2 size,
        Color surface, Color navy, Color accent, Color cyanHighlight, bool selected, bool iconOnly)
    {
        var button = ButtonRoot(name, parent, surface);
        var rect = button.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), position, size);
        PixelBorder(rect, "NavyFrame", navy, 3f);
        var inset = RectRoot("InsetFrame", rect);
        Stretch(inset, new Vector2(6f, 6f), new Vector2(-6f, -6f));
        PixelBorder(inset, "SoftFrame", new Color(0.73f, 0.78f, 0.91f, 0.95f), 1f);

        if (selected)
        {
            var selectedFrame = RectRoot("SelectedAccentFrame", rect);
            Stretch(selectedFrame, new Vector2(-3f, -3f), new Vector2(3f, 3f));
            PixelBorder(selectedFrame, "AccentFrame", accent, 3f);
            var underline = ImageRoot("SelectedUnderline", rect, accent);
            underline.raycastTarget = false;
            SetRect(underline.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 9f), new Vector2(size.x - 24f, 7f));
        }
        else
        {
            var highlight = ImageRoot("CyanCornerHighlight", rect, new Color(cyanHighlight.r, cyanHighlight.g, cyanHighlight.b, 0.70f));
            highlight.raycastTarget = false;
            SetRect(highlight.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(9f, -9f), new Vector2(12f, 3f));
        }

        var icon = ImageRoot("HudIcon", rect, selected ? accent : navy);
        icon.sprite = LoadNavigationSprite(iconId);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var iconSize = iconOnly ? new Vector2(44f, 44f) : selected ? new Vector2(58f, 58f) : new Vector2(52f, 52f);
        SetRect(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            iconOnly ? Vector2.zero : new Vector2(0f, 15f), iconSize);
        if (!iconOnly)
            Text(label, rect, 16f, FontStyles.Bold, TextAlignmentOptions.Bottom, navy,
                new Vector2(6f, 8f), new Vector2(-6f, -52f));
        return button;
    }

    private static RectTransform BuildTopArtworkPage(RectTransform parent, out RectTransform portraitPhoneFrame, out MenuTopReReMascot topMascot,
        out Button dressTileButton, out Button statusTileButton, out Button itemsTileButton, out Button charactersTileButton,
        out Button questTileButton, out Button mapTileButton)
    {
        var page = RectRoot("PageTop", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        page.gameObject.AddComponent<CanvasGroup>();

        portraitPhoneFrame = RectRoot("PortraitPhonePresentation", page);
        SetRect(portraitPhoneFrame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660f, 972f));

        var phoneShell = ImageRoot("PortraitPhoneShell", portraitPhoneFrame, new Color(0.10f, 0.06f, 0.16f, 0.18f));
        Stretch(phoneShell.rectTransform, Vector2.zero, Vector2.zero);
        phoneShell.raycastTarget = false;
        PixelBorder(phoneShell.rectTransform, "PortraitPhoneOuterFrame", new Color(0.02f, 0.02f, 0.04f, 1f), 10f);

        var phoneRim = ImageRoot("PortraitPhoneRim", portraitPhoneFrame, new Color(0.98f, 0.95f, 0.89f, 0.10f));
        Stretch(phoneRim.rectTransform, new Vector2(18f, 18f), new Vector2(-18f, -18f));
        phoneRim.raycastTarget = false;
        PixelBorder(phoneRim.rectTransform, "PortraitPhoneRimFrame", new Color(0.48f, 0.41f, 0.60f, 1f), 4f);

        var screen = ImageRoot("TransparentReReStage", portraitPhoneFrame, new Color(0.30f, 0.18f, 0.42f, 0.055f));
        Stretch(screen.rectTransform, new Vector2(34f, 34f), new Vector2(-34f, -34f));
        screen.raycastTarget = false;
        PixelBorder(screen.rectTransform, "TransparentStageFrame", new Color(0.54f, 0.43f, 0.76f, 0.72f), 3f);

        var speaker = ImageRoot("PortraitPhoneSpeaker", portraitPhoneFrame, new Color(0.10f, 0.06f, 0.16f, 0.92f));
        speaker.raycastTarget = false;
        SetRect(speaker.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(118f, 10f));

        var statusBar = ImageRoot("PortraitStatusBar", portraitPhoneFrame, new Color(0.18f, 0.11f, 0.28f, 0.94f));
        statusBar.raycastTarget = false;
        SetRect(statusBar.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(562f, 66f));
        PixelBorder(statusBar.rectTransform, "PortraitStatusFrame", Ink, 3f);
        Text("DAY 03   •   22:14", statusBar.rectTransform, 17f, FontStyles.Bold, TextAlignmentOptions.Left, Cream,
            new Vector2(18f, 8f), new Vector2(-230f, -8f));
        Text("MoshiReRe  /  ReRe ONLINE", statusBar.rectTransform, 15f, FontStyles.Bold, TextAlignmentOptions.Right, Mint,
            new Vector2(190f, 8f), new Vector2(-18f, -8f));

        var debtChip = PortraitStatusChip(portraitPhoneFrame, "DebtDeadlineChip", "返済まで 7 DAYS", new Vector2(-150f, 330f), Coral);
        var moneyChip = PortraitStatusChip(portraitPhoneFrame, "MoneyStatusChip", "¥ 145,000", new Vector2(150f, 330f), Mint);
        debtChip.raycastTarget = false;
        moneyChip.raycastTarget = false;

        var liveStage = ImageRoot("ReReLiveWindow", portraitPhoneFrame, new Color(0.48f, 0.76f, 0.91f, 0.035f));
        liveStage.raycastTarget = false;
        SetRect(liveStage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(314f, 480f));
        PixelBorder(liveStage.rectTransform, "ReReLiveWindowFrame", new Color(0.54f, 0.80f, 0.94f, 0.66f), 2f);
        Text("ReRe  LIVE LINK", liveStage.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.Top, new Color(0.76f, 0.95f, 0.96f, 0.90f),
            new Vector2(8f, 8f), new Vector2(-8f, -8f));
        Text("tap ReRe to ask for a hint", liveStage.rectTransform, 11f, FontStyles.Bold, TextAlignmentOptions.Bottom, new Color(0.83f, 0.90f, 0.98f, 0.82f),
            new Vector2(10f, 10f), new Vector2(-10f, -8f));

        dressTileButton = PortraitShortcutButton(portraitPhoneFrame, "DressTileHitbox", "DRESS", "dress", Lavender, new Vector2(-226f, 205f), new Vector2(116f, 104f));
        charactersTileButton = PortraitShortcutButton(portraitPhoneFrame, "CharactersTileHitbox", "CHAR", "char", Coral, new Vector2(-226f, 75f), new Vector2(116f, 104f));
        itemsTileButton = PortraitShortcutButton(portraitPhoneFrame, "ItemsTileHitbox", "ITEM", "item", Yellow, new Vector2(226f, 205f), new Vector2(116f, 104f));
        questTileButton = PortraitShortcutButton(portraitPhoneFrame, "QuestTileHitbox", "QUEST", "quest", Peach, new Vector2(226f, 75f), new Vector2(116f, 104f));
        mapTileButton = PortraitShortcutButton(portraitPhoneFrame, "MapTileHitbox", "MAP", "map", Cyan, new Vector2(-166f, -330f), new Vector2(150f, 96f));
        var saveButton = PortraitShortcutButton(portraitPhoneFrame, "SaveTileHitbox", "SAVE", "save", new Color(0.66f, 0.72f, 0.94f, 1f), Vector2.zero + new Vector2(0f, -330f), new Vector2(144f, 96f));
        var settingsButton = PortraitShortcutButton(portraitPhoneFrame, "SettingsTileHitbox", "SET", "settings", new Color(0.78f, 0.78f, 0.82f, 1f), new Vector2(166f, -330f), new Vector2(150f, 96f));
        statusTileButton = null;

        AddDynamicNotificationBadge(itemsTileButton, "TopItemNotificationBadge");
        AddDynamicNotificationBadge(questTileButton, "TopQuestNotificationBadge");
        ConfigurePageNavigation(page.gameObject, null, null, null, null, null, null, saveButton, settingsButton);
        topMascot = BuildTopReReMascot(portraitPhoneFrame, new Vector2(0f, -36f), new Vector2(276f, 402f), new Vector2(-160f, 232f));

        return page;
    }

    private static Image PortraitStatusChip(RectTransform parent, string name, string value, Vector2 position, Color color)
    {
        var chip = ImageRoot(name, parent, color);
        SetRect(chip.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(254f, 46f));
        PixelBorder(chip.rectTransform, name + "Frame", Ink, 2f);
        Text(value, chip.rectTransform, 14f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        return chip;
    }

    private static Button PortraitShortcutButton(RectTransform parent, string name, string label, string iconId, Color color, Vector2 position, Vector2 size)
    {
        var button = ButtonRoot(name, parent, color);
        var rect = button.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
        PixelBorder(rect, "Frame", Ink, 3f);

        var icon = ImageRoot("ShortcutIcon", rect, Color.white);
        icon.sprite = LoadNavigationSprite(iconId);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        SetRect(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(48f, 48f));
        Text(label, rect, 14f, FontStyles.Bold, TextAlignmentOptions.Bottom, Ink, new Vector2(5f, 6f), new Vector2(-5f, -4f));
        return button;
    }

    private static MenuTopReReMascot BuildTopReReMascot(RectTransform parent, Vector2 position, Vector2 mascotSize, Vector2 bubbleOffset,
        Button blankStageButton = null)
    {
        var container = RectRoot("TopReReMascot", parent);
        Stretch(container, Vector2.zero, Vector2.zero);
        var mascot = ImageRoot("Mascot", container, Color.white);
        mascot.sprite = LoadSprite(TopReReSpritePath);
        mascot.preserveAspect = true;
        mascot.raycastTarget = true;
        SetRect(mascot.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), position, mascotSize);
        var mascotButton = EnsureButton(mascot);

        var bubble = ImageRoot("IdleBubble", container, Color.white);
        bubble.sprite = LoadSprite(ReReSpeechBubblePath);
        bubble.preserveAspect = true;
        SetRect(bubble.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), bubbleOffset, new Vector2(324f, 140f));
        bubble.raycastTarget = false;
        var bubbleText = Text("今夜の準備をしよう。迷ったら、期限と情報ノードを確認してね。", bubble.rectTransform, 16f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Vector2(32f, 22f), new Vector2(-48f, -32f));
        bubbleText.font = FindPixelFontAsset();

        var mascotController = container.gameObject.AddComponent<MenuTopReReMascot>();
        var so = new SerializedObject(mascotController);
        SetObject(so, "mascotImage", mascot);
        SetObject(so, "mascotButton", mascotButton);
        SetObject(so, "blankStageButton", blankStageButton);
        SetObject(so, "mascot", mascot.rectTransform);
        SetObject(so, "bubble", bubble.rectTransform);
        SetObject(so, "bubbleText", bubbleText);
        SetStringArray(so, "clickMotionIds", System.Array.Empty<string>());
        SetVector2(so, "fixedBottomRightPosition", position);
        SetVector2(so, "bubbleOffset", bubbleOffset);
        SetFloat(so, "walkDistance", 36f);
        SetFloat(so, "walkSpeed", 22f);
        SetMotionSets(so);
        so.ApplyModifiedPropertiesWithoutUndo();
        mascotController.PlaceForMenuOpen();
        return mascotController;
    }

    private static Button TopHitbox(RectTransform parent, string name, Rect sourceRect)
    {
        const float artworkWidth = 1675f;
        const float artworkHeight = 943f;
        var button = ButtonRoot(name, parent, new Color(1f, 1f, 1f, 0.003f));
        var rect = button.GetComponent<RectTransform>();
        var centerX = sourceRect.x + sourceRect.width * 0.5f - artworkWidth * 0.5f;
        var centerY = artworkHeight * 0.5f - sourceRect.y - sourceRect.height * 0.5f;
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(centerX, centerY), sourceRect.size);
        return button;
    }

    private static RectTransform BuildDressArtworkPage(RectTransform parent, out Button homeButton, out Button dressButton,
        out Button statusButton, out Button itemsButton, out Button mapButton)
    {
        const float artworkWidth = 1672f;
        const float artworkHeight = 941f;
        var page = RectRoot("PageDressStatus", parent);
        Stretch(page, Vector2.zero, Vector2.zero);

        var artwork = ImageRoot("DressPhoneArtwork", page, Color.white);
        artwork.sprite = LoadSprite(CreateNeutralDressArtwork());
        artwork.preserveAspect = true;
        artwork.raycastTarget = false;
        SetRect(artwork.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1672f, 941f));

        homeButton = DressHitbox(page, "DressHomeHitbox", new Rect(184f, 81f, 152f, 54f), artworkWidth, artworkHeight);
        dressButton = DressHitbox(page, "DressDressHitbox", new Rect(369f, 82f, 58f, 54f), artworkWidth, artworkHeight);
        statusButton = DressHitbox(page, "DressStatusHitbox", new Rect(430f, 82f, 54f, 54f), artworkWidth, artworkHeight);
        itemsButton = DressHitbox(page, "DressItemsHitbox", new Rect(488f, 82f, 56f, 54f), artworkWidth, artworkHeight);
        mapButton = DressHitbox(page, "DressMapHitbox", new Rect(548f, 82f, 58f, 54f), artworkWidth, artworkHeight);

        var outfitRoot = RectRoot("OutfitCards", page);
        Stretch(outfitRoot, Vector2.zero, Vector2.zero);
        var cardRects = new[]
        {
            new Rect(244f, 661f, 186f, 169f),
            new Rect(445f, 661f, 186f, 169f),
            new Rect(648f, 661f, 186f, 169f),
            new Rect(850f, 661f, 186f, 169f),
            new Rect(1048f, 661f, 186f, 169f),
            new Rect(1250f, 661f, 186f, 169f)
        };

        for (int i = 0; i < cardRects.Length; i++)
        {
            var card = DressHitbox(outfitRoot, "OutfitCard" + i, cardRects[i], artworkWidth, artworkHeight);
            var selectedFrame = RectRoot("SelectedFrame", card.GetComponent<RectTransform>());
            Stretch(selectedFrame, Vector2.zero, Vector2.zero);
            var selectedTint = selectedFrame.gameObject.AddComponent<Image>();
            selectedTint.color = new Color(0.50f, 0.34f, 0.86f, 0.20f);
            selectedTint.raycastTarget = false;
            selectedFrame.gameObject.SetActive(i == 0);
            BorderPart(selectedFrame, "SelectedTop", Yellow, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 7f));
            BorderPart(selectedFrame, "SelectedBottom", Yellow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 7f));
            BorderPart(selectedFrame, "SelectedLeft", Yellow, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(7f, 0f));
            BorderPart(selectedFrame, "SelectedRight", Yellow, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(7f, 0f));

            var dynamicIcon = ImageRoot("OutfitIconDynamic" + i, card.GetComponent<RectTransform>(), Color.white);
            dynamicIcon.preserveAspect = true;
            dynamicIcon.raycastTarget = false;
            SetRect(dynamicIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(122f, 96f));
            dynamicIcon.gameObject.SetActive(false);
        }

        var rightPanel = ImageRoot("DressDynamicRightPanel", page, new Color(1f, 0.88f, 0.86f, 1f));
        rightPanel.raycastTarget = false;
        SetRect(rightPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            DressCenter(new Rect(1076f, 148f, 344f, 454f), artworkWidth, artworkHeight), DressSize(new Rect(1076f, 148f, 344f, 454f), artworkWidth, artworkHeight));
        PixelBorder(rightPanel.rectTransform, "RightPanelFrame", Ink, 4f);
        TextBoxCentered("ReRe's COMMENT", page, 24f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Rect(1084f, 154f, 324f, 42f), artworkWidth, artworkHeight);

        var commentRoot = RectRoot("ReReCommentPanel", page);
        Stretch(commentRoot, Vector2.zero, Vector2.zero);
        var bubble = ImageRoot("DynamicCommentBubble", commentRoot, new Color(1f, 0.98f, 0.87f, 1f));
        bubble.sprite = LoadSprite(DressCommentBubblePath);
        bubble.preserveAspect = true;
        bubble.raycastTarget = false;
        SetRect(bubble.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            DressCenter(new Rect(1096f, 218f, 244f, 104f), artworkWidth, artworkHeight), DressSize(new Rect(1096f, 218f, 244f, 104f), artworkWidth, artworkHeight));

        var commentText = TextBoxCentered("select outfit", commentRoot, 21f, FontStyles.Bold, TextAlignmentOptions.TopLeft, Ink,
            new Rect(1114f, 236f, 174f, 70f), artworkWidth, artworkHeight);
        commentText.name = "CommentText";

        var rereFace = ImageRoot("ReReFaceImage", commentRoot, Color.white);
        rereFace.sprite = LoadSprite(DressBodyMotionFolder + "/talk_idle/talk_idle_01.png");
        rereFace.preserveAspect = true;
        rereFace.raycastTarget = false;
        SetRect(rereFace.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            DressCenter(new Rect(1316f, 212f, 92f, 118f), artworkWidth, artworkHeight), DressSize(new Rect(1316f, 212f, 92f, 118f), artworkWidth, artworkHeight));

        var yesButton = ButtonRoot("YesButton", commentRoot, Mint);
        SetRect(yesButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            DressCenter(new Rect(1190f, 338f, 98f, 38f), artworkWidth, artworkHeight), DressSize(new Rect(1190f, 338f, 98f, 38f), artworkWidth, artworkHeight));
        PixelBorder(yesButton.GetComponent<RectTransform>(), "YesFrame", Ink, 3f);
        Text("YES", yesButton.GetComponent<RectTransform>(), 23f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);

        var bonusRoot = RectRoot("OutfitBonusPanel", page);
        Stretch(bonusRoot, Vector2.zero, Vector2.zero);
        var bonusPanel = ImageRoot("BonusPanelDynamicBg", bonusRoot, new Color(1f, 0.90f, 0.68f, 1f));
        bonusPanel.raycastTarget = false;
        SetRect(bonusPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            DressCenter(new Rect(1102f, 400f, 292f, 168f), artworkWidth, artworkHeight), DressSize(new Rect(1102f, 400f, 292f, 168f), artworkWidth, artworkHeight));
        PixelBorder(bonusPanel.rectTransform, "BonusPanelFrame", Ink, 3f);
        TextBoxCentered("BONUS", bonusRoot, 22f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Rect(1110f, 406f, 276f, 28f), artworkWidth, artworkHeight);
        var colors = new[] { Coral, Cyan, Lavender, Yellow, Mint };
        for (int i = 0; i < 5; i++)
            DressBonusOverlay(bonusRoot, i, colors[i], artworkWidth, artworkHeight, 442f + i * 24f);

        var radarRoot = RectRoot("RadarPanel", page);
        Stretch(radarRoot, Vector2.zero, Vector2.zero);
        var outfitRadar = RectRoot("OutfitAdjustedRadar", radarRoot);
        SetRect(outfitRadar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            DressCenter(new Rect(302f, 336f, 220f, 188f), artworkWidth, artworkHeight), DressSize(new Rect(302f, 336f, 220f, 188f), artworkWidth, artworkHeight));
        var outfitChart = outfitRadar.gameObject.AddComponent<RadarChart>();
        outfitChart.color = new Color(1f, 0.86f, 0.24f, 0.48f);
        outfitChart.SetRadius(88f);
        outfitChart.SetMaxValue(5f);
        outfitChart.SetBackgroundColor(Color.clear);
        outfitChart.SetValues(1, 1, 1, 1, 1);
        var baseRadar = RectRoot("BaseStatusRadar", radarRoot);
        SetRect(baseRadar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            DressCenter(new Rect(302f, 336f, 220f, 188f), artworkWidth, artworkHeight), DressSize(new Rect(302f, 336f, 220f, 188f), artworkWidth, artworkHeight));
        var baseChart = baseRadar.gameObject.AddComponent<RadarChart>();
        baseChart.color = new Color(1f, 0.42f, 0.54f, 0.46f);
        baseChart.SetRadius(88f);
        baseChart.SetMaxValue(5f);
        baseChart.SetGridVisible(true);
        baseChart.SetBackgroundColor(Color.clear);
        baseChart.SetValues(1, 1, 1, 1, 1);

        var booth = RectRoot("CharacterPreviewBooth", page);
        Stretch(booth, Vector2.zero, Vector2.zero);
        var bodyTint = ImageRoot("StandingSpritePlaceholder", booth, Color.white);
        bodyTint.sprite = LoadSprite(DressChangeFolder + "/room_blouse_black_skirt/room_blouse_black_skirt_01.png");
        bodyTint.preserveAspect = true;
        bodyTint.raycastTarget = false;
        SetRect(bodyTint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-3f, 56f), new Vector2(220f, 330f));
        var curtainLeft = ImageRoot("ChangingCurtainLeft", booth, Color.white);
        curtainLeft.sprite = LoadSprite(DressCurtainFolder + "/left_curtain/left_curtain_01.png");
        curtainLeft.preserveAspect = true;
        curtainLeft.raycastTarget = false;
        SetRect(curtainLeft.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-116f, 58f), new Vector2(210f, 370f));
        curtainLeft.gameObject.SetActive(false);
        var curtainRight = ImageRoot("ChangingCurtainRight", booth, Color.white);
        curtainRight.sprite = LoadSprite(DressCurtainFolder + "/right_curtain/right_curtain_01.png");
        curtainRight.preserveAspect = true;
        curtainRight.raycastTarget = false;
        SetRect(curtainRight.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(116f, 58f), new Vector2(210f, 370f));
        curtainRight.gameObject.SetActive(false);

        var dressController = page.gameObject.AddComponent<DressMenuController>();
        var dressControllerSo = new SerializedObject(dressController);
        SetObject(dressControllerSo, "rereFaceImage", rereFace);
        SetSpriteArray(dressControllerSo, "rereTalkFrames", LoadSpritesFromFolder(DressBodyMotionFolder + "/talk_idle"));
        SetObject(dressControllerSo, "curtainLeftImage", curtainLeft);
        SetObject(dressControllerSo, "curtainRightImage", curtainRight);
        SetSpriteArray(dressControllerSo, "curtainLeftFrames", LoadSpritesFromFolder(DressCurtainFolder + "/left_curtain"));
        SetSpriteArray(dressControllerSo, "curtainRightFrames", LoadSpritesFromFolder(DressCurtainFolder + "/right_curtain"));
        SetDressTalkSets(dressControllerSo);
        SetOutfitChangeSets(dressControllerSo);
        dressControllerSo.ApplyModifiedPropertiesWithoutUndo();
        BuildUnifiedPageNavigation(page, artworkWidth, artworkHeight);
        return page;
    }

    private static Button DressHitbox(RectTransform parent, string name, Rect sourceRect, float artworkWidth, float artworkHeight)
    {
        var button = ButtonRoot(name, parent, new Color(1f, 1f, 1f, 0.003f));
        var rect = button.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), DressCenter(sourceRect, artworkWidth, artworkHeight), DressSize(sourceRect, artworkWidth, artworkHeight));
        return button;
    }

    private static string CreateNeutralDressArtwork()
    {
        var sourceImporter = AssetImporter.GetAtPath(DressArtworkPath) as TextureImporter;
        if (sourceImporter == null)
            return DressArtworkPath;

        var wasReadable = sourceImporter.isReadable;
        if (!wasReadable)
        {
            sourceImporter.isReadable = true;
            sourceImporter.SaveAndReimport();
        }

        var source = AssetDatabase.LoadAssetAtPath<Texture2D>(DressArtworkPath);
        if (!source)
            return DressArtworkPath;

        var pixels = source.GetPixels32();
        var width = source.width;
        var height = source.height;
        var neutral = SampleTop(pixels, width, height, 450, 680);
        var frame = SampleTop(pixels, width, height, 445, 662);

        // The selected card's purple is a connected pixel-art background. Flooding it
        // from its empty corners keeps the dress sprite and the status icons intact.
        FloodFillTop(pixels, width, height, 264, 678, neutral);
        FloodFillTop(pixels, width, height, 410, 678, neutral);
        FloodFillTop(pixels, width, height, 264, 810, neutral);
        FloodFillTop(pixels, width, height, 410, 810, neutral);
        FloodFillTop(pixels, width, height, 254, 744, neutral);
        FloodFillTop(pixels, width, height, 269, 688, neutral);
        FloodFillTop(pixels, width, height, 398, 688, neutral);
        FloodFillTop(pixels, width, height, 397, 805, neutral);
        NeutralizeSelectionDecorations(pixels, width, height, neutral);
        ClearDressCommentEllipsis(pixels, width, height);
        ClearDressRadarFill(pixels, width, height);
        ClearDressBakedCharacter(pixels, width, height);

        PaintTopRect(pixels, width, height, new RectInt(244, 661, 186, 6), frame);
        PaintTopRect(pixels, width, height, new RectInt(244, 824, 186, 6), frame);
        PaintTopRect(pixels, width, height, new RectInt(244, 661, 6, 169), frame);
        PaintTopRect(pixels, width, height, new RectInt(424, 661, 6, 169), frame);
        PaintTopRect(pixels, width, height, new RectInt(239, 797, 10, 26), frame);
        PaintTopRect(pixels, width, height, new RectInt(424, 797, 10, 26), frame);

        var output = new Texture2D(width, height, TextureFormat.RGBA32, false);
        output.SetPixels32(pixels);
        output.Apply(false, false);
        File.WriteAllBytes(NeutralDressArtworkPath, output.EncodeToPNG());
        Object.DestroyImmediate(output);
        AssetDatabase.ImportAsset(NeutralDressArtworkPath, ImportAssetOptions.ForceUpdate);

        if (!wasReadable)
        {
            sourceImporter.isReadable = false;
            sourceImporter.SaveAndReimport();
        }

        return NeutralDressArtworkPath;
    }

    private static void FloodFillTop(Color32[] pixels, int width, int height, int startX, int startY, Color32 replacement)
    {
        var source = SampleTop(pixels, width, height, startX, startY);
        if (source.Equals(replacement))
            return;

        const int minX = 250;
        const int maxX = 423;
        const int minY = 667;
        const int maxY = 823;
        var pending = new Queue<Vector2Int>();
        pending.Enqueue(new Vector2Int(startX, startY));

        while (pending.Count > 0)
        {
            var point = pending.Dequeue();
            if (point.x < minX || point.x > maxX || point.y < minY || point.y > maxY)
                continue;

            var current = SampleTop(pixels, width, height, point.x, point.y);
            if (!SameColor(current, source))
                continue;

            SetTop(pixels, width, height, point.x, point.y, replacement);
            pending.Enqueue(new Vector2Int(point.x + 1, point.y));
            pending.Enqueue(new Vector2Int(point.x - 1, point.y));
            pending.Enqueue(new Vector2Int(point.x, point.y + 1));
            pending.Enqueue(new Vector2Int(point.x, point.y - 1));
        }
    }

    private static bool SameColor(Color32 a, Color32 b)
    {
        return Mathf.Abs(a.r - b.r) <= 8 && Mathf.Abs(a.g - b.g) <= 8 && Mathf.Abs(a.b - b.b) <= 8 && Mathf.Abs(a.a - b.a) <= 8;
    }

    private static void NeutralizeSelectionDecorations(Color32[] pixels, int width, int height, Color32 neutral)
    {
        for (var y = 667; y < 795; y++)
        for (var x = 250; x < 424; x++)
        {
            var pixel = SampleTop(pixels, width, height, x, y);
            var isLavender = pixel.r > 90 && pixel.b > 130 && pixel.b > pixel.r + 10 && pixel.b > pixel.g + 5;
            var isGold = pixel.r > 200 && pixel.g > 150 && pixel.b < 130;
            if (isLavender || isGold)
                SetTop(pixels, width, height, x, y, neutral);
        }
    }

    private static void ClearDressCommentEllipsis(Color32[] pixels, int width, int height)
    {
        var bubbleFill = SampleTop(pixels, width, height, 1180, 245);
        PaintTopRect(pixels, width, height, new RectInt(1108, 231, 58, 28), bubbleFill);
    }

    private static void ClearDressRadarFill(Color32[] pixels, int width, int height)
    {
        var background = SampleTop(pixels, width, height, 300, 360);
        for (var y = 306; y < 546; y++)
        for (var x = 250; x < 545; x++)
        {
            var pixel = SampleTop(pixels, width, height, x, y);
            var isRadarLine = pixel.g > 115 && pixel.b > 110 && pixel.r < 170;
            var isPink = pixel.r > pixel.g + 18 && pixel.r > pixel.b + 10;
            var isYellow = pixel.r > 190 && pixel.g > 125 && pixel.b < 170;
            var isGrayShadow = pixel.r > 70 && pixel.r < 170 && pixel.g > 80 && pixel.g < 180 && pixel.b > 80 && pixel.b < 190;
            if (isRadarLine || isPink || isYellow || isGrayShadow)
                SetTop(pixels, width, height, x, y, background);
        }
    }

    private static void ClearDressBakedCharacter(Color32[] pixels, int width, int height)
    {
        var background = SampleTop(pixels, width, height, 783, 322);
        var shadow = SampleTop(pixels, width, height, 774, 610);
        PaintTopRect(pixels, width, height, new RectInt(737, 258, 113, 330), background);
        PaintTopRect(pixels, width, height, new RectInt(722, 586, 144, 50), shadow);
    }

    private static void PaintTopRect(Color32[] pixels, int width, int height, RectInt rect, Color32 color)
    {
        for (var y = rect.yMin; y < rect.yMax; y++)
        for (var x = rect.xMin; x < rect.xMax; x++)
            SetTop(pixels, width, height, x, y, color);
    }

    private static Color32 SampleTop(Color32[] pixels, int width, int height, int x, int y)
    {
        return pixels[(height - 1 - y) * width + x];
    }

    private static void SetTop(Color32[] pixels, int width, int height, int x, int y, Color32 color)
    {
        pixels[(height - 1 - y) * width + x] = color;
    }

    private static void DressBonusOverlay(RectTransform parent, int index, Color color, float artworkWidth, float artworkHeight, float sourceY)
    {
        var source = new Rect(1130f, sourceY, 200f, 16f);
        var bg = ImageRoot("BonusMeterBg" + index, parent, new Color(0.98f, 0.95f, 0.78f, 1f));
        bg.raycastTarget = false;
        SetRect(bg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), DressCenter(source, artworkWidth, artworkHeight), DressSize(source, artworkWidth, artworkHeight));
        PixelBorder(bg.rectTransform, "MeterFrame", new Color(0.20f, 0.13f, 0.28f, 0.95f), 1f);
        var fill = ImageRoot("Fill", bg.rectTransform, color);
        fill.raycastTarget = false;
        SetRect(fill.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(2f, 0f), new Vector2(30f, 10f));
        var value = TextBoxCentered("+0", parent, 18f, FontStyles.Bold, TextAlignmentOptions.Right, Ink,
            new Rect(1342f, sourceY - 7f, 42f, 30f), artworkWidth, artworkHeight);
        value.name = "BonusValueText" + index;
    }

    private static Vector2 DressCenter(Rect sourceRect, float artworkWidth, float artworkHeight)
    {
        const float displayWidth = 1672f;
        const float displayHeight = 941f;
        var x = (sourceRect.x + sourceRect.width * 0.5f - artworkWidth * 0.5f) * displayWidth / artworkWidth;
        var y = (artworkHeight * 0.5f - sourceRect.y - sourceRect.height * 0.5f) * displayHeight / artworkHeight;
        return new Vector2(x, y);
    }

    private static Vector2 DressSize(Rect sourceRect, float artworkWidth, float artworkHeight)
    {
        const float displayWidth = 1672f;
        const float displayHeight = 941f;
        return new Vector2(sourceRect.width * displayWidth / artworkWidth, sourceRect.height * displayHeight / artworkHeight);
    }

    private static RectTransform BuildTopPage(RectTransform parent, out Button statusTileButton, out Button itemsTileButton,
        out Button charactersTileButton, out Button questTileButton, out Button mapTileButton)
    {
        var page = RectRoot("PageTop", parent);
        Stretch(page, Vector2.zero, Vector2.zero);

        var wallpaper = ImageRoot("PixelWallpaper", page, new Color(0.98f, 0.94f, 0.84f, 0.96f));
        Stretch(wallpaper.rectTransform, new Vector2(18f, 18f), new Vector2(-18f, -18f));
        PixelBorder(wallpaper.rectTransform, "WallpaperFrame", new Color(0.22f, 0.13f, 0.25f, 1f), 5f);
        AddPixelPattern(wallpaper.rectTransform);
        AddStatusBar(page);

        Text("ReRe OS", page, 22f, FontStyles.Bold, TextAlignmentOptions.TopLeft, new Color(0.44f, 0.24f, 0.52f, 1f),
            new Vector2(52f, 82f), new Vector2(-34f, -96f));
        Text("HOME / assistant tiles", page, 15f, FontStyles.Bold, TextAlignmentOptions.TopRight, new Color(0.42f, 0.34f, 0.46f, 1f),
            new Vector2(52f, 88f), new Vector2(-44f, -102f));

        var dress = Tile(page, "DressStatusTile", "DRESS / STATUS", Coral, new Vector2(42f, -148f), new Vector2(424f, 260f));
        var map = Tile(page, "MapTile", "MAP", Mint, new Vector2(494f, -148f), new Vector2(318f, 260f));
        var quest = Tile(page, "QuestTile", "QUEST", Yellow, new Vector2(840f, -148f), new Vector2(236f, 122f));
        var chars = Tile(page, "CharactersTile", "CHARACTERS", Lavender, new Vector2(840f, -286f), new Vector2(236f, 122f));
        var items = Tile(page, "ItemsTile", "ITEMS", Cyan, new Vector2(42f, -438f), new Vector2(282f, 158f));
        var save = Tile(page, "SaveTile", "SAVE", Peach, new Vector2(352f, -438f), new Vector2(146f, 158f));
        var settings = Tile(page, "SettingsTile", "SETTINGS", new Color(0.82f, 0.76f, 0.91f, 1f), new Vector2(526f, -438f), new Vector2(184f, 158f));
        var advice = Tile(page, "AdviceTile", "ReRe Hint", new Color(0.92f, 0.88f, 0.98f, 1f), new Vector2(738f, -438f), new Vector2(338f, 158f));

        AddAppGlyph(dress.rectTransform, "D+", "base stats / outfit bonus");
        AddAppGlyph(map.rectTransform, "MP", "area select");
        AddAppGlyph(quest.rectTransform, "Q!", "daily / route");
        AddAppGlyph(chars.rectTransform, "ID", "contacts");
        AddAppGlyph(items.rectTransform, "IT", "inventory");
        AddAppGlyph(save.rectTransform, "SV", "save");
        AddAppGlyph(settings.rectTransform, "ST", "config");
        AddAppGlyph(advice.rectTransform, "AI", "ReRe note");
        AddNotificationBadge(quest.rectTransform, "3");
        AddNotificationBadge(chars.rectTransform, "!");

        statusTileButton = EnsureButton(dress);
        itemsTileButton = EnsureButton(items);
        charactersTileButton = EnsureButton(chars);
        questTileButton = EnsureButton(quest);
        mapTileButton = EnsureButton(map);

        return page;
    }

    private static RectTransform BuildStatusPage(RectTransform parent)
    {
        var page = RectRoot("PageDressStatus", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        AddHeader(page, "DRESS ROOM", "outfit bonus / base status");

        var bg = ImageRoot("DressPixelBackdrop", page, new Color(1f, 0.96f, 0.88f, 1f));
        Stretch(bg.rectTransform, new Vector2(20f, 78f), new Vector2(-20f, -20f));
        bg.raycastTarget = false;
        PixelBorder(bg.rectTransform, "BackdropFrame", new Color(0.16f, 0.10f, 0.20f, 1f), 5f);
        AddPixelPattern(bg.rectTransform);

        var radarPanel = PixelPanel(page, "RadarPanel", new Vector2(44f, -116f), new Vector2(332f, 334f), new Color(0.98f, 0.86f, 0.94f, 1f));
        TextBox("STATUS RADAR", radarPanel.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Vector2(16f, -12f), new Vector2(300f, 30f));
        var baseRadar = RectRoot("BaseStatusRadar", radarPanel.rectTransform);
        SetRect(baseRadar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -14f), new Vector2(220f, 220f));
        var baseChart = baseRadar.gameObject.AddComponent<RadarChart>();
        baseChart.color = new Color(1f, 0.35f, 0.56f, 0.55f);
        baseChart.SetValues(3, 5, 4, 6, 3);
        var outfitRadar = RectRoot("OutfitAdjustedRadar", radarPanel.rectTransform);
        SetRect(outfitRadar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -14f), new Vector2(220f, 220f));
        var outfitChart = outfitRadar.gameObject.AddComponent<RadarChart>();
        outfitChart.color = new Color(1f, 0.82f, 0.12f, 0.55f);
        outfitChart.SetValues(6, 5, 7, 7, 4);
        AddRadarLegend(radarPanel.rectTransform);

        var mannequin = PixelPanel(page, "CharacterPreviewBooth", new Vector2(404f, -116f), new Vector2(338f, 440f), new Color(0.94f, 0.86f, 1f, 1f));
        TextBox("FITTING BOOTH", mannequin.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Vector2(16f, -12f), new Vector2(306f, 30f));
        AddBoothCurtain(mannequin.rectTransform);
        AddStandingReRePlaceholder(mannequin.rectTransform);
        var comment = PixelPanel(mannequin.rectTransform, "ReReCommentPanel", new Vector2(18f, -330f), new Vector2(302f, 94f), new Color(1f, 0.95f, 0.78f, 1f));
        TextBox("ReRe's COMMENT", comment.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.TopLeft, Ink,
            new Vector2(14f, -7f), new Vector2(180f, 20f));
        var face = ImageRoot("ReReFaceIcon", comment.rectTransform, Color.white);
        face.sprite = LoadSprite(TopReReSpritePath);
        face.preserveAspect = true;
        face.raycastTarget = false;
        SetRect(face.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -10f), new Vector2(58f, 58f));
        var commentText = TextBox("select outfit", comment.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.TopLeft, new Color(0.35f, 0.25f, 0.40f, 1f),
            new Vector2(14f, -29f), new Vector2(200f, 38f));
        commentText.name = "CommentText";
        var yes = ButtonRoot("YesButton", comment.rectTransform, Mint);
        SetRect(yes.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-132f, 10f), new Vector2(58f, 28f));
        TextBox("YES", yes.GetComponent<RectTransform>(), 12f, FontStyles.Bold, TextAlignmentOptions.Center, Ink, new Vector2(0f, 0f), new Vector2(58f, 28f));
        var no = ButtonRoot("NoButton", comment.rectTransform, Coral);
        SetRect(no.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-68f, 10f), new Vector2(50f, 28f));
        TextBox("NO", no.GetComponent<RectTransform>(), 12f, FontStyles.Bold, TextAlignmentOptions.Center, Ink, new Vector2(0f, 0f), new Vector2(50f, 28f));

        var bonus = PixelPanel(page, "OutfitBonusPanel", new Vector2(770f, -116f), new Vector2(310f, 334f), new Color(1f, 0.91f, 0.66f, 1f));
        TextBox("EQUIP BONUS", bonus.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Vector2(16f, -12f), new Vector2(278f, 30f));
        OutfitBonusRow(bonus.rectTransform, 0, "胆力", 1, Coral);
        OutfitBonusRow(bonus.rectTransform, 1, "知力", 1, Yellow);
        OutfitBonusRow(bonus.rectTransform, 2, "注意", 2, Mint);
        OutfitBonusRow(bonus.rectTransform, 3, "攻撃", 0, Cyan);
        OutfitBonusRow(bonus.rectTransform, 4, "防御", 1, Lavender);

        var closet = PixelPanel(page, "OutfitCards", new Vector2(44f, -480f), new Vector2(1036f, 126f), new Color(0.91f, 0.96f, 1f, 1f));
        TextBox("CLOSET", closet.rectTransform, 17f, FontStyles.Bold, TextAlignmentOptions.TopLeft, Ink,
            new Vector2(18f, -8f), new Vector2(150f, 26f));
        var names = new[] { "ROOM", "DATE", "WORK", "CYBER", "CASUAL", "LOCK" };
        var colors = new[] { Lavender, Coral, Mint, Cyan, Peach, new Color(0.72f, 0.72f, 0.76f, 1f) };
        for (int i = 0; i < 6; i++)
            OutfitCard(closet.rectTransform, i, names[i], colors[i], i == 0, i == 5);
        page.gameObject.AddComponent<DressMenuController>();
        return page;
    }

    private static RectTransform BuildItemsPage(RectTransform parent, out Button homeButton, out Button dressButton,
        out Button itemsButton, out Button charactersButton, out Button questButton, out Button mapButton)
    {
        const float artworkWidth = 1672f;
        const float artworkHeight = 941f;
        var page = RectRoot("PageItems", parent);
        Stretch(page, Vector2.zero, Vector2.zero);

        var phone = ImageRoot("ItemPhoneFrame", page, new Color(0.05f, 0.05f, 0.07f, 0.98f));
        phone.raycastTarget = false;
        SetRect(phone.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1672f, 941f));
        PixelBorder(phone.rectTransform, "OuterPixelFrame", new Color(0.01f, 0.01f, 0.02f, 1f), 9f);

        var body = ImageRoot("ItemPhoneBody", page, new Color(1f, 0.97f, 0.91f, 1f));
        body.raycastTarget = false;
        SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1576f, 846f));
        PixelBorder(body.rectTransform, "BodyPixelFrame", new Color(0.46f, 0.42f, 0.58f, 1f), 4f);

        TextBoxCentered("MoshiReRe", page, 45f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.28f, 0.22f, 0.56f, 1f),
            new Rect(760f, 42f, 360f, 64f), artworkWidth, artworkHeight);
        TextBoxCentered("ITEM", page, 31f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Rect(134f, 166f, 280f, 44f), artworkWidth, artworkHeight);

        homeButton = CharacterNavButton(page, "ItemsHomeButton", "HOME", new Rect(120f, 82f, 166f, 58f), Lavender, artworkWidth, artworkHeight);
        dressButton = CharacterNavButton(page, "ItemsDressButton", "DRESS", new Rect(294f, 82f, 66f, 58f), Lavender, artworkWidth, artworkHeight);
        itemsButton = CharacterNavButton(page, "ItemsItemsButton", "ITEM", new Rect(370f, 82f, 58f, 58f), Yellow, artworkWidth, artworkHeight);
        charactersButton = CharacterNavButton(page, "ItemsCharactersButton", "CHAR", new Rect(438f, 82f, 60f, 58f), Coral, artworkWidth, artworkHeight);
        questButton = CharacterNavButton(page, "ItemsQuestButton", "QUEST", new Rect(508f, 82f, 68f, 58f), Yellow, artworkWidth, artworkHeight);
        mapButton = CharacterNavButton(page, "ItemsMapButton", "MAP", new Rect(586f, 82f, 58f, 58f), Cyan, artworkWidth, artworkHeight);

        var categoryPanel = PixelPanelAt(page, "ItemCategoryTabs", new Rect(128f, 220f, 132f, 446f), new Color(0.93f, 0.92f, 0.98f, 1f), artworkWidth, artworkHeight);
        LocalPixelButton(categoryPanel.rectTransform, "BagCategoryButton", "BAG", new Vector2(12f, -22f), new Vector2(108f, 68f), Mint);
        LocalPixelButton(categoryPanel.rectTransform, "KeyCategoryButton", "KEY", new Vector2(12f, -108f), new Vector2(108f, 68f), Cream);
        LocalPixelButton(categoryPanel.rectTransform, "GiftCategoryButton", "GIFT", new Vector2(12f, -194f), new Vector2(108f, 68f), Coral);
        LocalPixelButton(categoryPanel.rectTransform, "AllCategoryButton", "ALL", new Vector2(12f, -280f), new Vector2(108f, 68f), Lavender);

        var grid = PixelPanelAt(page, "ItemInventoryGrid", new Rect(282f, 220f, 556f, 446f), new Color(0.94f, 0.98f, 0.96f, 1f), artworkWidth, artworkHeight);
        TextBox("ITEM LIST", grid.rectTransform, 23f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(24f, -18f), new Vector2(260f, 34f));
        var openBagPreview = ImageRoot("OpenBagInventoryArtwork", grid.rectTransform, Color.white);
        openBagPreview.sprite = LoadSprite(ItemBagOpenSpritePath);
        openBagPreview.preserveAspect = true;
        openBagPreview.raycastTarget = false;
        SetRect(openBagPreview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -24f), new Vector2(520f, 392f));
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 4; x++)
            ItemDraftCard(grid.rectTransform, y * 4 + x, new Vector2(72f + x * 106f, -116f - y * 112f), new Vector2(92f, 96f),
                new Color(0.96f, 0.94f, 0.99f, 0.90f));

        var detail = PixelPanelAt(page, "SelectedItemDetail", new Rect(878f, 220f, 372f, 300f), new Color(1f, 0.94f, 0.84f, 1f), artworkWidth, artworkHeight);
        TextBox("SELECTED ITEM", detail.rectTransform, 23f, FontStyles.Bold, TextAlignmentOptions.Center, Ink, new Vector2(20f, -16f), new Vector2(332f, 34f));
        var detailIcon = ImageRoot("DetailItemIcon", detail.rectTransform, Coral);
        detailIcon.raycastTarget = false;
        SetRect(detailIcon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -70f), new Vector2(112f, 112f));
        PixelBorder(detailIcon.rectTransform, "DetailIconFrame", Ink, 3f);
        var detailTitle = TextBox("Lucky Charm", detail.rectTransform, 24f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(164f, -70f), new Vector2(180f, 34f));
        detailTitle.name = "DetailTitle";
        var detailDescription = TextBox("A small charm for uncertain conversations.", detail.rectTransform, 20f, FontStyles.Bold,
            TextAlignmentOptions.TopLeft, new Color(0.34f, 0.27f, 0.42f, 1f), new Vector2(164f, -114f), new Vector2(174f, 104f));
        detailDescription.name = "DetailDescription";
        TextBox("RELATED", detail.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(28f, -210f), new Vector2(130f, 28f));
        for (var i = 0; i < 3; i++)
            Circle(detail.rectTransform, "RelatedCharacter" + i, new Vector2(128f + i * 62f, -204f), 48f, i == 0 ? Coral : i == 1 ? Lavender : Yellow);

        var rerePanel = PixelPanelAt(page, "ReReItemCommentPanel", new Rect(1270f, 220f, 260f, 300f), new Color(1f, 0.90f, 0.91f, 1f), artworkWidth, artworkHeight);
        TextBox("ReRe NOTE", rerePanel.rectTransform, 22f, FontStyles.Bold, TextAlignmentOptions.Center, Ink, new Vector2(18f, -16f), new Vector2(224f, 32f));
        var rereFace = ImageRoot("ReReFacePreview", rerePanel.rectTransform, Color.white);
        rereFace.sprite = LoadSprite(CommonUiCropFolder + "/rere_guide.png");
        rereFace.preserveAspect = true;
        rereFace.raycastTarget = false;
        SetRect(rereFace.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-88f, -54f), new Vector2(70f, 70f));
        var rereBubble = ImageRoot("ReReItemBubble", rerePanel.rectTransform, new Color(1f, 0.98f, 0.86f, 1f));
        rereBubble.raycastTarget = false;
        SetRect(rereBubble.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-10f, -120f), new Vector2(-42f, 96f));
        PixelBorder(rereBubble.rectTransform, "BubbleFrame", Ink, 3f);
        var rereText = Text("Drag an item into the bag to carry it.", rereBubble.rectTransform, 18f, FontStyles.Bold,
            TextAlignmentOptions.TopLeft, Ink, new Vector2(14f, 12f), new Vector2(-14f, -12f));
        rereText.name = "ReReCommentText";
        var addButton = LocalPixelButton(rerePanel.rectTransform, "AddToBagButton", "BAG +", new Vector2(28f, -238f), new Vector2(92f, 42f), Mint);
        addButton.gameObject.SetActive(false);
        var confirmButton = LocalPixelButton(rerePanel.rectTransform, "ConfirmBagButton", "YES", new Vector2(138f, -238f), new Vector2(92f, 42f), Coral);

        var bagPanel = PixelPanelAt(page, "CarryBagPanel", new Rect(128f, 696f, 1402f, 128f), new Color(0.94f, 0.91f, 0.99f, 1f), artworkWidth, artworkHeight);
        TextBox("CARRY BAG", bagPanel.rectTransform, 26f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(28f, -18f), new Vector2(210f, 38f));
        var statusText = TextBox("0/4 packed", bagPanel.rectTransform, 19f, FontStyles.Bold, TextAlignmentOptions.Left, new Color(0.36f, 0.28f, 0.46f, 1f),
            new Vector2(32f, -64f), new Vector2(180f, 30f));
        statusText.name = "BagStatusText";
        for (var i = 0; i < 4; i++)
            BagSlot(bagPanel.rectTransform, i, new Vector2(260f + i * 222f, -28f), new Vector2(184f, 72f));
        TextBox("選んだアイテムをバッグへ", bagPanel.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.42f, 0.34f, 0.52f, 1f),
            new Vector2(1160f, -42f), new Vector2(198f, 42f));

        BuildBagZipOverlay(page, artworkWidth, artworkHeight);

        var controller = page.gameObject.AddComponent<ItemMenuController>();
        var so = new SerializedObject(controller);
        SetObject(so, "inventoryDatabase", FindFirstAssetOfType<InventoryDatabase>());
        SetObject(so, "bagDropArea", bagPanel.rectTransform);
        SetObject(so, "addToBagButton", addButton);
        SetObject(so, "confirmBagButton", confirmButton);
        SetObject(so, "openBagSprite", LoadSprite(ItemBagOpenSpritePath));
        SetObject(so, "closedBagSprite", LoadSprite(ItemBagClosedSpritePath));
        SetSpriteArray(so, "bagZipStages", LoadSpritesFromSheet(ItemBagStatesSheetPath));
        SetObject(so, "zipperHookSprite", LoadSprite(ItemBagHookSpritePath));
        SetSpriteArray(so, "zipperReReFrames", LoadSpritesFromFolder(ItemBagReReZipFolder));
        SetFloat(so, "zipTravelSeconds", 2.15f);
        SetFloat(so, "zipReReFrameRate", 6f);
        SetVector2(so, "zipReReSize", new Vector2(230f, 230f));
        SetFloat(so, "bagStageDisplayWidth", 540f);
        SetFloat(so, "bagStageBottomY", -312f);
        SetVector2(so, "zipperPathCenter", new Vector2(-10f, 24f));
        SetVector2(so, "zipperPathHalfSize", new Vector2(232f, 160f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
        BuildUnifiedPageNavigation(page, artworkWidth, artworkHeight);
        return page;
    }

    private static RectTransform BuildCharactersPage(RectTransform parent, out Button homeButton, out Button dressButton,
        out Button itemsButton, out Button charactersButton, out Button questButton, out Button mapButton)
    {
        const float artworkWidth = 1672f;
        const float artworkHeight = 941f;
        var page = RectRoot("PageCharacters", parent);
        Stretch(page, Vector2.zero, Vector2.zero);

        var phone = ImageRoot("CharacterPhoneFrame", page, new Color(0.05f, 0.05f, 0.07f, 0.98f));
        phone.raycastTarget = false;
        SetRect(phone.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1672f, 941f));
        PixelBorder(phone.rectTransform, "OuterPixelFrame", new Color(0.01f, 0.01f, 0.02f, 1f), 9f);

        var body = ImageRoot("CharacterPhoneBody", page, new Color(1f, 0.97f, 0.91f, 1f));
        body.raycastTarget = false;
        SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1576f, 846f));
        PixelBorder(body.rectTransform, "BodyPixelFrame", new Color(0.46f, 0.42f, 0.58f, 1f), 4f);
        TextBoxCentered("MoshiReRe", page, 45f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.28f, 0.22f, 0.56f, 1f),
            new Rect(608f, 42f, 456f, 64f), artworkWidth, artworkHeight);
        TextBoxCentered("▂▃▆", page, 23f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.31f, 0.39f, 0.54f, 1f),
            new Rect(140f, 62f, 74f, 38f), artworkWidth, artworkHeight);
        TextBoxCentered("▰▰▰", page, 23f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.24f, 0.67f, 0.66f, 1f),
            new Rect(1446f, 62f, 88f, 38f), artworkWidth, artworkHeight);

        homeButton = CharacterNavButton(page, "CharactersHomeButton", "⌂ HOME", new Rect(120f, 82f, 166f, 58f), Lavender, artworkWidth, artworkHeight);
        dressButton = CharacterNavButton(page, "CharactersDressButton", "DRESS", new Rect(332f, 82f, 84f, 58f), Lavender, artworkWidth, artworkHeight);
        itemsButton = CharacterNavButton(page, "CharactersItemsButton", "ITEM", new Rect(430f, 82f, 76f, 58f), new Color(0.86f, 0.84f, 0.88f, 1f), artworkWidth, artworkHeight);
        charactersButton = CharacterNavButton(page, "CharactersCharactersButton", "CHAR", new Rect(520f, 82f, 88f, 58f), Coral, artworkWidth, artworkHeight);
        questButton = CharacterNavButton(page, "CharactersQuestButton", "QUEST", new Rect(622f, 82f, 88f, 58f), Yellow, artworkWidth, artworkHeight);
        mapButton = CharacterNavButton(page, "CharactersMapButton", "MAP", new Rect(724f, 82f, 76f, 58f), Cyan, artworkWidth, artworkHeight);

        TextBoxCentered("CHARACTER CONTACT", page, 30f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Rect(134f, 166f, 430f, 44f), artworkWidth, artworkHeight);
        var contactsTab = CharacterTabButton(page, "ContactsTabButton", "CONTACT", new Rect(968f, 158f, 188f, 48f), Mint, artworkWidth, artworkHeight);
        var relationTab = CharacterTabButton(page, "RelationTabButton", "RELATION", new Rect(1172f, 158f, 188f, 48f), Lavender, artworkWidth, artworkHeight);

        var contactsPage = RectRoot("ContactsSubPage", page);
        Stretch(contactsPage, Vector2.zero, Vector2.zero);
        var ojiTab = CharacterTabButton(contactsPage, "OjiFilter", "OJI", new Rect(150f, 224f, 236f, 62f), Cyan, artworkWidth, artworkHeight);
        var itadakiTab = CharacterTabButton(contactsPage, "ItadakiFilter", "ITADAKI", new Rect(404f, 224f, 236f, 62f), Coral, artworkWidth, artworkHeight);
        var ojiListRoot = CharacterScrollList(contactsPage, "OjiContactScroll", new Rect(150f, 306f, 536f, 408f), artworkWidth, artworkHeight, out var ojiList);
        var itadakiListRoot = CharacterScrollList(contactsPage, "ItadakiContactScroll", new Rect(150f, 306f, 536f, 408f), artworkWidth, artworkHeight, out var itadakiList);
        itadakiListRoot.gameObject.SetActive(false);
        var database = FindFirstAssetOfType<CharacterDatabase>();
        var ojiCharacters = GetCharactersByCategory(database, CharacterCategory.Oj, 6);
        var itadakiCharacters = GetCharactersByCategory(database, CharacterCategory.Itadaki, 6);
        for (int i = 0; i < ojiCharacters.Length; i++)
            CharacterContactRow(ojiList, i, GetContactName(ojiCharacters[i]), ojiCharacters[i] ? "OJI" : "???", false,
                ojiCharacters[i] ? Cyan : new Color(0.76f, 0.76f, 0.78f, 1f), new Rect(10f, 10f + i * 106f, 500f, 92f), 536f, 744f);
        for (int i = 0; i < itadakiCharacters.Length; i++)
            CharacterContactRow(itadakiList, i, GetContactName(itadakiCharacters[i]), itadakiCharacters[i] ? "ITADAKI" : "???", false,
                itadakiCharacters[i] ? Coral : new Color(0.76f, 0.76f, 0.78f, 1f), new Rect(10f, 10f + i * 106f, 500f, 92f), 536f, 744f);

        var detail = PixelPanelAt(contactsPage, "SelectedCharacterDetail", new Rect(724f, 224f, 794f, 410f), new Color(1f, 0.94f, 0.86f, 1f), artworkWidth, artworkHeight);
        CircleAt(detail.rectTransform, "SelectedPortrait", new Rect(34f, 34f, 188f, 188f), Lavender);
        TextBox("GRANDPA HIKO", detail.rectTransform, 31f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(246f, -34f), new Vector2(508f, 42f));
        AttributeChip(detail.rectTransform, "GIVER", new Vector2(250f, -86f), new Color(0.42f, 0.74f, 0.45f, 1f));
        TextBox("RELATIONSHIP\nGood friends. He cares deeply about everyone in town.", detail.rectTransform, 21f, FontStyles.Bold, TextAlignmentOptions.TopLeft,
            new Color(0.30f, 0.25f, 0.38f, 1f), new Vector2(250f, -142f), new Vector2(492f, 110f));
        TextBox("AREA    Moshi Village", detail.rectTransform, 23f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(38f, -274f), new Vector2(514f, 42f));
        LocalPixelButton(detail.rectTransform, "GoAreaButton", "GO >", new Vector2(620f, -292f), new Vector2(120f, 48f), Lavender);

        var nodePanel = PixelPanelAt(contactsPage, "CharacterInformationNodes", new Rect(724f, 224f, 794f, 410f), new Color(0.92f, 0.96f, 1f, 1f), artworkWidth, artworkHeight);
        var selectedNodeCharacterText = TextBox("CHARACTER INTEL", nodePanel.rectTransform, 27f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(28f, -24f), new Vector2(724f, 38f));
        TextBox("RE: INFORMATION NODES", nodePanel.rectTransform, 17f, FontStyles.Bold, TextAlignmentOptions.Left, new Color(0.31f, 0.39f, 0.54f, 1f),
            new Vector2(28f, -68f), new Vector2(460f, 30f));
        var emptyNodeText = TextBox("NO INTEL NODES REGISTERED", nodePanel.rectTransform, 19f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.42f, 0.42f, 0.50f, 1f),
            new Vector2(40f, -192f), new Vector2(714f, 46f));
        var nodeListRoot = RectRoot("InformationNodeList", nodePanel.rectTransform);
        SetRect(nodeListRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -108f), new Vector2(738f, 272f));
        var nodeTemplate = Panel(nodeListRoot, "InformationNodeRowTemplate", Vector2.zero, new Vector2(738f, 76f), new Color(0.68f, 0.70f, 0.76f, 1f));
        PixelBorder(nodeTemplate.rectTransform, "NodeFrame", Ink, 2f);
        TextBox("NODE", nodeTemplate.rectTransform, 19f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(14f, -8f), new Vector2(270f, 26f));
        TextBox("CATEGORY  CONFIDENCE", nodeTemplate.rectTransform, 15f, FontStyles.Bold, TextAlignmentOptions.Right, Ink, new Vector2(304f, -10f), new Vector2(416f, 24f));
        TextBox("CONTENT", nodeTemplate.rectTransform, 16f, FontStyles.Normal, TextAlignmentOptions.Left, Ink, new Vector2(14f, -38f), new Vector2(708f, 28f));
        nodeTemplate.gameObject.SetActive(false);

        var memo = PixelPanelAt(contactsPage, "ReReMemo", new Rect(760f, 660f, 700f, 112f), new Color(1f, 0.98f, 0.90f, 1f), artworkWidth, artworkHeight);
        var memoFace = CircleAt(memo.rectTransform, "ReReFace", new Rect(20f, 18f, 76f, 76f), Color.white);
        memoFace.sprite = LoadSprite(CommonUiCropFolder + "/rere_happy.png");
        memoFace.preserveAspect = true;
        TextBox("ReRe COMMENT\nこの人はクエストに関係してそう。話しかけに行く？", memo.rectTransform, 20f, FontStyles.Bold,
            TextAlignmentOptions.TopLeft, new Color(0.32f, 0.25f, 0.45f, 1f), new Vector2(116f, -18f), new Vector2(548f, 74f));

        var relationPage = RectRoot("RelationSubPage", page);
        Stretch(relationPage, Vector2.zero, Vector2.zero);
        relationPage.gameObject.SetActive(false);
        var relationBoard = PixelPanelAt(relationPage, "RelationBoard", new Rect(154f, 226f, 900f, 544f), new Color(0.98f, 0.96f, 0.90f, 1f), artworkWidth, artworkHeight);
        TextBox("AI RELATION", relationBoard.rectTransform, 28f, FontStyles.Bold, TextAlignmentOptions.Center, Ink, new Vector2(24f, -18f), new Vector2(852f, 42f));
        RelationLine(relationBoard.rectTransform, new Vector2(450f, -274f), new Vector2(236f, -164f), new Color(0.42f, 0.74f, 0.45f, 0.8f));
        RelationLine(relationBoard.rectTransform, new Vector2(450f, -274f), new Vector2(650f, -164f), new Color(0.94f, 0.43f, 0.43f, 0.8f));
        RelationLine(relationBoard.rectTransform, new Vector2(450f, -274f), new Vector2(250f, -390f), new Color(0.94f, 0.74f, 0.28f, 0.8f));
        RelationLine(relationBoard.rectTransform, new Vector2(450f, -274f), new Vector2(662f, -392f), new Color(0.56f, 0.56f, 0.62f, 0.8f));
        CharacterRelationNode(relationBoard.rectTransform, "ReRe", new Vector2(450f, -274f), Lavender, "AI", "ReRe is checking everyone's ties.");
        CharacterRelationNode(relationBoard.rectTransform, "Yui", new Vector2(236f, -164f), Coral, "G", "Yui has a quest flag. Her links look active.");
        CharacterRelationNode(relationBoard.rectTransform, "Uma", new Vector2(650f, -164f), Peach, "T", "Uma may pull resources from nearby contacts.");
        CharacterRelationNode(relationBoard.rectTransform, "Hiko", new Vector2(250f, -390f), Cyan, "M", "Hiko is a useful bridge to this area.");
        CharacterRelationNode(relationBoard.rectTransform, "???", new Vector2(662f, -392f), new Color(0.72f, 0.72f, 0.76f, 1f), "?", "Unknown contact. ReRe needs more story clues.");
        var relationDetail = PixelPanelAt(relationPage, "RelationDetail", new Rect(1090f, 226f, 386f, 544f), new Color(0.92f, 0.96f, 1f, 1f), artworkWidth, artworkHeight);
        TextBox("SELECTED\nGRANDPA HIKO", relationDetail.rectTransform, 25f, FontStyles.Bold, TextAlignmentOptions.TopLeft, Ink,
            new Vector2(28f, -28f), new Vector2(330f, 80f));
        AttributeChip(relationDetail.rectTransform, "QUEST!", new Vector2(30f, -132f), Coral);
        TextBox("関連人物と現在クエストのつながりをReReが解析中。", relationDetail.rectTransform, 21f, FontStyles.Bold,
            TextAlignmentOptions.TopLeft, Ink, new Vector2(30f, -196f), new Vector2(326f, 150f));
        LocalPixelButton(relationDetail.rectTransform, "RelationGoAreaButton", "GO AREA", new Vector2(32f, -424f), new Vector2(322f, 64f), Lavender);
        var hintBubble = PixelPanelAt(relationPage, "RelationHintBubble", new Rect(690f, 636f, 360f, 118f), new Color(1f, 0.98f, 0.90f, 1f), artworkWidth, artworkHeight);
        hintBubble.gameObject.SetActive(false);
        var hintText = TextBox("ReRe memo", hintBubble.rectTransform, 19f, FontStyles.Bold, TextAlignmentOptions.TopLeft, Ink,
            new Vector2(22f, -18f), new Vector2(310f, 78f));
        hintText.name = "RelationHintText";

        var controller = page.gameObject.AddComponent<CharacterMenuController>();
        var so = new SerializedObject(controller);
        SetObject(so, "contactsTabButton", contactsTab);
        SetObject(so, "relationTabButton", relationTab);
        SetObject(so, "ojiTabButton", ojiTab);
        SetObject(so, "itadakiTabButton", itadakiTab);
        SetObject(so, "contactsPage", contactsPage.gameObject);
        SetObject(so, "relationPage", relationPage.gameObject);
        SetObject(so, "ojiListRoot", ojiListRoot.gameObject);
        SetObject(so, "itadakiListRoot", itadakiListRoot.gameObject);
        SetObject(so, "relationHintBubble", hintBubble.rectTransform);
        SetObject(so, "relationHintText", hintText);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);

        var informationPanel = page.gameObject.AddComponent<CharacterInformationNodePanel>();
        var informationPanelSo = new SerializedObject(informationPanel);
        SetObject(informationPanelSo, "characterDatabase", database);
        SetObject(informationPanelSo, "selectedCharacterText", selectedNodeCharacterText);
        SetObject(informationPanelSo, "emptyText", emptyNodeText);
        SetObject(informationPanelSo, "nodeListRoot", nodeListRoot);
        SetObject(informationPanelSo, "nodeRowPrefab", nodeTemplate.gameObject);
        var rows = new Object[ojiList.childCount + itadakiList.childCount];
        for (var i = 0; i < ojiList.childCount; i++) rows[i] = ojiList.GetChild(i).GetComponent<Button>();
        for (var i = 0; i < itadakiList.childCount; i++) rows[ojiList.childCount + i] = itadakiList.GetChild(i).GetComponent<Button>();
        SetQuestObjectArray(informationPanelSo, "characterRowButtons", rows);
        var rowIndexes = new int[rows.Length];
        for (var i = 0; i < rowIndexes.Length; i++)
        {
            var character = i < ojiCharacters.Length ? ojiCharacters[i] : itadakiCharacters[i - ojiCharacters.Length];
            rowIndexes[i] = FindCharacterIndex(database, character);
            if (rowIndexes[i] < 0 && rows[i] is Button button)
                button.interactable = false;
        }
        SetIntArray(informationPanelSo, "characterRowIndexes", rowIndexes);
        informationPanelSo.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(informationPanel);
        BuildUnifiedPageNavigation(page, artworkWidth, artworkHeight);
        return page;
    }

    private static RectTransform BuildQuestPage(RectTransform parent)
    {
        var page = RectRoot("PageQuest", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        var headerStrip = ImageRoot("QuestHeaderStrip", page, new Color(0.98f, 0.94f, 0.84f, 1f));
        headerStrip.raycastTarget = false;
        SetRect(headerStrip.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(0f, 70f));
        PixelBorder(headerStrip.rectTransform, "HeaderFrame", Ink, 3f);
        AddHeader(page, "QUEST", "ReRe inbox / case board");

        var inboxTab = QuestTabButton(page, "QuestInboxTab", "INBOX", new Vector2(32f, -72f), new Vector2(214f, 48f), Cream);
        var caseBoardTab = QuestTabButton(page, "QuestCaseBoardTab", "CASE BOARD", new Vector2(254f, -72f), new Vector2(244f, 48f), Lavender);

        var inboxRoot = RectRoot("QuestInboxRoot", page);
        Stretch(inboxRoot, Vector2.zero, Vector2.zero);
        TextMeshProUGUI activeQuestTitleText;
        TextMeshProUGUI activeQuestObjectiveText;
        TextMeshProUGUI activeQuestProgressText;
        TextMeshProUGUI activeQuestHintText;
        TextMeshProUGUI activeQuestRewardText;
        Image[] inboxCardImages;
        var inboxCards = BuildQuestInbox(inboxRoot, out inboxCardImages, out activeQuestTitleText,
            out activeQuestObjectiveText, out activeQuestProgressText, out activeQuestHintText, out activeQuestRewardText);

        var caseBoardRoot = RectRoot("QuestCaseBoardRoot", page);
        Stretch(caseBoardRoot, Vector2.zero, Vector2.zero);
        BuildQuestCaseBoard(caseBoardRoot);
        caseBoardRoot.gameObject.SetActive(false);

        var controller = page.gameObject.AddComponent<QuestMenuController>();
        var so = new SerializedObject(controller);
        SetObject(so, "inboxRoot", inboxRoot.gameObject);
        SetObject(so, "caseBoardRoot", caseBoardRoot.gameObject);
        SetObject(so, "inboxTabButton", inboxTab);
        SetObject(so, "caseBoardTabButton", caseBoardTab);
        SetQuestObjectArray(so, "inboxQuestButtons", inboxCards);
        SetQuestObjectArray(so, "inboxCardImages", inboxCardImages);
        SetObject(so, "activeQuestTitleText", activeQuestTitleText);
        SetObject(so, "activeQuestObjectiveText", activeQuestObjectiveText);
        SetObject(so, "activeQuestProgressText", activeQuestProgressText);
        SetObject(so, "activeQuestHintText", activeQuestHintText);
        SetObject(so, "activeQuestRewardText", activeQuestRewardText);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
        return page;
    }

    private static Button[] BuildQuestInbox(RectTransform parent, out Image[] cardImages,
        out TextMeshProUGUI titleText, out TextMeshProUGUI objectiveText, out TextMeshProUGUI progressText,
        out TextMeshProUGUI hintText, out TextMeshProUGUI rewardText)
    {
        var cards = new Button[1];
        cardImages = new Image[cards.Length];
        var list = PixelPanel(parent, "QuestInboxList", new Vector2(32f, -132f), new Vector2(294f, 554f), Cream);
        TextBox("ReRe INBOX", list.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(18f, -16f), new Vector2(258f, 34f));
        AddNotificationBadge(list.rectTransform, "!");

        var names = new[] { "メインクエスト" };
        var marks = new[] { "!", "?", "♥", "→" };
        var colors = new[] { Coral };
        for (var i = 0; i < cards.Length; i++)
        {
            cards[i] = QuestInboxCard(list.rectTransform, i, names[i], marks[i], colors[i]);
            cardImages[i] = cards[i].GetComponent<Image>();
        }

        var detail = PixelPanel(parent, "QuestActivePanel", new Vector2(344f, -132f), new Vector2(492f, 554f), new Color(1f, 0.95f, 0.85f, 1f));
        TextBox("ACTIVE", detail.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(18f, -16f), new Vector2(456f, 34f));
        TextBox("★", detail.rectTransform, 24f, FontStyles.Bold, TextAlignmentOptions.Right, Yellow,
            new Vector2(18f, -16f), new Vector2(456f, 34f));

        var activeCard = Panel(detail.rectTransform, "ActiveQuestSummary", new Vector2(18f, -62f), new Vector2(456f, 148f), new Color(1f, 0.90f, 0.88f, 1f));
        Circle(activeCard.rectTransform, "QuestIcon", new Vector2(18f, -26f), 74f, Coral);
        Text("!", activeCard.rectTransform, 28f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white,
            new Vector2(18f, -26f), new Vector2(92f, 48f));
        titleText = TextBox("メインクエストはありません", activeCard.rectTransform, 24f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(112f, -22f), new Vector2(326f, 34f));
        TextBox("OBJECTIVE", activeCard.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.Left, new Color(0.38f, 0.28f, 0.42f, 1f),
            new Vector2(112f, -60f), new Vector2(326f, 22f));
        objectiveText = TextBox("シナリオ中にクエストが設定されます", activeCard.rectTransform, 17f, FontStyles.Normal, TextAlignmentOptions.Left, Ink,
            new Vector2(112f, -86f), new Vector2(326f, 28f));
        progressText = TextBox("期限: --", activeCard.rectTransform, 16f, FontStyles.Bold, TextAlignmentOptions.Right, Coral,
            new Vector2(112f, -116f), new Vector2(326f, 24f));

        var hint = PixelPanel(detail.rectTransform, "QuestHintPanel", new Vector2(18f, -224f), new Vector2(456f, 74f), new Color(0.70f, 0.92f, 0.89f, 1f));
        TextBox("HINT", hint.rectTransform, 16f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(16f, -14f), new Vector2(94f, 26f));
        hintText = TextBox("現在のクエストを確認しよう", hint.rectTransform, 15f, FontStyles.Normal, TextAlignmentOptions.Left, Ink,
            new Vector2(90f, -14f), new Vector2(348f, 26f));
        var questReRe = ImageRoot("QuestReReIdea", hint.rectTransform, Color.white);
        questReRe.sprite = LoadSprite(CommonUiCropFolder + "/rere_idea.png");
        questReRe.preserveAspect = true;
        questReRe.raycastTarget = false;
        SetRect(questReRe.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(54f, 54f));
        TextBox("›", hint.rectTransform, 24f, FontStyles.Bold, TextAlignmentOptions.Right, Ink,
            new Vector2(16f, -14f), new Vector2(424f, 26f));

        QuestRouteStrip(detail.rectTransform, new Vector2(18f, -314f), new Vector2(456f, 126f));

        var action = PixelPanel(parent, "QuestActionPanel", new Vector2(852f, -132f), new Vector2(246f, 554f), new Color(0.91f, 0.87f, 0.98f, 1f));
        TextBox("GO?", action.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Vector2(16f, -16f), new Vector2(214f, 32f));
        var go = ButtonRoot("GoToAreaButton", action.rectTransform, Mint);
        SetRect(go.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(16f, -58f), new Vector2(-32f, 64f));
        PixelBorder(go.GetComponent<RectTransform>(), "GoFrame", Ink, 3f);
        Text("YES", go.GetComponent<RectTransform>(), 24f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);

        TextBox("RELATED", action.rectTransform, 14f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(18f, -148f), new Vector2(210f, 22f));
        QuestCharacterChip(action.rectTransform, "ReRe", "R", new Vector2(18f, -184f), Coral);
        QuestCharacterChip(action.rectTransform, "Yui", "Y", new Vector2(88f, -184f), Lavender);
        QuestCharacterChip(action.rectTransform, "OJI", "O", new Vector2(158f, -184f), Yellow);

        TextBox("REWARD", action.rectTransform, 14f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(18f, -278f), new Vector2(210f, 22f));
        var reward = PixelPanel(action.rectTransform, "QuestReward", new Vector2(18f, -310f), new Vector2(210f, 78f), new Color(1f, 0.91f, 0.69f, 1f));
        rewardText = Text("★ 200", reward.rectTransform, 22f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        TextBox("carry bag", action.rectTransform, 13f, FontStyles.Normal, TextAlignmentOptions.Center, Ink,
            new Vector2(18f, -416f), new Vector2(210f, 22f));

        return cards;
    }

    private static void SetQuestObjectArray(SerializedObject so, string name, Object[] values)
    {
        var property = so.FindProperty(name);
        if (property == null) return;
        property.arraySize = values.Length;
        for (var i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void BuildQuestCaseBoard(RectTransform parent)
    {
        var list = PixelPanel(parent, "QuestCaseList", new Vector2(32f, -132f), new Vector2(294f, 554f), new Color(0.91f, 0.88f, 0.98f, 1f));
        TextBox("CASE", list.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Vector2(18f, -16f), new Vector2(258f, 34f));
        var names = new[] { "AI WHISPER", "SMILE CODE", "FAKE FRIEND", "MEMORIES" };
        var marks = new[] { "▣", "♥", "☺", "▤" };
        var colors = new[] { Lavender, Mint, Coral, new Color(0.72f, 0.68f, 0.88f, 1f) };
        for (var i = 0; i < names.Length; i++)
        {
            var card = Panel(list.rectTransform, "CaseCard" + i, new Vector2(16f, -58f - i * 112f), new Vector2(262f, 92f), i == 0 ? new Color(1f, 0.91f, 0.69f, 1f) : Cream);
            PixelBorder(card.rectTransform, "CaseFrame", Ink, 3f);
            Circle(card.rectTransform, "CaseIcon", new Vector2(16f, -16f), 54f, colors[i]);
            Text(marks[i], card.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
                new Vector2(16f, -16f), new Vector2(70f, 34f));
            TextBox(names[i], card.rectTransform, 15f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
                new Vector2(84f, -18f), new Vector2(166f, 24f));
            AddMiniBars(card.rectTransform, new Vector2(86f, -54f), 3);
            TextBox((i + 1) + "/6", card.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.Right, Ink,
                new Vector2(84f, -54f), new Vector2(166f, 22f));
        }

        var board = PixelPanel(parent, "QuestCaseBoard", new Vector2(344f, -132f), new Vector2(492f, 554f), new Color(1f, 0.91f, 0.70f, 1f));
        TextBox("CASE BOARD", board.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(18f, -16f), new Vector2(456f, 34f));
        TextBox("ACTIVE CASE", board.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.Right, Coral,
            new Vector2(18f, -18f), new Vector2(456f, 28f));

        RelationLine(board.rectTransform, new Vector2(130f, -182f), new Vector2(246f, -128f), Coral);
        RelationLine(board.rectTransform, new Vector2(246f, -128f), new Vector2(366f, -188f), Coral);
        RelationLine(board.rectTransform, new Vector2(246f, -128f), new Vector2(246f, -286f), Lavender);
        RelationLine(board.rectTransform, new Vector2(246f, -286f), new Vector2(366f, -346f), Mint);
        QuestCaseNode(board.rectTransform, "Yui", "Y", new Vector2(88f, -146f), Coral);
        QuestCaseNode(board.rectTransform, "ReRe", "R", new Vector2(204f, -94f), Lavender);
        QuestCaseNode(board.rectTransform, "OJI", "O", new Vector2(324f, -154f), Coral);
        QuestCaseNode(board.rectTransform, "MAP", "M", new Vector2(204f, -252f), Mint);
        QuestCaseNode(board.rectTransform, "?", "?", new Vector2(324f, -312f), new Color(0.60f, 0.58f, 0.72f, 1f));
        var note = PixelPanel(board.rectTransform, "CaseNote", new Vector2(18f, -432f), new Vector2(456f, 74f), new Color(0.96f, 0.94f, 0.82f, 1f));
        TextBox("ReRe is connecting the clues...", note.rectTransform, 15f, FontStyles.Normal, TextAlignmentOptions.Center, Ink,
            new Vector2(14f, -20f), new Vector2(428f, 34f));

        var action = PixelPanel(parent, "QuestCaseAction", new Vector2(852f, -132f), new Vector2(246f, 554f), new Color(0.98f, 0.86f, 0.90f, 1f));
        TextBox("HINT", action.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Vector2(16f, -16f), new Vector2(214f, 32f));
        var hint = PixelPanel(action.rectTransform, "CaseHint", new Vector2(16f, -62f), new Vector2(214f, 126f), new Color(0.70f, 0.92f, 0.89f, 1f));
        TextBox("The clue is near\nthe next route.", hint.rectTransform, 16f, FontStyles.Normal, TextAlignmentOptions.Center, Ink,
            new Vector2(12f, -28f), new Vector2(190f, 56f));
        TextBox("RELATED", action.rectTransform, 14f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(18f, -224f), new Vector2(210f, 22f));
        QuestCharacterChip(action.rectTransform, "Yui", "Y", new Vector2(18f, -260f), Coral);
        QuestCharacterChip(action.rectTransform, "OJI", "O", new Vector2(88f, -260f), Yellow);
        TextBox("REWARD", action.rectTransform, 14f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(18f, -352f), new Vector2(210f, 22f));
        var go = ButtonRoot("CaseGoToAreaButton", action.rectTransform, Mint);
        SetRect(go.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(16f, -416f), new Vector2(-32f, 60f));
        PixelBorder(go.GetComponent<RectTransform>(), "GoFrame", Ink, 3f);
        Text("GO?", go.GetComponent<RectTransform>(), 22f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
    }

    private static RectTransform BuildSavePage(RectTransform parent)
    {
        var page = RectRoot("PageSave", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        var frame = PixelPanel(page, "SaveFrame", new Vector2(20f, -20f), new Vector2(1320f, 770f), new Color(0.96f, 0.92f, 0.84f, 1f));
        TextBox("SAVE DATA", frame.rectTransform, 38f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(30f, -28f), new Vector2(360f, 54f));
        var saveMode = ButtonRoot("SaveModeButton", frame.rectTransform, Coral);
        SetRect(saveMode.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(570f, -25f), new Vector2(140f, 48f));
        Text("SAVE", saveMode.GetComponent<RectTransform>(), 24f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        var loadMode = ButtonRoot("LoadModeButton", frame.rectTransform, Lavender);
        SetRect(loadMode.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(720f, -25f), new Vector2(140f, 48f));
        Text("LOAD", loadMode.GetComponent<RectTransform>(), 24f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        var mode = TextBox("SAVE", frame.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Right, Ink, new Vector2(1010f, -38f), new Vector2(250f, 36f));
        var views = new MenuSaveLoadController.SlotView[8];
        for (var i = 0; i < views.Length; i++)
        {
            var col = i % 2;
            var row = i / 2;
            var slot = ButtonRoot("SaveSlot" + (i + 1), frame.rectTransform, new Color(1f, 0.98f, 0.91f, 1f));
            SetRect(slot.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f + col * 635f, -105f - row * 145f), new Vector2(605f, 124f));
            PixelBorder(slot.GetComponent<RectTransform>(), "Frame", new Color(0.38f, 0.30f, 0.48f, 1f), 3f);
            var detail = Text("SLOT " + (i + 1).ToString("00") + "\nEMPTY", slot.GetComponent<RectTransform>(), 22f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(22f, 10f), new Vector2(-72f, -10f));
            var delete = ButtonRoot("DeleteButton", slot.GetComponent<RectTransform>(), new Color(0.78f, 0.34f, 0.38f, 1f));
            SetRect(delete.GetComponent<RectTransform>(), new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(-14f, 0f), new Vector2(50f, 50f));
            Text("X", delete.GetComponent<RectTransform>(), 25f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            views[i] = new MenuSaveLoadController.SlotView(slot, delete, detail);
        }
        var back = ButtonRoot("BackButton", frame.rectTransform, Mint);
        SetRect(back.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 28f), new Vector2(160f, 54f));
        Text("BACK", back.GetComponent<RectTransform>(), 23f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        var confirmPanel = PixelPanel(frame.rectTransform, "Confirmation", new Vector2(390f, -290f), new Vector2(520f, 190f), new Color(0.20f, 0.15f, 0.29f, .98f));
        var confirmText = TextBox("OVERWRITE THIS SAVE?", confirmPanel.rectTransform, 24f, FontStyles.Bold, TextAlignmentOptions.Center, Cream, new Vector2(20f, -28f), new Vector2(480f, 50f));
        var yes = ButtonRoot("ConfirmButton", confirmPanel.rectTransform, Mint);
        SetRect(yes.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(64f, 25f), new Vector2(170f, 50f));
        Text("YES", yes.GetComponent<RectTransform>(), 22f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        var no = ButtonRoot("CancelButton", confirmPanel.rectTransform, Coral);
        SetRect(no.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-64f, 25f), new Vector2(170f, 50f));
        Text("NO", no.GetComponent<RectTransform>(), 22f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        page.gameObject.AddComponent<MenuSaveLoadController>().Configure(views, saveMode, loadMode, back, yes, no, confirmPanel.gameObject, confirmText, mode);
        return page;
    }

    private static RectTransform BuildSettingsPage(RectTransform parent)
    {
        var page = RectRoot("PageSettings", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        var frame = PixelPanel(page, "SettingsFrame", new Vector2(20f, -20f), new Vector2(1320f, 770f), new Color(0.96f, 0.92f, 0.84f, 1f));
        TextBox("CONFIG", frame.rectTransform, 38f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(30f, -28f), new Vector2(360f, 54f));
        var labels = new[] { "BGM", "SE", "VOICE", "TEXT SPEED", "AUTO SPEED" };
        var sliders = new Slider[labels.Length];
        for (var i = 0; i < labels.Length; i++)
        {
            TextBox(labels[i], frame.rectTransform, 25f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(60f, -125f - i * 92f), new Vector2(250f, 38f));
            sliders[i] = SliderRoot("Config" + labels[i].Replace(" ", string.Empty) + "Slider", frame.rectTransform, new Vector2(330f, -130f - i * 92f), new Vector2(680f, 30f));
            sliders[i].interactable = true;
            sliders[i].value = i < 3 ? 1f : .5f;
        }
        var toggleRoot = ImageRoot("FullscreenToggle", frame.rectTransform, new Color(0.30f, 0.25f, 0.42f, 1f));
        SetRect(toggleRoot.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(330f, -600f), new Vector2(52f, 52f));
        var toggle = toggleRoot.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = toggleRoot;
        toggle.graphic = ImageRoot("Check", toggleRoot.rectTransform, Mint);
        Stretch(toggle.graphic.rectTransform, new Vector2(8f, 8f), new Vector2(-8f, -8f));
        TextBox("FULLSCREEN", frame.rectTransform, 25f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(60f, -607f), new Vector2(250f, 38f));
        var reset = ButtonRoot("ResetButton", frame.rectTransform, Coral);
        SetRect(reset.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 28f), new Vector2(240f, 54f));
        Text("RESET", reset.GetComponent<RectTransform>(), 23f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        var back = ButtonRoot("BackButton", frame.rectTransform, Mint);
        SetRect(back.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(1090f, 28f), new Vector2(160f, 54f));
        Text("BACK", back.GetComponent<RectTransform>(), 23f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        page.gameObject.AddComponent<MenuSettingsController>().Configure(sliders[0], sliders[1], sliders[2], sliders[3], sliders[4], toggle, reset, back);
        return page;
    }

    private static RectTransform BuildMapPage(RectTransform parent)
    {
        var page = RectRoot("PageMap", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        AddHeader(page, "MAP", "day / night city guide");

        var shortcuts = new Button[6];
        var shortcutLabels = new[] { "H", "D", "I", "C", "Q", "M" };
        var shortcutColors = new[] { Cream, Coral, Mint, Lavender, Yellow, Cyan };
        for (var i = 0; i < shortcuts.Length; i++)
        {
            shortcuts[i] = QuestTabButton(page, "MapShortcut" + i, shortcutLabels[i], new Vector2(30f + i * 48f, -72f), new Vector2(40f, 38f), shortcutColors[i]);
        }

        var mapFrame = PixelPanel(page, "IsometricMapFrame", new Vector2(32f, -120f), new Vector2(756f, 568f), new Color(0.80f, 0.90f, 0.92f, 1f));
        var viewport = ImageRoot("MapViewport", mapFrame.rectTransform, new Color(1f, 1f, 1f, 0.02f));
        SetRect(viewport.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-8f, -8f));
        viewport.gameObject.AddComponent<RectMask2D>();
        var mapContent = RectRoot("MapContent", viewport.rectTransform);
        SetRect(mapContent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(1260f, 840f));
        var mapScroll = viewport.gameObject.AddComponent<ScrollRect>();
        mapScroll.viewport = viewport.rectTransform;
        mapScroll.content = mapContent;
        mapScroll.horizontal = true;
        mapScroll.vertical = true;
        mapScroll.movementType = ScrollRect.MovementType.Clamped;
        mapScroll.inertia = true;
        mapScroll.scrollSensitivity = 24f;
        var dayMap = RawImageRoot("DayMapArtwork", mapContent, MapWideArtworkPath, new Rect(0f, 0f, 1f, 1f));
        Stretch(dayMap.rectTransform, Vector2.zero, Vector2.zero);
        var nightMap = RawImageRoot("NightMapArtwork", mapContent, MapWideArtworkPath, new Rect(0f, 0f, 1f, 1f));
        Stretch(nightMap.rectTransform, Vector2.zero, Vector2.zero);
        nightMap.color = new Color(0.42f, 0.48f, 0.76f, 1f);
        nightMap.gameObject.SetActive(false);

        var timeBadge = PixelPanel(mapFrame.rectTransform, "TimeBadge", new Vector2(14f, -14f), new Vector2(116f, 36f), new Color(0.16f, 0.15f, 0.34f, 0.92f));
        var timeText = Text("DAY", timeBadge.rectTransform, 15f, FontStyles.Bold, TextAlignmentOptions.Center, Cream);
        timeText.name = "DayNightLabel";

        TextBox("SAFETY", mapFrame.rectTransform, 14f, FontStyles.Bold, TextAlignmentOptions.Right, Cream,
            new Vector2(484f, -16f), new Vector2(86f, 28f));
        var safety = SliderRoot("MapSafetySlider", mapFrame.rectTransform, new Vector2(574f, -18f), new Vector2(164f, 24f));

        var zoneNames = new[] { "STATION", "LIBRARY", "OFFICE", "ENTERTAINMENT", "PARK", "HOTEL" };
        var locationButtons = new Button[zoneNames.Length];
        var locationImages = new Image[zoneNames.Length];
        var locationPolygons = new MapLocationPolygon[zoneNames.Length];
        var zonePolygons = new[]
        {
            new[] { new Vector2(.08f,.72f), new Vector2(.23f,.86f), new Vector2(.39f,.80f), new Vector2(.43f,.62f), new Vector2(.30f,.51f), new Vector2(.12f,.57f) },
            new[] { new Vector2(.46f,.78f), new Vector2(.60f,.89f), new Vector2(.72f,.78f), new Vector2(.68f,.60f), new Vector2(.52f,.58f), new Vector2(.43f,.67f) },
            new[] { new Vector2(.75f,.81f), new Vector2(.91f,.86f), new Vector2(.98f,.69f), new Vector2(.89f,.55f), new Vector2(.73f,.61f), new Vector2(.69f,.73f) },
            new[] { new Vector2(.06f,.46f), new Vector2(.22f,.52f), new Vector2(.39f,.41f), new Vector2(.35f,.22f), new Vector2(.16f,.16f), new Vector2(.05f,.29f) },
            new[] { new Vector2(.43f,.49f), new Vector2(.60f,.56f), new Vector2(.70f,.42f), new Vector2(.62f,.25f), new Vector2(.47f,.22f), new Vector2(.38f,.34f) },
            new[] { new Vector2(.76f,.46f), new Vector2(.94f,.50f), new Vector2(.99f,.34f), new Vector2(.90f,.14f), new Vector2(.74f,.20f), new Vector2(.69f,.34f) }
        };
        for (var i = 0; i < zoneNames.Length; i++)
        {
            var zoneGo = new GameObject("MapLocation" + i + "_" + zoneNames[i], typeof(RectTransform), typeof(CanvasRenderer), typeof(MapLocationPolygon));
            zoneGo.transform.SetParent(mapContent, false);
            var zone = zoneGo.GetComponent<MapLocationPolygon>();
            Stretch(zone.rectTransform, Vector2.zero, Vector2.zero);
            zone.Initialize(zonePolygons[i]);
            locationButtons[i] = EnsureButton(zone);
            locationPolygons[i] = zone;
        }

        var detail = PixelPanel(page, "MapLocationDetail", new Vector2(808f, -120f), new Vector2(290f, 568f), new Color(0.98f, 0.94f, 0.88f, 1f));
        var detailTitle = TextBox("駅前", detail.rectTransform, 25f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Vector2(16f, -16f), new Vector2(258f, 38f));
        var rere = PixelPanel(detail.rectTransform, "ReReHint", new Vector2(16f, -72f), new Vector2(258f, 92f), new Color(0.90f, 0.84f, 0.98f, 1f));
        TextBox("ReRe HINT", rere.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(12f, -10f), new Vector2(234f, 20f));
        var hintText = TextBox("乗り換え前なら、少しだけ話を聞けそう。", rere.rectTransform, 14f, FontStyles.Normal, TextAlignmentOptions.TopLeft, Ink,
            new Vector2(12f, -34f), new Vector2(234f, 48f));
        var mapReRe = ImageRoot("MapReReGuide", rere.rectTransform, Color.white);
        mapReRe.sprite = LoadSprite(CommonUiCropFolder + "/rere_worried.png");
        mapReRe.preserveAspect = true;
        mapReRe.raycastTarget = false;
        SetRect(mapReRe.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-8f, 2f), new Vector2(62f, 62f));
        var descriptionText = TextBox("待ち合わせと移動の起点。夕方は帰宅客で混み合う。", detail.rectTransform, 14f, FontStyles.Normal, TextAlignmentOptions.TopLeft, Ink,
            new Vector2(18f, -180f), new Vector2(254f, 42f));

        TextBox("RELATED ITEM", detail.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(18f, -236f), new Vector2(254f, 20f));
        var itemCard = PixelPanel(detail.rectTransform, "RelatedItemCard", new Vector2(18f, -264f), new Vector2(254f, 54f), new Color(1f, 0.91f, 0.69f, 1f));
        var itemText = Text("交通ICカード", itemCard.rectTransform, 16f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);

        TextBox("HERE NOW", detail.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(18f, -332f), new Vector2(254f, 20f));
        var characterButtons = new Button[3];
        for (var i = 0; i < characterButtons.Length; i++)
        {
            characterButtons[i] = LocalPixelButton(detail.rectTransform, "MapCharacter" + i, i == 0 ? "R" : i == 1 ? "Y" : "O",
                new Vector2(20f + i * 82f, -364f), new Vector2(66f, 58f), i == 0 ? Lavender : i == 1 ? Coral : Yellow);
        }
        var characterText = TextBox("ReRe / 対象A", detail.rectTransform, 12f, FontStyles.Normal, TextAlignmentOptions.Center, Ink,
            new Vector2(18f, -430f), new Vector2(254f, 20f));
        var goButton = LocalPixelButton(detail.rectTransform, "MapGoButton", "GO?  YES", new Vector2(18f, -470f), new Vector2(254f, 72f), Mint);

        var controller = page.gameObject.AddComponent<MapMenuController>();
        var so = new SerializedObject(controller);
        ConfigureMapLocations(so);
        SetQuestObjectArray(so, "locationButtons", locationButtons);
        SetQuestObjectArray(so, "locationImages", locationImages);
        SetQuestObjectArray(so, "locationPolygons", locationPolygons);
        SetQuestObjectArray(so, "goButtons", new Object[] { goButton });
        SetObject(so, "detailNameText", detailTitle);
        SetObject(so, "detailDescriptionText", descriptionText);
        SetObject(so, "rereHintText", hintText);
        SetObject(so, "relatedItemText", itemText);
        SetObject(so, "relatedCharacterText", characterText);
        SetObject(so, "safetySlider", safety);
        SetObject(so, "safetyFillImage", safety.fillRect.GetComponent<Image>());
        SetObject(so, "dayNightLabelText", timeText);
        SetObject(so, "dayMapRoot", dayMap.gameObject);
        SetObject(so, "nightMapRoot", nightMap.gameObject);
        var hoverAudio = page.gameObject.AddComponent<AudioSource>();
        hoverAudio.playOnAwake = false;
        hoverAudio.spatialBlend = 0f;
        SetObject(so, "hoverAudioSource", hoverAudio);
        SetObject(so, "hoverAudioClip", AssetDatabase.LoadAssetAtPath<AudioClip>(MapHoverAudioPath));
        SetQuestObjectArray(so, "relatedCharacterButtons", characterButtons);
        SetIntArray(so, "relatedCharacterLocationIndexes", new[] { 0, 0, 0 });
        SetIntArray(so, "goButtonLocationIndexes", new[] { 3 });
        SetQuestObjectArray(so, "pageNavigationButtons", shortcuts);
        so.FindProperty("fallbackHour").intValue = 12;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);

        var routeLauncher = page.gameObject.AddComponent<MapRouteLauncher>();
        var routeSo = new SerializedObject(routeLauncher);
        SetObject(routeSo, "mapController", controller);
        var routes = routeSo.FindProperty("routes");
        if (routes != null)
        {
            routes.arraySize = 1;
            var route = routes.GetArrayElementAtIndex(0);
            SetChildBool(route, "enabled", true);
            SetChildInt(route, "locationIndex", 3);
            SetChildString(route, "routeId", "papa_cafe");
            SetChildString(route, "sceneName", string.Empty);
            SetChildString(route, "mapId", "papa_cafe");
            SetChildString(route, "entryScriptPath", "Scenario/PapaQuestDemo");
            SetChildString(route, "entryLabel", string.Empty);
        }
        routeSo.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(routeLauncher);
        return page;
    }

    private static RectTransform BuildNavigation(RectTransform parent, out Button top, out Button status, out Button items,
        out Button characters, out Button quest, out Button map, out Button save, out Button settings)
    {
        var nav = ImageRoot("PersistentNav", parent, new Color(0.98f, 0.96f, 0.91f, 1f));
        SetRect(nav.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(-196f, 72f));
        PixelBorder(nav.rectTransform, "NavFrame", new Color(0.46f, 0.42f, 0.58f, 1f), 3f);
        var x = 12f;
        top = WideNavButton(nav.rectTransform, "TopButton", "HOME", x, Cream); x += 178f;
        status = WideNavButton(nav.rectTransform, "StatusButton", "DRESS", x, Lavender); x += 178f;
        items = WideNavButton(nav.rectTransform, "ItemsButton", "ITEM", x, Yellow); x += 178f;
        characters = WideNavButton(nav.rectTransform, "CharactersButton", "CHAR", x, Coral); x += 178f;
        quest = WideNavButton(nav.rectTransform, "QuestButton", "QUEST", x, Peach); x += 178f;
        map = WideNavButton(nav.rectTransform, "MapButton", "MAP", x, Cyan); x += 178f;
        save = WideNavButton(nav.rectTransform, "SaveButton", "SAVE", x, new Color(0.66f, 0.72f, 0.94f, 1f)); x += 178f;
        settings = WideNavButton(nav.rectTransform, "SettingsButton", "SET", x, new Color(0.78f, 0.78f, 0.82f, 1f));
        AddDynamicNotificationBadge(items, "ItemNotificationBadge");
        AddDynamicNotificationBadge(quest, "QuestNotificationBadge");
        ConfigurePageNavigation(nav.gameObject, top, status, items, characters, quest, map, save, settings);
        return nav.rectTransform;
    }

    private static Button WideNavButton(RectTransform parent, string name, string label, float x, Color color)
    {
        var button = ButtonRoot(name, parent, color);
        SetRect(button.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -8f), new Vector2(166f, 56f));
        PixelBorder(button.GetComponent<RectTransform>(), "Frame", Ink, 2f);
        Text(label, button.GetComponent<RectTransform>(), 14f, FontStyles.Bold, TextAlignmentOptions.Center, Ink, new Vector2(44f, 0f), new Vector2(-4f, 0f));
        AddNavigationIcon(button, name.Replace("Button", string.Empty));
        return button;
    }

    private static void BuildReRe(RectTransform root, RectTransform phone)
    {
        var anchor = new GameObject("WanderingReReV2", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MenuReReSpriteAnimator));
        anchor.transform.SetParent(root, false);
        var rt = anchor.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(555f, -384f);
        rt.sizeDelta = new Vector2(150f, 430f);
        var image = anchor.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.enabled = false;

        var bubble = Panel(root, "ReReSpeechBubble", new Vector2(1260f, -322f), new Vector2(360f, 150f), new Color(1f, 0.96f, 0.88f, 1f));
        var message = Text("気になるアイコンを選んでみて。今必要そうなことをReReが一緒に整理するよ。", bubble.rectTransform, 18f,
            FontStyles.Bold, TextAlignmentOptions.TopLeft, Ink, new Vector2(22f, 18f), new Vector2(-22f, -64f));
        message.name = "Message";

        var confirm = RectRoot("ConfirmButtons", bubble.rectTransform);
        SetRect(confirm, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-22f, 20f), new Vector2(164f, 40f));
        var yes = ButtonRoot("YesButton", confirm, Mint);
        SetRect(yes.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(72f, 34f));
        Text("YES", yes.GetComponent<RectTransform>(), 14f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        var no = ButtonRoot("NoButton", confirm, Coral);
        SetRect(no.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(72f, 34f));
        Text("NO", no.GetComponent<RectTransform>(), 14f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        confirm.gameObject.SetActive(false);
        bubble.gameObject.SetActive(false);
    }

    private static Button NavButton(RectTransform parent, string name, string label, float x, float y, Color color)
    {
        var button = ButtonRoot(name, parent, color);
        SetRect(button.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(86f, 62f));
        Text(label, button.GetComponent<RectTransform>(), 14f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        return button;
    }

    private static Image Tile(RectTransform parent, string name, string label, Color color, Vector2 pos, Vector2 size)
    {
        var tile = Panel(parent, name, pos, size, color);
        EnsureButton(tile);
        PixelBorder(tile.rectTransform, "TileFrame", new Color(0.18f, 0.11f, 0.22f, 1f), 5f);
        var accent = ImageRoot("TileAccent", tile.rectTransform, new Color(1f, 1f, 1f, 0.25f));
        accent.raycastTarget = false;
        SetRect(accent.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(0f, 10f));
        Text(label, tile.rectTransform, 24f, FontStyles.Bold, TextAlignmentOptions.BottomLeft, Ink, new Vector2(24f, 20f), new Vector2(-24f, -size.y + 76f));
        return tile;
    }

    private static void AddStatusBar(RectTransform parent)
    {
        var bar = ImageRoot("SmartphoneStatusBar", parent, new Color(0.21f, 0.13f, 0.25f, 0.94f));
        SetRect(bar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(0f, 54f));
        PixelBorder(bar.rectTransform, "StatusBarFrame", new Color(0.08f, 0.05f, 0.10f, 1f), 3f);

        Text("13:24", bar.rectTransform, 18f, FontStyles.Bold, TextAlignmentOptions.Left, Cream, new Vector2(26f, 0f), new Vector2(-500f, 0f));
        Text("ReRe resident AI", bar.rectTransform, 15f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.86f, 0.76f, 0.94f, 1f));

        for (int i = 0; i < 4; i++)
        {
            var dot = ImageRoot("SignalDot" + i, bar.rectTransform, i < 3 ? Mint : new Color(1f, 1f, 1f, 0.3f));
            dot.raycastTarget = false;
            SetRect(dot.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-94f + i * 14f, 0f), new Vector2(8f, 8f));
        }
    }

    private static void AddPixelPattern(RectTransform parent)
    {
        for (int i = 0; i < 18; i++)
        {
            var color = i % 3 == 0 ? Coral : i % 3 == 1 ? Mint : Lavender;
            var pixel = ImageRoot("WallpaperPixel" + i, parent, new Color(color.r, color.g, color.b, 0.12f));
            pixel.raycastTarget = false;
            float x = 46f + (i * 137f) % 980f;
            float y = -82f - (i * 83f) % 560f;
            SetRect(pixel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(18f + (i % 2) * 12f, 18f + (i % 4) * 6f));
        }
    }

    private static void AddAppGlyph(RectTransform parent, string glyph, string caption)
    {
        var icon = ImageRoot("AppGlyph", parent, new Color(1f, 1f, 1f, 0.42f));
        icon.raycastTarget = false;
        SetRect(icon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -26f), new Vector2(92f, 92f));
        PixelBorder(icon.rectTransform, "GlyphFrame", new Color(0.20f, 0.13f, 0.24f, 0.75f), 3f);
        Text(glyph, icon.rectTransform, 26f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        Text(caption, parent, 13f, FontStyles.Bold, TextAlignmentOptions.TopLeft, new Color(0.25f, 0.18f, 0.30f, 0.82f),
            new Vector2(128f, 22f), new Vector2(-18f, -30f));
    }

    private static void AddNotificationBadge(RectTransform parent, string value)
    {
        var badge = ImageRoot("NotificationBadge", parent, Coral);
        badge.raycastTarget = false;
        SetRect(badge.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -18f), new Vector2(42f, 34f));
        PixelBorder(badge.rectTransform, "BadgeFrame", new Color(0.16f, 0.09f, 0.18f, 1f), 3f);
        Text(value, badge.rectTransform, 17f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
    }

    private static void PixelBorder(RectTransform parent, string name, Color color, float thickness)
    {
        var frame = RectRoot(name, parent);
        Stretch(frame, Vector2.zero, Vector2.zero);

        BorderPart(frame, "Top", color, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, thickness));
        BorderPart(frame, "Bottom", color, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, thickness));
        BorderPart(frame, "Left", color, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(thickness, 0f));
        BorderPart(frame, "Right", color, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(thickness, 0f));
    }

    private static void AddPixelSpeechTail(RectTransform bubble)
    {
        var dark = new Color(0.18f, 0.11f, 0.22f, 1f);
        var fill = new Color(1f, 0.96f, 0.88f, 1f);

        var borderA = ImageRoot("TailBorderA", bubble, dark);
        borderA.raycastTarget = false;
        SetRect(borderA.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-36f, 3f), new Vector2(34f, 18f));

        var fillA = ImageRoot("TailFillA", bubble, fill);
        fillA.raycastTarget = false;
        SetRect(fillA.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-39f, 6f), new Vector2(25f, 10f));

        var borderB = ImageRoot("TailBorderB", bubble, dark);
        borderB.raycastTarget = false;
        SetRect(borderB.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-18f, -11f), new Vector2(22f, 18f));

        var fillB = ImageRoot("TailFillB", bubble, fill);
        fillB.raycastTarget = false;
        SetRect(fillB.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-21f, -8f), new Vector2(13f, 9f));
    }

    private static void BorderPart(RectTransform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var image = ImageRoot(name, parent, color);
        image.raycastTarget = false;
        SetRect(image.rectTransform, anchorMin, anchorMax, pivot, pos, size);
    }

    private static Button EnsureButton(Graphic target)
    {
        var button = target.GetComponent<Button>();
        if (!button)
            button = target.gameObject.AddComponent<Button>();

        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.88f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        button.colors = colors;
        button.targetGraphic = target;
        return button;
    }

    private static Image Panel(RectTransform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var image = ImageRoot(name, parent, color);
        SetRect(image.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, size);
        return image;
    }

    private static Image PixelPanel(RectTransform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var image = Panel(parent, name, pos, size, color);
        PixelBorder(image.rectTransform, name + "Frame", new Color(0.16f, 0.10f, 0.20f, 1f), 4f);
        var topLight = ImageRoot("TopLight", image.rectTransform, new Color(1f, 1f, 1f, 0.24f));
        topLight.raycastTarget = false;
        SetRect(topLight.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(0f, 8f));
        return image;
    }

    private static void AddRadarLegend(RectTransform parent)
    {
        LegendDot(parent, "BASE", new Vector2(28f, -286f), new Color(1f, 0.35f, 0.56f, 1f));
        LegendDot(parent, "EQUIP", new Vector2(152f, -286f), Yellow);
        var note = ImageRoot("RadarNoteLine", parent, new Color(0.22f, 0.15f, 0.26f, 0.25f));
        note.raycastTarget = false;
        SetRect(note.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -58f), new Vector2(284f, 8f));
    }

    private static void LegendDot(RectTransform parent, string label, Vector2 pos, Color color)
    {
        var dot = ImageRoot(label + "Dot", parent, color);
        dot.raycastTarget = false;
        SetRect(dot.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, new Vector2(16f, 16f));
        PixelBorder(dot.rectTransform, "DotFrame", new Color(0.16f, 0.10f, 0.20f, 1f), 2f);
        TextBox(label, parent, 13f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, pos + new Vector2(24f, -1f), new Vector2(78f, 18f));
    }

    private static void AddBoothCurtain(RectTransform parent)
    {
        var rail = ImageRoot("CurtainRail", parent, new Color(0.18f, 0.11f, 0.22f, 1f));
        rail.raycastTarget = false;
        SetRect(rail.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -64f), new Vector2(-54f, 8f));

        for (int i = 0; i < 5; i++)
        {
            var strip = ImageRoot("CurtainStrip" + i, parent, i % 2 == 0 ? new Color(0.76f, 0.63f, 0.95f, 0.42f) : new Color(1f, 0.76f, 0.84f, 0.42f));
            strip.raycastTarget = false;
            SetRect(strip.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-112f + i * 56f, -72f), new Vector2(40f, 240f));
        }

        var changingLeft = ImageRoot("ChangingCurtainLeft", parent, new Color(0.54f, 0.35f, 0.82f, 0.92f));
        changingLeft.raycastTarget = false;
        SetRect(changingLeft.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-94f, -72f), new Vector2(116f, 246f));
        PixelBorder(changingLeft.rectTransform, "ChangingLeftFrame", new Color(0.18f, 0.11f, 0.22f, 0.9f), 3f);

        var changingRight = ImageRoot("ChangingCurtainRight", parent, new Color(0.54f, 0.35f, 0.82f, 0.92f));
        changingRight.raycastTarget = false;
        SetRect(changingRight.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(94f, -72f), new Vector2(116f, 246f));
        PixelBorder(changingRight.rectTransform, "ChangingRightFrame", new Color(0.18f, 0.11f, 0.22f, 0.9f), 3f);

        var floor = ImageRoot("BoothFloor", parent, new Color(0.20f, 0.13f, 0.24f, 0.18f));
        floor.raycastTarget = false;
        SetRect(floor.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(230f, 26f));
    }

    private static void AddStandingReRePlaceholder(RectTransform parent)
    {
        var shadow = ImageRoot("SpriteShadow", parent, new Color(0.16f, 0.10f, 0.20f, 0.24f));
        shadow.raycastTarget = false;
        SetRect(shadow.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 122f), new Vector2(150f, 18f));

        var body = ImageRoot("StandingSpritePlaceholder", parent, new Color(0.42f, 0.32f, 0.52f, 0.34f));
        body.raycastTarget = false;
        SetRect(body.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 130f), new Vector2(108f, 214f));
        PixelBorder(body.rectTransform, "SpriteFrame", new Color(0.18f, 0.11f, 0.22f, 0.55f), 4f);

        var head = ImageRoot("SpriteHead", parent, new Color(0.73f, 0.58f, 0.96f, 0.72f));
        head.raycastTarget = false;
        SetRect(head.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 310f), new Vector2(132f, 88f));
        PixelBorder(head.rectTransform, "HeadFrame", new Color(0.18f, 0.11f, 0.22f, 0.55f), 4f);

        var dress = ImageRoot("SpriteDress", parent, new Color(1f, 0.76f, 0.86f, 0.72f));
        dress.raycastTarget = false;
        SetRect(dress.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 176f), new Vector2(148f, 86f));
        PixelBorder(dress.rectTransform, "DressFrame", new Color(0.18f, 0.11f, 0.22f, 0.55f), 4f);
    }

    private static void OutfitBonusRow(RectTransform parent, int index, string label, int value, Color color)
    {
        var y = -66f - index * 46f;
        TextBox(label, parent, 15f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(22f, y), new Vector2(86f, 22f));

        var bg = ImageRoot("BonusMeterBg" + index, parent, new Color(1f, 1f, 1f, 0.32f));
        bg.raycastTarget = false;
        SetRect(bg.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(114f, y - 1f), new Vector2(164f, 18f));
        PixelBorder(bg.rectTransform, "MeterFrame", new Color(0.16f, 0.10f, 0.20f, 0.9f), 2f);

        var fill = ImageRoot("Fill", bg.rectTransform, color);
        fill.raycastTarget = false;
        SetRect(fill.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(3f, 0f), new Vector2(24f * Mathf.Clamp(value, 1, 6), 10f));

        var valueText = TextBox("+" + value, parent, 15f, FontStyles.Bold, TextAlignmentOptions.Right, Ink, new Vector2(246f, y), new Vector2(42f, 22f));
        valueText.name = "BonusValueText" + index;
    }

    private static void OutfitCard(RectTransform parent, int index, string label, Color color, bool selected, bool locked)
    {
        var card = Panel(parent, "OutfitCard" + index, new Vector2(96f + index * 154f, -42f), new Vector2(132f, 70f), locked ? new Color(0.70f, 0.70f, 0.74f, 1f) : color);
        EnsureButton(card);
        PixelBorder(card.rectTransform, "OutfitCardFrame", selected ? Yellow : new Color(0.16f, 0.10f, 0.20f, 1f), selected ? 5f : 3f);

        var selectedFrame = RectRoot("SelectedFrame", card.rectTransform);
        Stretch(selectedFrame, Vector2.zero, Vector2.zero);
        selectedFrame.gameObject.SetActive(selected);
        BorderPart(selectedFrame, "SelectedTop", Yellow, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 6f));
        BorderPart(selectedFrame, "SelectedBottom", Yellow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 6f));
        BorderPart(selectedFrame, "SelectedLeft", Yellow, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(6f, 0f));
        BorderPart(selectedFrame, "SelectedRight", Yellow, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(6f, 0f));

        var swatch = ImageRoot("OutfitSwatch", card.rectTransform, locked ? new Color(0.40f, 0.40f, 0.44f, 1f) : new Color(1f, 1f, 1f, 0.44f));
        swatch.raycastTarget = false;
        SetRect(swatch.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(38f, 38f));
        PixelBorder(swatch.rectTransform, "SwatchFrame", new Color(0.16f, 0.10f, 0.20f, 0.8f), 2f);

        TextBox(locked ? "???" : label, card.rectTransform, 15f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Vector2(46f, -18f), new Vector2(76f, 36f));
    }

    private static void AddHeader(RectTransform parent, string title, string subtitle)
    {
        Text(title, parent, 34f, FontStyles.Bold, TextAlignmentOptions.TopLeft, Ink, new Vector2(32f, 22f), new Vector2(-32f, -24f));
        Text(subtitle, parent, 18f, FontStyles.Normal, TextAlignmentOptions.TopRight, new Color(0.45f, 0.36f, 0.48f, 1f), new Vector2(32f, 30f), new Vector2(-32f, -28f));
    }

    private static void AddIcon(RectTransform parent, string label)
    {
        var icon = ImageRoot("Icon", parent, new Color(1f, 1f, 1f, 0.42f));
        icon.raycastTarget = false;
        SetRect(icon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(94f, 94f));
        Text(label, icon.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
    }

    private static void SmallCard(RectTransform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var card = Panel(parent, name, pos, size, color);
        EnsureButton(card);
        var icon = ImageRoot("Icon", card.rectTransform, new Color(1f, 1f, 1f, 0.38f));
        icon.raycastTarget = false;
        SetRect(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size.y * 0.46f, size.y * 0.46f));
    }

    private static void ContactCard(RectTransform parent, int index)
    {
        var card = Panel(parent, "ContactCard" + index, new Vector2(20f + (index % 2) * 290f, -20f - (index / 2) * 112f), new Vector2(270f, 92f), index % 2 == 0 ? Cream : new Color(0.95f, 0.90f, 1f, 1f));
        EnsureButton(card);
        Circle(card.rectTransform, "Portrait", new Vector2(18f, -16f), 56f, index % 3 == 0 ? Coral : Mint);
        AddMiniBars(card.rectTransform, new Vector2(92f, -20f), 3);
    }

    private static Button CharacterNavButton(RectTransform parent, string name, string label, Rect sourceRect, Color color, float artworkWidth, float artworkHeight)
    {
        var button = CharacterTabButton(parent, name, label, sourceRect, color, artworkWidth, artworkHeight);
        PixelBorder(button.GetComponent<RectTransform>(), "NavFrame", Ink, 3f);
        return button;
    }

    private static void BuildUnifiedPageNavigation(RectTransform page, float artworkWidth, float artworkHeight)
    {
        var strip = PixelPanelAt(page, "UnifiedPageNavigation", new Rect(112f, 76f, 1448f, 72f),
            new Color(0.98f, 0.96f, 0.91f, 1f), artworkWidth, artworkHeight);
        var labels = new[] { "HOME", "DRESS", "ITEM", "CHAR", "QUEST", "MAP", "SAVE", "SET" };
        var names = new[] { "Home", "Dress", "Item", "Characters", "Quest", "Map", "Save", "Settings" };
        var colors = new[] { Cream, Lavender, Yellow, Coral, Peach, Cyan, new Color(0.66f, 0.72f, 0.94f, 1f), new Color(0.78f, 0.78f, 0.82f, 1f) };
        var buttons = new Button[labels.Length];
        for (var i = 0; i < buttons.Length; i++)
        {
            buttons[i] = CharacterNavButton(page, "Unified" + names[i] + "Button", labels[i],
                new Rect(120f + i * 178f, 84f, 166f, 56f), colors[i], artworkWidth, artworkHeight);
            AddNavigationIcon(buttons[i], names[i]);
        }
        AddDynamicNotificationBadge(buttons[2], "ItemNotificationBadge");
        AddDynamicNotificationBadge(buttons[4], "QuestNotificationBadge");
        ConfigurePageNavigation(strip.gameObject, buttons[0], buttons[1], buttons[2], buttons[3], buttons[4], buttons[5], buttons[6], buttons[7]);
    }

    private static void AddNavigationIcon(Button button, string id)
    {
        var sprite = LoadNavigationSprite(id);
        if (!sprite) return;
        var icon = ImageRoot("NavIcon", button.GetComponent<RectTransform>(), Color.white);
        icon.sprite = sprite;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        SetRect(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(46f, 46f));
        icon.transform.SetAsFirstSibling();
    }

    private static Sprite LoadNavigationSprite(string id)
    {
        var key = id.ToLowerInvariant();
        if (key == "top") key = "home";
        if (key == "items") key = "item";
        if (key == "characters") key = "char";
        return LoadSprite(CommonUiCropFolder + "/nav_" + key + ".png");
    }

    private static void ConfigurePageNavigation(GameObject target, Button home, Button dress, Button item, Button characters,
        Button quest, Button map, Button save, Button settings)
    {
        var navigation = target.GetComponent<MenuPageNavigation>() ?? target.AddComponent<MenuPageNavigation>();
        var so = new SerializedObject(navigation);
        SetObject(so, "home", home);
        SetObject(so, "dress", dress);
        SetObject(so, "item", item);
        SetObject(so, "characters", characters);
        SetObject(so, "quest", quest);
        SetObject(so, "map", map);
        SetObject(so, "save", save);
        SetObject(so, "settings", settings);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(navigation);
    }

    private static void ConfigureTopHudState(GameObject target, TextMeshProUGUI dayText, TextMeshProUGUI debtText,
        TextMeshProUGUI moneyText, GameObject urgencyMark, MenuNotificationBadge dressBadge,
        MenuNotificationBadge charactersBadge, MenuNotificationBadge homeBadge, MenuNotificationBadge itemsBadge,
        MenuNotificationBadge saveBadge, MenuNotificationBadge mapBadge, MenuNotificationBadge questBadge,
        MenuNotificationBadge settingsBadge)
    {
        var state = target.GetComponent<MenuTopHudState>() ?? target.AddComponent<MenuTopHudState>();
        var so = new SerializedObject(state);
        SetObject(so, "dayText", dayText);
        SetObject(so, "debtDaysText", debtText);
        SetObject(so, "moneyText", moneyText);
        SetObject(so, "debtUrgencyMark", urgencyMark);

        var bindings = so.FindProperty("badgeBindings");
        if (bindings != null)
        {
            var values = new[]
            {
                dressBadge, charactersBadge, homeBadge, itemsBadge,
                saveBadge, mapBadge, questBadge, settingsBadge
            };
            bindings.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                var binding = bindings.GetArrayElementAtIndex(i);
                var action = binding.FindPropertyRelative("action");
                var badge = binding.FindPropertyRelative("badge");
                if (action != null) action.enumValueIndex = i;
                if (badge != null) badge.objectReferenceValue = values[i];
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(state);
    }

    private static MenuNotificationBadge AddDynamicNotificationBadge(Button button, string name, int initialCount = 0)
    {
        button.transform.SetAsLastSibling();
        var badge = ImageRoot(name, button.GetComponent<RectTransform>(), Coral);
        SetRect(badge.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-8f, -8f), new Vector2(34f, 34f));
        PixelBorder(badge.rectTransform, "BadgeFrame", Ink, 2f);
        var count = Text("!", badge.rectTransform, 17f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        count.name = "Count";
        var controller = button.gameObject.AddComponent<MenuNotificationBadge>();
        var so = new SerializedObject(controller);
        SetObject(so, "badgeTarget", badge.gameObject);
        SetObject(so, "countText", count);
        var visible = so.FindProperty("initialVisible");
        if (visible != null) visible.boolValue = initialCount > 0;
        var initialCountProperty = so.FindProperty("initialCount");
        if (initialCountProperty != null) initialCountProperty.intValue = Mathf.Max(0, initialCount);
        so.ApplyModifiedPropertiesWithoutUndo();
        badge.gameObject.SetActive(initialCount > 0);
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static Button CharacterTabButton(RectTransform parent, string name, string label, Rect sourceRect, Color color, float artworkWidth, float artworkHeight)
    {
        var button = ButtonRoot(name, parent, color);
        var rt = button.GetComponent<RectTransform>();
        SetRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            DressCenter(sourceRect, artworkWidth, artworkHeight), DressSize(sourceRect, artworkWidth, artworkHeight));
        PixelBorder(rt, "TabFrame", new Color(0.18f, 0.13f, 0.28f, 1f), 3f);
        Text(label, rt, 19f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        return button;
    }

    private static Button LocalPixelButton(RectTransform parent, string name, string label, Vector2 pos, Vector2 size, Color color)
    {
        var button = ButtonRoot(name, parent, color);
        var rt = button.GetComponent<RectTransform>();
        SetRect(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, size);
        PixelBorder(rt, "ButtonFrame", new Color(0.18f, 0.13f, 0.28f, 1f), 3f);
        Text(label, rt, 18f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        return button;
    }

    private static void ItemDraftCard(RectTransform parent, int index, Vector2 pos, Vector2 size, Color color)
    {
        var button = ButtonRoot("ItemCard" + index, parent, color);
        var rt = button.GetComponent<RectTransform>();
        SetRect(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, size);
        PixelBorder(rt, "Frame", Ink, 3f);

        var highlight = RectRoot("SelectedFrame", rt);
        Stretch(highlight, Vector2.zero, Vector2.zero);
        var tint = highlight.gameObject.AddComponent<Image>();
        tint.color = new Color(1f, 0.82f, 0.18f, 0.22f);
        tint.raycastTarget = false;
        BorderPart(highlight, "SelectedTop", Yellow, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 6f));
        BorderPart(highlight, "SelectedBottom", Yellow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 6f));
        BorderPart(highlight, "SelectedLeft", Yellow, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(6f, 0f));
        BorderPart(highlight, "SelectedRight", Yellow, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(6f, 0f));
        highlight.gameObject.SetActive(index == 0);

        var icon = ImageRoot("ItemIcon" + index, rt, index % 2 == 0 ? Coral : Cyan);
        icon.raycastTarget = false;
        SetRect(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(56f, 56f));
        PixelBorder(icon.rectTransform, "IconFrame", new Color(1f, 1f, 1f, 0.55f), 2f);

        var nameText = Text("Item", rt, 16f, FontStyles.Bold, TextAlignmentOptions.Center, Ink, new Vector2(6f, 8f), new Vector2(-6f, -86f));
        nameText.name = "ItemName" + index;
    }

    private static void BagSlot(RectTransform parent, int index, Vector2 pos, Vector2 size)
    {
        var slot = ImageRoot("BagSlot" + index, parent, new Color(1f, 0.98f, 0.88f, 1f));
        SetRect(slot.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, size);
        PixelBorder(slot.rectTransform, "Frame", Ink, 3f);
        slot.raycastTarget = true;

        var icon = ImageRoot("BagSlotIcon" + index, slot.rectTransform, new Color(1f, 1f, 1f, 0.18f));
        icon.raycastTarget = false;
        SetRect(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(44f, 44f));

        var text = Text("EMPTY", slot.rectTransform, 15f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(66f, 8f), new Vector2(-8f, -8f));
        text.name = "BagSlotText" + index;
    }

    private static void BuildBagZipOverlay(RectTransform parent, float artworkWidth, float artworkHeight)
    {
        var overlay = RectRoot("BagZipOverlay", parent);
        Stretch(overlay, Vector2.zero, Vector2.zero);
        overlay.SetAsLastSibling();

        var backdrop = ImageRoot("ZipBackdrop", overlay, new Color(0.01f, 0.01f, 0.02f, 0.96f));
        backdrop.raycastTarget = true;
        SetRect(backdrop.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            DressCenter(new Rect(104f, 34f, 1464f, 816f), artworkWidth, artworkHeight), DressSize(new Rect(104f, 34f, 1464f, 816f), artworkWidth, artworkHeight));
        PixelBorder(backdrop.rectTransform, "ZipBackdropFrame", new Color(0.10f, 0.08f, 0.14f, 1f), 5f);

        var openBag = ImageRoot("OpenBagImage", overlay, Color.white);
        openBag.sprite = LoadSprite(ItemBagOpenSpritePath);
        openBag.preserveAspect = true;
        openBag.raycastTarget = false;
        SetRect(openBag.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-10f, -12f), new Vector2(680f, 560f));

        var closedBag = ImageRoot("ClosedBagImage", overlay, Color.white);
        closedBag.sprite = LoadSprite(ItemBagClosedSpritePath);
        closedBag.preserveAspect = true;
        closedBag.raycastTarget = false;
        SetRect(closedBag.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-10f, -12f), new Vector2(680f, 560f));
        closedBag.gameObject.SetActive(false);

        var itemPositions = new[]
        {
            new Vector2(-188f, 24f),
            new Vector2(-62f, 30f),
            new Vector2(64f, 28f),
            new Vector2(188f, 22f)
        };
        for (var i = 0; i < itemPositions.Length; i++)
        {
            var item = ImageRoot("PackedOverlayItem" + i, overlay, i % 2 == 0 ? Coral : Cyan);
            item.raycastTarget = false;
            SetRect(item.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                itemPositions[i], new Vector2(70f, 70f));
            PixelBorder(item.rectTransform, "PackedItemFrame", Cream, 3f);
            item.gameObject.SetActive(false);
        }

        var hook = ImageRoot("ZipperHookImage", overlay, Color.white);
        hook.sprite = LoadSprite(ItemBagHookSpritePath);
        hook.preserveAspect = true;
        hook.raycastTarget = false;
        SetRect(hook.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(294f, 218f), new Vector2(76f, 96f));

        var rere = ImageRoot("ZipperReReImage", overlay, Color.white);
        var rereFrames = LoadSpritesFromFolder(ItemBagReReZipFolder);
        rere.sprite = rereFrames.Length > 0 ? rereFrames[0] : LoadSprite(TopReReSpritePath);
        rere.preserveAspect = true;
        rere.raycastTarget = false;
        SetRect(rere.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(294f, 132f), new Vector2(230f, 230f));

        var message = Text("Packing...", overlay, 30f, FontStyles.Bold, TextAlignmentOptions.Center, Cream, new Vector2(0f, 270f), new Vector2(0f, -680f));
        message.name = "ZipMessageText";

        overlay.gameObject.SetActive(false);
    }

    private static Image PixelPanelAt(RectTransform parent, string name, Rect sourceRect, Color color, float artworkWidth, float artworkHeight)
    {
        var panel = ImageRoot(name, parent, color);
        panel.raycastTarget = false;
        SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            DressCenter(sourceRect, artworkWidth, artworkHeight), DressSize(sourceRect, artworkWidth, artworkHeight));
        PixelBorder(panel.rectTransform, "PanelFrame", new Color(0.42f, 0.36f, 0.55f, 1f), 4f);
        return panel;
    }

    private static Image CircleAt(RectTransform parent, string name, Rect rect, Color color)
    {
        var image = ImageRoot(name, parent, color);
        image.raycastTarget = false;
        SetRect(image.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(rect.x, -rect.y), new Vector2(rect.width, rect.height));
        PixelBorder(image.rectTransform, "PortraitFrame", Ink, 3f);
        return image;
    }

    private static void CharacterContactRow(RectTransform parent, int index, string name, string attribute, bool quest, Color color, Rect sourceRect, float artworkWidth, float artworkHeight)
    {
        var card = Panel(parent, "CharacterContact" + index, new Vector2(sourceRect.x, -sourceRect.y), new Vector2(sourceRect.width, sourceRect.height), color);
        PixelBorder(card.rectTransform, "PanelFrame", new Color(0.42f, 0.36f, 0.55f, 1f), 4f);
        EnsureButton(card);
        var rt = card.rectTransform;
        CircleAt(rt, "Icon", new Rect(24f, 16f, 62f, 62f), index == 3 ? new Color(0.45f, 0.45f, 0.50f, 1f) : Lavender);
        TextBox(name, rt, 24f, FontStyles.Bold, TextAlignmentOptions.Left, Ink, new Vector2(106f, -16f), new Vector2(312f, 32f));
        AttributeChip(rt, attribute, new Vector2(108f, -54f), AttributeColor(attribute));
        if (quest)
            QuestBadge(rt, new Vector2(454f, -18f));
    }

    private static RectTransform CharacterScrollList(RectTransform parent, string name, Rect sourceRect, float artworkWidth, float artworkHeight, out RectTransform content)
    {
        var viewport = PixelPanelAt(parent, name, sourceRect, new Color(0.92f, 0.98f, 0.96f, 1f), artworkWidth, artworkHeight);
        viewport.name = name;
        viewport.raycastTarget = true;
        var mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        content = RectRoot("Content", viewport.rectTransform);
        SetRect(content, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(0f, 744f));

        var scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.viewport = viewport.rectTransform;
        scrollRect.content = content;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
        return viewport.rectTransform;
    }

    private static void AttributeChip(RectTransform parent, string label, Vector2 pos, Color color)
    {
        var chip = ImageRoot("Chip" + label, parent, color);
        chip.raycastTarget = false;
        SetRect(chip.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, new Vector2(112f, 34f));
        PixelBorder(chip.rectTransform, "ChipFrame", new Color(1f, 1f, 1f, 0.55f), 2f);
        Text(label, chip.rectTransform, 16f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
    }

    private static Color AttributeColor(string label)
    {
        switch ((label ?? string.Empty).ToUpperInvariant())
        {
            case "GIVER": return new Color(0.42f, 0.74f, 0.45f, 1f);
            case "MATCH": return new Color(0.92f, 0.66f, 0.22f, 1f);
            case "TAKER": return new Color(0.86f, 0.34f, 0.36f, 1f);
            default: return new Color(0.62f, 0.62f, 0.66f, 1f);
        }
    }

    private static void QuestBadge(RectTransform parent, Vector2 pos)
    {
        var badge = ImageRoot("QuestBadge", parent, Coral);
        badge.raycastTarget = false;
        SetRect(badge.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), pos, new Vector2(42f, 42f));
        PixelBorder(badge.rectTransform, "BadgeFrame", Ink, 2f);
        Text("!", badge.rectTransform, 23f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
    }

    private static void CharacterRelationNode(RectTransform parent, string name, Vector2 pos, Color color, string mark, string hint)
    {
        var root = RectRoot("RelationNode" + name, parent);
        SetRect(root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), pos, new Vector2(116f, 132f));
        var hit = root.gameObject.AddComponent<Image>();
        hit.color = new Color(1f, 1f, 1f, 0f);
        hit.raycastTarget = true;
        var target = root.gameObject.AddComponent<CharacterRelationHintTarget>();
        target.Configure(null, hint, new Vector2(118f, 34f));
        var icon = ImageRoot("Icon", root, color);
        icon.raycastTarget = true;
        SetRect(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(82f, 82f));
        PixelBorder(icon.rectTransform, "IconFrame", Ink, 3f);
        Text(mark, icon.rectTransform, 23f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        Text(name, root, 16f, FontStyles.Bold, TextAlignmentOptions.Center, Ink, new Vector2(0f, -88f), new Vector2(116f, 32f));
    }

    private static void RelationLine(RectTransform parent, Vector2 from, Vector2 to, Color color)
    {
        var line = ImageRoot("RelationLine", parent, color);
        line.raycastTarget = false;
        var center = (from + to) * 0.5f;
        var delta = to - from;
        SetRect(line.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), center, new Vector2(delta.magnitude, 5f));
        line.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    private static void QuestCard(RectTransform parent, int index)
    {
        var card = Panel(parent, "QuestCard" + index, new Vector2(22f, -20f - index * 86f), new Vector2(596f, 68f), index % 2 == 0 ? Cream : new Color(0.98f, 0.89f, 0.74f, 1f));
        EnsureButton(card);
        Circle(card.rectTransform, "QuestIcon", new Vector2(18f, -10f), 48f, index % 2 == 0 ? Yellow : Coral);
        AddMiniBars(card.rectTransform, new Vector2(84f, -14f), 2);
        var progress = ImageRoot("Progress", card.rectTransform, Mint);
        SetRect(progress.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(84f, 12f), new Vector2(320f - index * 30f, 10f));
    }

    private static Button QuestTabButton(RectTransform parent, string name, string label, Vector2 pos, Vector2 size, Color color)
    {
        var tab = ButtonRoot(name, parent, color);
        SetRect(tab.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, size);
        PixelBorder(tab.GetComponent<RectTransform>(), "TabFrame", Ink, 3f);
        Text(label, tab.GetComponent<RectTransform>(), 17f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
        return tab;
    }

    private static Button QuestInboxCard(RectTransform parent, int index, string title, string mark, Color color)
    {
        var card = Panel(parent, "QuestCardInbox" + index, new Vector2(12f, -56f - index * 112f), new Vector2(270f, 94f), index == 0 ? new Color(1f, 0.88f, 0.88f, 1f) : Cream);
        var button = EnsureButton(card);
        PixelBorder(card.rectTransform, "CardFrame", index == 0 ? Coral : Ink, 3f);
        var icon = Circle(card.rectTransform, "QuestIcon", new Vector2(14f, -18f), 54f, color);
        Text(mark, icon.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        TextBox(title, card.rectTransform, 16f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(82f, -18f), new Vector2(166f, 24f));
        AddMiniBars(card.rectTransform, new Vector2(84f, -54f), 2);
        TextBox(index == 0 ? "NEW" : (index + 1) + "/6", card.rectTransform, 13f, FontStyles.Bold, TextAlignmentOptions.Right,
            index == 0 ? Coral : Ink, new Vector2(82f, -54f), new Vector2(166f, 22f));
        return button;
    }

    private static void QuestRouteStrip(RectTransform parent, Vector2 pos, Vector2 size)
    {
        var route = PixelPanel(parent, "QuestRoutePanel", pos, size, new Color(0.86f, 0.93f, 0.99f, 1f));
        TextBox("ROUTE", route.rectTransform, 15f, FontStyles.Bold, TextAlignmentOptions.Left, Ink,
            new Vector2(16f, -12f), new Vector2(258f, 22f));
        RelationLine(route.rectTransform, new Vector2(92f, -76f), new Vector2(194f, -76f), Lavender);
        RelationLine(route.rectTransform, new Vector2(194f, -76f), new Vector2(296f, -76f), Lavender);
        RelationLine(route.rectTransform, new Vector2(296f, -76f), new Vector2(398f, -76f), Lavender);
        QuestCaseNode(route.rectTransform, "Start", "★", new Vector2(50f, -50f), Coral);
        QuestCaseNode(route.rectTransform, "Yui", "Y", new Vector2(152f, -50f), Lavender);
        QuestCaseNode(route.rectTransform, "Map", "M", new Vector2(254f, -50f), Mint);
        QuestCaseNode(route.rectTransform, "Goal", "!", new Vector2(356f, -50f), Yellow);
    }

    private static void QuestCharacterChip(RectTransform parent, string name, string mark, Vector2 pos, Color color)
    {
        var chip = Panel(parent, "Related" + name, pos, new Vector2(56f, 58f), new Color(1f, 1f, 1f, 0.42f));
        var icon = Circle(chip.rectTransform, "Icon", new Vector2(8f, -6f), 38f, color);
        Text(mark, icon.rectTransform, 15f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        TextBox(name, chip.rectTransform, 10f, FontStyles.Bold, TextAlignmentOptions.Center, Ink,
            new Vector2(4f, -44f), new Vector2(48f, 16f));
    }

    private static void QuestCaseNode(RectTransform parent, string name, string mark, Vector2 pos, Color color)
    {
        var node = Panel(parent, "QuestNode" + name, pos, new Vector2(68f, 68f), new Color(1f, 1f, 1f, 0.74f));
        PixelBorder(node.rectTransform, "NodeFrame", Ink, 3f);
        var icon = Circle(node.rectTransform, "NodeIcon", new Vector2(8f, -8f), 50f, color);
        Text(mark, icon.rectTransform, 18f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
    }

    private static void MapZone(RectTransform parent, string name, Vector2 pos, Vector2 size, Color color, bool selected)
    {
        var zone = Panel(parent, name, pos, size, selected ? color : new Color(color.r, color.g, color.b, 0.55f));
        EnsureButton(zone);
        var pin = Circle(zone.rectTransform, "Pin", new Vector2(size.x * 0.5f - 18f, -size.y * 0.5f + 18f), 36f, selected ? Coral : Cream);
        pin.raycastTarget = false;
        Text(selected ? "!" : "?", pin.rectTransform, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
    }

    private static RawImage RawImageRoot(string name, RectTransform parent, string texturePath, Rect uvRect)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<RawImage>();
        image.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        image.uvRect = uvRect;
        image.raycastTarget = false;
        return image;
    }

    private static Slider SliderRoot(string name, RectTransform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, size);
        var background = ImageRoot("Background", rect, new Color(0.14f, 0.13f, 0.28f, 0.84f));
        Stretch(background.rectTransform, Vector2.zero, Vector2.zero);
        PixelBorder(background.rectTransform, "Frame", Cream, 2f);
        var fillArea = RectRoot("Fill Area", rect);
        Stretch(fillArea, new Vector2(4f, 4f), new Vector2(-4f, -4f));
        var fill = ImageRoot("Fill", fillArea, Mint);
        Stretch(fill.rectTransform, Vector2.zero, Vector2.zero);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        var slider = go.GetComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.targetGraphic = fill;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.82f;
        return slider;
    }

    private static void ConfigureMapLocations(SerializedObject so)
    {
        var names = new[] { "駅前", "市立図書館", "オフィス街", "繁華街", "中央公園", "シティホテル" };
        var descriptions = new[]
        {
            "待ち合わせと移動の起点。夕方は帰宅客で混み合う。",
            "新聞・業界誌・過去資料を静かに調べられる公共施設。",
            "昼は会社パートの中心。退勤前後には対象と接触しやすい。",
            "飲食店や遊技施設が集まる夜の情報交換エリア。",
            "人目を避けて話せる場所が多い、街の中央にある公園。",
            "夜になると人の流れが変わる、格式の高い宿泊施設。"
        };
        var hints = new[]
        {
            "乗り換え前なら、少しだけ話を聞けそう。",
            "過去の記事を調べるなら、ここがいちばん確実。",
            "昼休みか退勤直後を狙うのが安全そう。",
            "夜は出会いが増えるけど、出費にも注意してね。",
            "噴水の近くなら、落ち着いて話せるかも。",
            "ロビーの顔ぶれは日没後に変わるみたい。"
        };
        var safety = new[] { 0.86f, 0.94f, 0.68f, 0.56f, 0.91f, 0.74f };
        var items = new[] { "交通ICカード", "図書館利用証", "社員証", "ゲームコイン", "テイクアウトコーヒー", "ルームキー" };
        var characters = new[] { "ReRe / 対象A", "情報屋B / 司書", "取引先の元担当 / ReRe", "同業者A / ReRe", "対象A / ReRe", "ReRe / ホテルスタッフ" };
        var dayColors = new[] { Cyan, Lavender, new Color(0.62f, 0.82f, 0.94f, 1f), Coral, Mint, Yellow };
        var nightColors = new[]
        {
            new Color(0.38f, 0.48f, 0.86f, 1f), new Color(0.52f, 0.43f, 0.78f, 1f), new Color(0.34f, 0.48f, 0.74f, 1f),
            new Color(0.88f, 0.34f, 0.76f, 1f), new Color(0.30f, 0.66f, 0.58f, 1f), new Color(0.92f, 0.48f, 0.68f, 1f)
        };
        var property = so.FindProperty("locations");
        property.arraySize = names.Length;
        for (var i = 0; i < names.Length; i++)
        {
            var item = property.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("baseName").stringValue = names[i];
            item.FindPropertyRelative("description").stringValue = descriptions[i];
            item.FindPropertyRelative("rereHint").stringValue = hints[i];
            item.FindPropertyRelative("safety").floatValue = safety[i];
            item.FindPropertyRelative("relatedItemName").stringValue = items[i];
            item.FindPropertyRelative("relatedCharacterName").stringValue = characters[i];
            item.FindPropertyRelative("dayColor").colorValue = dayColors[i];
            item.FindPropertyRelative("nightColor").colorValue = nightColors[i];
        }
    }

    private static void SetIntArray(SerializedObject so, string name, int[] values)
    {
        var property = so.FindProperty(name);
        if (property == null) return;
        property.arraySize = values.Length;
        for (var i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).intValue = values[i];
    }

    private static void Road(RectTransform parent, Vector2 pos, Vector2 size)
    {
        Panel(parent, "Road", pos, size, new Color(1f, 1f, 1f, 0.52f));
    }

    private static void Meter(RectTransform parent, int index)
    {
        var bg = ImageRoot("BonusMeter" + index, parent, new Color(1f, 1f, 1f, 0.35f));
        SetRect(bg.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -32f - index * 40f), new Vector2(228f, 18f));
        var fill = ImageRoot("Fill", bg.rectTransform, Yellow);
        Stretch(fill.rectTransform, Vector2.zero, new Vector2(-60f + index * 8f, 0f));
    }

    private static void AddDetailBars(RectTransform parent, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var bar = ImageRoot("TextBar" + i, parent, new Color(0.28f, 0.20f, 0.32f, 0.22f));
            SetRect(bar.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(170f, -28f - i * 36f), new Vector2(Mathf.Max(90f, 210f - i * 18f), 18f));
        }
    }

    private static void AddMiniBars(RectTransform parent, Vector2 pos, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var bar = ImageRoot("MiniBar" + i, parent, new Color(0.28f, 0.20f, 0.32f, 0.22f));
            SetRect(bar.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(pos.x, pos.y - i * 24f), new Vector2(130f - i * 22f, 12f));
        }
    }

    private static Image Circle(RectTransform parent, string name, Vector2 pos, float size, Color color)
    {
        var image = Panel(parent, name, pos, new Vector2(size, size), color);
        return image;
    }

    private static Image ImageRoot(string name, RectTransform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = GetDefaultSprite();
        image.type = Image.Type.Simple;
        image.color = color;
        return image;
    }

    private static RectTransform RectRoot(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static Button ButtonRoot(string name, RectTransform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = GetDefaultSprite();
        image.type = Image.Type.Simple;
        image.color = color;
        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        button.colors = colors;
        return button;
    }

    private static TextMeshProUGUI Text(string value, RectTransform parent, float size, FontStyles style, TextAlignmentOptions alignment, Color color)
    {
        return Text(value, parent, size, style, alignment, color, Vector2.zero, Vector2.zero);
    }

    private static TextMeshProUGUI Text(string value, RectTransform parent, float size, FontStyles style, TextAlignmentOptions alignment, Color color, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        Stretch(rt, offsetMin, offsetMax);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.font = FindPixelFontAsset();
        return text;
    }

    private static TextMeshProUGUI TextBox(string value, RectTransform parent, float size, FontStyles style, TextAlignmentOptions alignment, Color color, Vector2 pos, Vector2 boxSize)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        SetRect(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), pos, boxSize);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.font = FindPixelFontAsset();
        return text;
    }

    private static TextMeshProUGUI TextBoxCentered(string value, RectTransform parent, float size, FontStyles style, TextAlignmentOptions alignment, Color color, Rect sourceRect, float artworkWidth, float artworkHeight)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        SetRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), DressCenter(sourceRect, artworkWidth, artworkHeight), DressSize(sourceRect, artworkWidth, artworkHeight));
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.font = FindPixelFontAsset();
        return text;
    }

    private static TMP_FontAsset FindPixelFontAsset()
    {
        var pixelFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/PixelMplus12-Regular SDF.asset");
        return pixelFont ? pixelFont : TMP_Settings.defaultFontAsset;
    }

    private static void ConfigureRoot(MenuRootV2UI ui, MenuRootV2OrientationTransition orientationTransition, GameObject standardPhoneLayer, GameObject topPage, GameObject statusPage, GameObject itemsPage,
        GameObject charactersPage, GameObject questPage, GameObject mapPage, GameObject savePage, GameObject settingsPage, Button top, Button status, Button items,
        Button characters, Button quest, Button map, Button save, Button settings, MenuTopReReMascot topMascot, Button dressTile,
        Button statusTile, Button itemsTile, Button charactersTile, Button questTile, Button mapTile, Button dressHome,
        Button dressDress, Button dressStatus, Button dressItems, Button dressMap, Button charactersHome, Button charactersDress,
        Button charactersItems, Button charactersCharacters, Button charactersQuest, Button charactersMap, Button itemsHome,
        Button itemsDress, Button itemsItems, Button itemsCharacters, Button itemsQuest, Button itemsMap)
    {
        var so = new SerializedObject(ui);
        SetBool(so, "visibleOnAwake", false);
        SetObject(so, "orientationTransition", orientationTransition);
        SetObject(so, "standardPhoneLayer", standardPhoneLayer);
        SetObject(so, "pageTop", topPage);
        SetObject(so, "pageStatus", statusPage);
        SetObject(so, "pageItems", itemsPage);
        SetObject(so, "pageCharacters", charactersPage);
        SetObject(so, "pageQuest", questPage);
        SetObject(so, "pageMap", mapPage);
        SetObject(so, "pageSave", savePage);
        SetObject(so, "pageSettings", settingsPage);
        SetObject(so, "topButton", top);
        SetObject(so, "statusButton", status);
        SetObject(so, "itemsButton", items);
        SetObject(so, "charactersButton", characters);
        SetObject(so, "questButton", quest);
        SetObject(so, "mapButton", map);
        SetObject(so, "saveButton", save);
        SetObject(so, "settingsButton", settings);
        SetObject(so, "dressHomeButton", dressHome);
        SetObject(so, "dressDressButton", dressDress);
        SetObject(so, "dressStatusButton", dressStatus);
        SetObject(so, "dressItemsButton", dressItems);
        SetObject(so, "dressMapButton", dressMap);
        SetObject(so, "charactersHomeButton", charactersHome);
        SetObject(so, "charactersDressButton", charactersDress);
        SetObject(so, "charactersItemsButton", charactersItems);
        SetObject(so, "charactersCharactersButton", charactersCharacters);
        SetObject(so, "charactersQuestButton", charactersQuest);
        SetObject(so, "charactersMapButton", charactersMap);
        SetObject(so, "itemsHomeButton", itemsHome);
        SetObject(so, "itemsDressButton", itemsDress);
        SetObject(so, "itemsItemsButton", itemsItems);
        SetObject(so, "itemsCharactersButton", itemsCharacters);
        SetObject(so, "itemsQuestButton", itemsQuest);
        SetObject(so, "itemsMapButton", itemsMap);
        SetObject(so, "topMascot", topMascot);
        SetObject(so, "dressTileButton", dressTile);
        SetObject(so, "statusTileButton", statusTile);
        SetObject(so, "itemsTileButton", itemsTile);
        SetObject(so, "charactersTileButton", charactersTile);
        SetObject(so, "questTileButton", questTile);
        SetObject(so, "mapTileButton", mapTile);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(ui);
    }

    private static void ConfigureOrientation(MenuRootV2OrientationTransition controller, RectTransform portraitPhoneFrame,
        RectTransform sharedLandscapePhoneFrame, params GameObject[] sharedLandscapePages)
    {
        if (!controller)
            return;

        var so = new SerializedObject(controller);
        SetObject(so, "portraitPhoneFrame", portraitPhoneFrame);
        SetObject(so, "sharedLandscapePhoneFrame", sharedLandscapePhoneFrame);
        var pages = so.FindProperty("sharedLandscapePages");
        if (pages != null)
        {
            pages.arraySize = sharedLandscapePages.Length;
            for (var i = 0; i < sharedLandscapePages.Length; i++)
                pages.GetArrayElementAtIndex(i).objectReferenceValue = sharedLandscapePages[i];
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
    }

    private static void SetPreviewVisibleOnAwake(GameObject root, bool visible)
    {
        if (!root)
            return;

        var ui = root.GetComponent<MenuRootV2UI>();
        if (!ui)
            return;

        var so = new SerializedObject(ui);
        SetBool(so, "visibleOnAwake", visible);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(ui);
    }

    private static void SetObject(SerializedObject so, string name, Object value)
    {
        var prop = so.FindProperty(name);
        if (prop != null)
            prop.objectReferenceValue = value;
    }

    private static void SetBool(SerializedObject so, string name, bool value)
    {
        var prop = so.FindProperty(name);
        if (prop != null)
            prop.boolValue = value;
    }

    private static void SetFloat(SerializedObject so, string name, float value)
    {
        var prop = so.FindProperty(name);
        if (prop != null)
            prop.floatValue = value;
    }

    private static void SetVector2(SerializedObject so, string name, Vector2 value)
    {
        var prop = so.FindProperty(name);
        if (prop != null)
            prop.vector2Value = value;
    }

    private static void SetSpriteArray(SerializedObject so, string name, Sprite[] sprites)
    {
        var prop = so.FindProperty(name);
        if (prop == null)
            return;

        prop.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
    }

    private static void SetStringArray(SerializedObject so, string name, string[] values)
    {
        var prop = so.FindProperty(name);
        if (prop == null)
            return;

        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).stringValue = values[i];
    }

    private static void SetMotionSets(SerializedObject so)
    {
        var prop = so.FindProperty("motionSets");
        if (prop == null)
            return;

        var sets = new (string id, string folder, float weight, float frameRate, bool walkMotion, bool showBubble, string nextId, int loops, float hold, bool loop)[]
        {
            ("idle_talk", TopReReTalkFolder, 1f, 5f, false, false, "", 1, 0f, true)
        };

        prop.arraySize = sets.Length;
        for (int i = 0; i < sets.Length; i++)
        {
            var element = prop.GetArrayElementAtIndex(i);
            SetChildString(element, "id", sets[i].id);
            SetChildSpriteArray(element, "frames", LoadSpritesFromFolder(sets[i].folder));
            SetChildFloat(element, "weight", sets[i].weight);
            SetChildFloat(element, "frameRate", sets[i].frameRate);
            SetChildBool(element, "walkMotion", sets[i].walkMotion);
            SetChildBool(element, "showBubble", sets[i].showBubble);
            SetChildString(element, "nextId", sets[i].nextId);
            SetChildInt(element, "loopsBeforeNext", sets[i].loops);
            SetChildFloat(element, "holdSeconds", sets[i].hold);
            SetChildBool(element, "loop", sets[i].loop);
        }
    }

    private static void SetDressTalkSets(SerializedObject so)
    {
        var prop = so.FindProperty("outfitTalkSets");
        if (prop == null)
            return;

        var sets = new (string outfitId, string folder)[]
        {
            ("room", DressBodyMotionFolder + "/talk_idle"),
            ("date", DressBodyMotionFolder + "/wink_wave"),
            ("work", DressBodyMotionFolder + "/glasses_adjust"),
            ("cyber", DressBodyMotionFolder + "/fired_up"),
            ("formal", DressBodyMotionFolder + "/secret_whisper"),
            ("casual", DressBodyMotionFolder + "/gyaru_playful")
        };

        prop.arraySize = sets.Length;
        for (int i = 0; i < sets.Length; i++)
        {
            var element = prop.GetArrayElementAtIndex(i);
            SetChildString(element, "outfitId", sets[i].outfitId);
            SetChildSpriteArray(element, "frames", LoadSpritesFromFolder(sets[i].folder));
        }
    }

    private static void SetOutfitChangeSets(SerializedObject so)
    {
        var prop = so.FindProperty("outfitChangeSets");
        if (prop == null)
            return;

        var sets = new (string outfitId, string folder)[]
        {
            ("room", DressChangeFolder + "/room_blouse_black_skirt"),
            ("date", DressChangeFolder + "/beige_cardigan_uniform")
        };

        prop.arraySize = sets.Length;
        for (int i = 0; i < sets.Length; i++)
        {
            var element = prop.GetArrayElementAtIndex(i);
            SetChildString(element, "outfitId", sets[i].outfitId);
            SetChildSpriteArray(element, "frames", LoadSpritesFromFolder(sets[i].folder));
        }
    }

    private static void SetChildString(SerializedProperty parent, string name, string value)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop != null)
            prop.stringValue = value;
    }

    private static void SetChildFloat(SerializedProperty parent, string name, float value)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop != null)
            prop.floatValue = value;
    }

    private static void SetChildBool(SerializedProperty parent, string name, bool value)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop != null)
            prop.boolValue = value;
    }

    private static void SetChildInt(SerializedProperty parent, string name, int value)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop != null)
            prop.intValue = value;
    }

    private static void SetChildSpriteArray(SerializedProperty parent, string name, Sprite[] sprites)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop == null)
            return;

        prop.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
    }

    private static void SetPrefabVisibleOnAwake(string prefabPath, bool visible)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (!prefab)
            return;

        var ui = prefab.GetComponent<MenuRootV2UI>();
        if (!ui)
            return;

        var so = new SerializedObject(ui);
        SetBool(so, "visibleOnAwake", visible);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(ui);
        AssetDatabase.SaveAssets();
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
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PixelFillSpritePath);
        if (sprite)
            return sprite;

        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply(false, false);
        File.WriteAllBytes(PixelFillSpritePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(PixelFillSpritePath, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(PixelFillSpritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(PixelFillSpritePath);
    }

    private static Sprite LoadSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite[] LoadSpritesFromFolder(string folder)
    {
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        System.Array.Sort(guids, (a, b) => string.CompareOrdinal(AssetDatabase.GUIDToAssetPath(a), AssetDatabase.GUIDToAssetPath(b)));
        var sprites = new System.Collections.Generic.List<Sprite>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                continue;

            var sprite = LoadSprite(path);
            if (sprite)
                sprites.Add(sprite);
        }

        return sprites.ToArray();
    }

    private static Sprite[] LoadSpritesFromSheet(string path)
    {
        var sprites = new System.Collections.Generic.List<Sprite>();
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset is Sprite sprite)
                sprites.Add(sprite);
        }

        sprites.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return sprites.ToArray();
    }

    private static T FindFirstAssetOfType<T>() where T : Object
    {
        var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { "Assets/Database" });
        if (guids.Length == 0)
            return null;
        System.Array.Sort(guids, (left, right) => string.CompareOrdinal(AssetDatabase.GUIDToAssetPath(left), AssetDatabase.GUIDToAssetPath(right)));
        return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private static CharacterInfo[] GetCharactersByCategory(CharacterDatabase database, CharacterCategory category, int capacity)
    {
        var result = new CharacterInfo[capacity];
        if (!database) return result;

        var writeIndex = 0;
        foreach (var character in database.GetAll())
        {
            if (!character || character.category != category) continue;
            result[writeIndex++] = character;
            if (writeIndex == result.Length) break;
        }

        return result;
    }

    private static int FindCharacterIndex(CharacterDatabase database, CharacterInfo character)
    {
        if (!database || !character) return -1;
        var characters = database.GetAll();
        for (var i = 0; i < characters.Count; i++)
            if (characters[i] == character) return i;
        return -1;
    }

    private static string GetContactName(CharacterInfo character)
    {
        if (!character) return "???";
        if (!string.IsNullOrWhiteSpace(character.displayName)) return character.displayName;
        if (!string.IsNullOrWhiteSpace(character.id)) return character.id;
        return character.name;
    }

    private static void EnsureFolder(string path)
    {
        var parts = path.Split('/');
        var current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static Camera AddPreviewCamera()
    {
        var cameraObject = new GameObject("PreviewCamera", typeof(Camera));
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        var camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.07f, 0.10f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 540f;
        var additionalCameraDataType = FindType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
        if (additionalCameraDataType != null)
            cameraObject.AddComponent(additionalCameraDataType);
        return camera;
    }

    private static void SetPreviewCanvasCamera(GameObject instance, Camera previewCamera)
    {
        var canvas = instance.GetComponent<Canvas>();
        if (!canvas || !previewCamera)
            return;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = previewCamera;
        canvas.planeDistance = 10f;
        EditorUtility.SetDirty(canvas);
    }

    private static System.Type FindType(string fullName)
    {
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(fullName);
            if (type != null)
                return type;
        }

        return null;
    }
}
