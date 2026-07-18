using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds and presents the title screen. It can build itself at runtime so an
/// existing TitleScene remains usable before the editor builder is run.
/// </summary>
public sealed class TitleScreenPresentation : MonoBehaviour
{
    private const string BackgroundResourcePath = "Title/Title_Background_Light_1920x1080";
    private const string GhostResourcePath = "Title/Title_Character_Ghost_1920x1080";
    private const string LogoResourcePath = "Title/Title_Logo_EN_1920x1080";
    private const string SubtitleResourcePath = "Title/Title_Subtitle_JP_Compact_1920x1080";
    private const string GlintResourcePath = "Title/Title_Glint";
    private const string LightSweepResourcePath = "Title/Title_LightSweep";
    private const string BgmResourcePath = "Title/Title_BGM";
    private const string SelectSfxResourcePath = "Title/title_cursor_move";
    private const string ConfirmSfxResourcePath = "Title/title_confirm";

    [Header("Presentation")]
    [SerializeField] private Canvas titleCanvas;
    [SerializeField] private CanvasGroup backgroundGroup;
    [SerializeField] private CanvasGroup characterGroup;
    [SerializeField] private CanvasGroup logoGroup;
    [SerializeField] private CanvasGroup glintGroup;
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private CanvasGroup pressAnyButtonGroup;
    [SerializeField] private CanvasGroup menuGroup;
    [SerializeField] private CanvasGroup[] menuItemGroups;
    [SerializeField] private RectTransform characterRoot;
    [SerializeField] private RectTransform titleRoot;
    [SerializeField] private RectTransform subtitleRoot;
    [SerializeField] private RectTransform lightSweepRoot;
    [SerializeField] private Graphic lightSweepGraphic;

    [Header("Actions")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TitleSceneController sceneController;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource confirmSfxSource;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip selectSfx;
    [SerializeField] private AudioClip confirmSfx;
    [SerializeField] private AudioClip titleBgm;
    [SerializeField, Range(0f, 1f)] private float bgmBaseVolume = 0.26f;
    [SerializeField, Min(0.01f)] private float bgmFadeInDuration = 0.65f;
    [SerializeField, Min(0.01f)] private float bgmFadeOutDuration = 0.55f;

    [Header("Timing")]
    [SerializeField, Min(0.01f)] private float backgroundFadeDuration = 0.65f;
    [SerializeField, Min(0.01f)] private float characterFadeDuration = 0.8f;
    [SerializeField, Min(0.01f)] private float logoFadeDuration = 0.65f;
    [SerializeField, Min(0.01f)] private float subtitleFadeDuration = 0.45f;
    [SerializeField, Min(0.01f)] private float menuTransitionDuration = 0.45f;
    [SerializeField, Min(0f)] private float menuItemStagger = 0.11f;

    private enum State { Intro, PressAnyButton, OpeningMenu, Menu }

    private State state;
    private Coroutine presentationRoutine;
    private Coroutine glintRoutine;
    private Coroutine bgmVolumeRoutine;
    private Vector2 titleStartPosition;
    private Vector2 subtitleStartPosition;
    private int lastSelectedButtonId;
    private bool actionPending;
    private float nextEventSystemCheckTime;

    /// <summary>Used by TitleSceneController to suppress its legacy Return-key fallback.</summary>
    public bool HandlesInput => isActiveAndEnabled;

    private void Awake()
    {
        BuildPresentation();
        ResolveOptionalAssets();
        ConfigureAudio();
        ConfigureButtons();
        ResetVisualState();
    }

    private IEnumerator Start()
    {
        // Naninovel creates its EventSystem during engine initialization. Waiting one
        // frame lets us reuse it instead of briefly registering a second system.
        yield return null;
        EnsureEventSystem();
        RemoveDuplicateEventSystems();
        presentationRoutine = StartCoroutine(PlayIntro());
    }

    private void Update()
    {
        if (Time.unscaledTime >= nextEventSystemCheckTime)
        {
            nextEventSystemCheckTime = Time.unscaledTime + 0.5f;
            RemoveDuplicateEventSystems();
        }

        if (state == State.Intro && WasAnyInputPressed())
        {
            if (presentationRoutine != null)
                StopCoroutine(presentationRoutine);

            ShowPressAnyButtonImmediately();
        }
        else if (state == State.PressAnyButton && WasAnyInputPressed())
        {
            presentationRoutine = StartCoroutine(OpenMenu());
        }
    }

    /// <summary>
    /// Creates the title hierarchy when the scene has not been rebuilt yet.
    /// This is also called by TitleSceneBuilder when saving TitleScene.unity.
    /// </summary>
    public void BuildPresentation()
    {
        if (titleCanvas != null)
            return;

        var existing = transform.Find("TitleScreenCanvas");
        if (existing != null)
        {
            titleCanvas = existing.GetComponent<Canvas>();
            if (titleCanvas != null)
                CacheExistingPresentation(existing);
            if (titleCanvas != null)
                return;
        }

        var canvasRoot = new GameObject("TitleScreenCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasRoot.transform.SetParent(transform, false);
        titleCanvas = canvasRoot.GetComponent<Canvas>();
        titleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        titleCanvas.sortingOrder = 500;

        var scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        BuildHierarchy(canvasRoot.GetComponent<RectTransform>());
    }

    /// <summary>Editor-only callers may use this to regenerate the canvas hierarchy.</summary>
    public void RebuildPresentation()
    {
        var existing = transform.Find("TitleScreenCanvas");
        if (existing != null)
        {
            if (Application.isPlaying)
                Destroy(existing.gameObject);
            else
                DestroyImmediate(existing.gameObject);
        }

        titleCanvas = null;
        BuildPresentation();
    }

    private void BuildHierarchy(RectTransform canvasRoot)
    {
        backgroundGroup = CreateArtworkLayer("Background", canvasRoot, BackgroundResourcePath, out _);
        characterGroup = CreateArtworkLayer("GhostCharacters", canvasRoot, GhostResourcePath, out characterRoot);

        titleRoot = CreateRoot("TitleArtwork", canvasRoot);
        Stretch(titleRoot);
        logoGroup = CreateArtworkLayer("EnglishLogo", titleRoot, LogoResourcePath, out _);
        glintGroup = CreateArtworkLayer("Glint", titleRoot, GlintResourcePath, out var glintRoot);
        SetAnchoredRect(glintRoot, new Vector2(0.5f, 0.5f), new Vector2(420f, -100f), new Vector2(170f, 170f));
        glintGroup.alpha = 0.7f;
        var sweepGroup = CreateArtworkLayer("LightSweep", titleRoot, LightSweepResourcePath, out lightSweepRoot);
        SetAnchoredRect(lightSweepRoot, new Vector2(0.5f, 0.5f), new Vector2(-1000f, -60f), new Vector2(230f, 590f));
        lightSweepGraphic = sweepGroup.GetComponent<Graphic>();

        subtitleGroup = CreateArtworkLayer("CompactSubtitle", titleRoot, SubtitleResourcePath, out subtitleRoot);
        pressAnyButtonGroup = CreatePrompt(canvasRoot);
        menuGroup = CreateMenu(canvasRoot, out menuItemGroups);
    }

    private void CacheExistingPresentation(Transform root)
    {
        backgroundGroup = FindGroup(root, "Background");
        characterGroup = FindGroup(root, "GhostCharacters");
        logoGroup = FindGroup(root, "TitleArtwork/EnglishLogo");
        glintGroup = FindGroup(root, "TitleArtwork/Glint");
        subtitleGroup = FindGroup(root, "TitleArtwork/CompactSubtitle");
        pressAnyButtonGroup = FindGroup(root, "PressAnyButton");
        menuGroup = FindGroup(root, "Menu");
        characterRoot = characterGroup != null ? characterGroup.GetComponent<RectTransform>() : null;
        titleRoot = root.Find("TitleArtwork") as RectTransform;
        subtitleRoot = subtitleGroup != null ? subtitleGroup.GetComponent<RectTransform>() : null;
        lightSweepRoot = root.Find("TitleArtwork/LightSweep") as RectTransform;
        lightSweepGraphic = lightSweepRoot != null ? lightSweepRoot.GetComponent<Graphic>() : null;
        newGameButton = FindButton(root, "NewGame");
        continueButton = FindButton(root, "Continue");
        optionsButton = FindButton(root, "Options");
        quitButton = FindButton(root, "Quit");

        if (menuGroup != null)
        {
            var buttons = menuGroup.GetComponentsInChildren<Button>(true);
            menuItemGroups = new CanvasGroup[buttons.Length];
            for (var i = 0; i < buttons.Length; i++)
                menuItemGroups[i] = buttons[i].GetComponent<CanvasGroup>();
        }
    }

    private void ResolveOptionalAssets()
    {
        selectSfx ??= Resources.Load<AudioClip>(SelectSfxResourcePath);
        confirmSfx ??= Resources.Load<AudioClip>(ConfirmSfxResourcePath);
        titleBgm ??= Resources.Load<AudioClip>(BgmResourcePath);
        sceneController ??= GetComponent<TitleSceneController>();
    }

    private void ConfigureAudio()
    {
        sfxSource ??= GetComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        if (confirmSfxSource == null)
        {
            var confirmObject = new GameObject("TitleConfirmSFX", typeof(AudioSource));
            confirmObject.transform.SetParent(transform, false);
            confirmSfxSource = confirmObject.GetComponent<AudioSource>();
        }
        confirmSfxSource.playOnAwake = false;
        confirmSfxSource.spatialBlend = 0f;

        var confirmEcho = confirmSfxSource.GetComponent<AudioEchoFilter>();
        if (confirmEcho == null)
            confirmEcho = confirmSfxSource.gameObject.AddComponent<AudioEchoFilter>();
        confirmEcho.delay = 72f;
        confirmEcho.decayRatio = 0.16f;
        confirmEcho.dryMix = 0.96f;
        confirmEcho.wetMix = 0.10f;

        if (bgmSource == null)
        {
            var bgmObject = new GameObject("TitleBGM", typeof(AudioSource));
            bgmObject.transform.SetParent(transform, false);
            bgmSource = bgmObject.GetComponent<AudioSource>();
        }

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
        if (titleBgm != null && !bgmSource.isPlaying)
        {
            bgmSource.clip = titleBgm;
            bgmSource.volume = 0f;
            bgmSource.Play();
            bgmVolumeRoutine = StartCoroutine(FadeBgmVolume(0f, bgmBaseVolume, bgmFadeInDuration));
        }
    }

    private void ConfigureButtons()
    {
        ConfigureButton(newGameButton, true, () => sceneController?.StartGame());
        ConfigureButton(continueButton, false, null);
        ConfigureButton(optionsButton, false, null);
        ConfigureButton(quitButton, true, () => sceneController?.QuitGame());
    }

    private void ConfigureButton(Button button, bool enabled, Action action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.interactable = enabled;
        if (enabled && action != null)
            button.onClick.AddListener(() => StartCoroutine(ConfirmAndInvoke(action)));

        var trigger = button.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
        trigger.triggers ??= new System.Collections.Generic.List<EventTrigger.Entry>();
        trigger.triggers.Clear();
        var select = new EventTrigger.Entry { eventID = EventTriggerType.Select };
        select.callback.AddListener(_ => SelectButton(button));
        trigger.triggers.Add(select);
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => SelectButton(button));
        trigger.triggers.Add(enter);
    }

    private void ResetVisualState()
    {
        state = State.Intro;
        titleStartPosition = titleRoot != null ? titleRoot.anchoredPosition : Vector2.zero;
        subtitleStartPosition = subtitleRoot != null ? subtitleRoot.anchoredPosition : Vector2.zero;
        SetGroup(backgroundGroup, 0f, false);
        SetGroup(characterGroup, 0f, false);
        SetGroup(logoGroup, 0f, false);
        SetGroup(glintGroup, 0f, false);
        SetGroup(subtitleGroup, 0f, false);
        SetGroup(pressAnyButtonGroup, 0f, false);
        SetGroup(menuGroup, 0f, false);
        if (characterRoot != null) characterRoot.localScale = Vector3.one * 1.03f;
        if (subtitleRoot != null) subtitleRoot.anchoredPosition = subtitleStartPosition + Vector2.down * 12f;
        if (lightSweepGraphic != null) SetGraphicAlpha(lightSweepGraphic, 0f);
        if (lightSweepRoot != null) lightSweepRoot.anchoredPosition = new Vector2(-1000f, -60f);

        if (menuItemGroups != null)
        {
            foreach (var group in menuItemGroups)
                SetGroup(group, 0f, false);
        }
    }

    private IEnumerator PlayIntro()
    {
        yield return FadeGroup(backgroundGroup, 0f, 1f, backgroundFadeDuration);
        yield return FadeCharacter();
        yield return FadeLogoAndSweep();
        yield return FadeSubtitle();
        yield return FadeGroup(pressAnyButtonGroup, 0f, 1f, 0.35f);
        state = State.PressAnyButton;
        StartGlintLoop();
    }

    private IEnumerator FadeCharacter()
    {
        var elapsed = 0f;
        while (elapsed < characterFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / characterFadeDuration);
            if (characterGroup != null) characterGroup.alpha = t;
            if (characterRoot != null) characterRoot.localScale = Vector3.one * Mathf.Lerp(1.03f, 1f, EaseOutCubic(t));
            yield return null;
        }
    }

    private IEnumerator FadeLogoAndSweep()
    {
        var elapsed = 0f;
        while (elapsed < logoFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / logoFadeDuration);
            if (logoGroup != null) logoGroup.alpha = t;
            if (glintGroup != null) glintGroup.alpha = t * 0.7f;
            if (lightSweepGraphic != null) SetGraphicAlpha(lightSweepGraphic, Mathf.Sin(t * Mathf.PI) * 0.8f);
            if (lightSweepRoot != null) lightSweepRoot.anchoredPosition = new Vector2(Mathf.Lerp(-1000f, 1000f, t), -60f);
            yield return null;
        }
        if (lightSweepGraphic != null) SetGraphicAlpha(lightSweepGraphic, 0f);
    }

    private IEnumerator FadeSubtitle()
    {
        var elapsed = 0f;
        while (elapsed < subtitleFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / subtitleFadeDuration);
            if (subtitleGroup != null) subtitleGroup.alpha = t;
            if (subtitleRoot != null) subtitleRoot.anchoredPosition = Vector2.Lerp(subtitleStartPosition + Vector2.down * 12f, subtitleStartPosition, EaseOutCubic(t));
            yield return null;
        }
    }

    private void ShowPressAnyButtonImmediately()
    {
        SetGroup(backgroundGroup, 1f, false);
        SetGroup(characterGroup, 1f, false);
        SetGroup(logoGroup, 1f, false);
        SetGroup(glintGroup, 0.7f, false);
        SetGroup(subtitleGroup, 1f, false);
        SetGroup(pressAnyButtonGroup, 1f, false);
        if (characterRoot != null) characterRoot.localScale = Vector3.one;
        if (subtitleRoot != null) subtitleRoot.anchoredPosition = subtitleStartPosition;
        if (lightSweepGraphic != null) SetGraphicAlpha(lightSweepGraphic, 0f);
        state = State.PressAnyButton;
        StartGlintLoop();
    }

    private IEnumerator OpenMenu()
    {
        state = State.OpeningMenu;
        SetGroup(pressAnyButtonGroup, 0f, false);
        yield return MoveAndScaleTitle();

        SetGroup(menuGroup, 1f, true);
        if (menuItemGroups != null)
        {
            foreach (var group in menuItemGroups)
            {
                if (group == null) continue;
                yield return FadeGroup(group, 0f, 1f, 0.18f);
                SetGroup(group, 1f, true);
                if (menuItemStagger > 0f)
                    yield return new WaitForSecondsRealtime(menuItemStagger);
            }
        }

        state = State.Menu;
        if (newGameButton != null && EventSystem.current != null)
        {
            lastSelectedButtonId = newGameButton.GetInstanceID();
            EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
        }
    }

    private IEnumerator MoveAndScaleTitle()
    {
        if (titleRoot == null)
            yield break;

        var elapsed = 0f;
        var startScale = titleRoot.localScale;
        while (elapsed < menuTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = EaseOutCubic(Mathf.Clamp01(elapsed / menuTransitionDuration));
            titleRoot.localScale = Vector3.Lerp(startScale, Vector3.one * 0.9f, t);
            titleRoot.anchoredPosition = Vector2.Lerp(titleStartPosition, titleStartPosition + Vector2.up * 145f, t);
            yield return null;
        }
    }

    private void SelectButton(Button button)
    {
        if (button == null || !button.interactable || state != State.Menu)
            return;

        var id = button.GetInstanceID();
        if (lastSelectedButtonId == id)
            return;

        lastSelectedButtonId = id;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        if (selectSfx != null && sfxSource != null)
            sfxSource.PlayOneShot(selectSfx);
    }

    private void PlayConfirmSfx()
    {
        if (confirmSfx != null && confirmSfxSource != null)
            confirmSfxSource.PlayOneShot(confirmSfx);
    }

    private IEnumerator ConfirmAndInvoke(Action action)
    {
        if (actionPending)
            yield break;

        actionPending = true;
        PlayConfirmSfx();
        yield return FadeOutBgmForConfirmation();
        action?.Invoke();
        actionPending = false;
    }

    private IEnumerator FadeOutBgmForConfirmation()
    {
        if (bgmSource == null || !bgmSource.isPlaying)
            yield break;

        if (bgmVolumeRoutine != null)
            StopCoroutine(bgmVolumeRoutine);
        yield return FadeBgmVolume(bgmSource.volume, 0f, bgmFadeOutDuration);
        bgmSource.Stop();
    }

    private IEnumerator FadeBgmVolume(float from, float to, float duration)
    {
        if (bgmSource == null)
            yield break;

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        bgmSource.volume = to;
        bgmVolumeRoutine = null;
    }

    private void StartGlintLoop()
    {
        if (glintRoutine == null && glintGroup != null)
            glintRoutine = StartCoroutine(PlayGlintLoop());
    }

    private IEnumerator PlayGlintLoop()
    {
        SetGroup(glintGroup, 0.12f, false);
        while (isActiveAndEnabled)
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(2.6f, 4.8f));
            yield return FadeGroup(glintGroup, 0.12f, 0.72f, 0.12f);
            yield return FadeGroup(glintGroup, 0.72f, 0.12f, 0.32f);
        }
        glintRoutine = null;
    }

    private static bool WasAnyInputPressed()
    {
        return Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
    }

    private static CanvasGroup CreateArtworkLayer(string name, RectTransform parent, string resourcePath, out RectTransform rect)
    {
        rect = CreateRoot(name, parent);
        Stretch(rect);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = Resources.Load<Sprite>(resourcePath);
        image.preserveAspect = true;
        image.raycastTarget = false;
        return rect.gameObject.AddComponent<CanvasGroup>();
    }

    private static CanvasGroup CreatePrompt(RectTransform parent)
    {
        var root = CreateRoot("PressAnyButton", parent);
        SetAnchoredRect(root, new Vector2(0.5f, 0f), new Vector2(0f, 88f), new Vector2(620f, 70f));
        var text = root.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = "PRESS ANY BUTTON";
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = 30f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.08f, 0.12f, 0.25f, 1f);
        text.raycastTarget = false;
        return root.gameObject.AddComponent<CanvasGroup>();
    }

    private CanvasGroup CreateMenu(RectTransform parent, out CanvasGroup[] itemGroups)
    {
        var root = CreateRoot("Menu", parent);
        SetAnchoredRect(root, new Vector2(0f, 0f), new Vector2(283f, 259f), new Vector2(410f, 310f));
        var group = root.gameObject.AddComponent<CanvasGroup>();
        var panel = root.gameObject.AddComponent<Image>();
        panel.color = new Color(1f, 1f, 1f, 0f);
        panel.raycastTarget = false;

        newGameButton = CreateMenuButton("NewGame", root, "NEW GAME", 0, true);
        continueButton = CreateMenuButton("Continue", root, "CONTINUE", 1, false);
        optionsButton = CreateMenuButton("Options", root, "OPTIONS", 2, false);
        quitButton = CreateMenuButton("Quit", root, "QUIT", 3, true);
        itemGroups = new[]
        {
            newGameButton.GetComponent<CanvasGroup>(), continueButton.GetComponent<CanvasGroup>(),
            optionsButton.GetComponent<CanvasGroup>(), quitButton.GetComponent<CanvasGroup>()
        };
        return group;
    }

    private static Button CreateMenuButton(string name, RectTransform parent, string label, int index, bool interactable)
    {
        var root = CreateRoot(name, parent);
        SetAnchoredRect(root, new Vector2(0f, 1f), new Vector2(205f, -48f - index * 68f), new Vector2(374f, 54f));
        var image = root.gameObject.AddComponent<Image>();
        image.color = Color.white;
        var button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.interactable = interactable;
        var colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0f);
        colors.highlightedColor = new Color(0.20f, 0.42f, 0.66f, 0.14f);
        colors.selectedColor = new Color(0.20f, 0.42f, 0.66f, 0.14f);
        colors.pressedColor = new Color(0.92f, 0.48f, 0.27f, 0.18f);
        colors.disabledColor = new Color(1f, 1f, 1f, 0f);
        button.colors = colors;

        var textRoot = CreateRoot("Label", root);
        Stretch(textRoot, new Vector2(18f, 0f), new Vector2(-18f, 0f));
        var text = textRoot.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = 23f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.color = interactable
            ? new Color(0.05f, 0.12f, 0.24f, 1f)
            : new Color(0.36f, 0.43f, 0.50f, 0.55f);
        text.raycastTarget = false;
        root.gameObject.AddComponent<CanvasGroup>();
        return button;
    }

    private static void EnsureEventSystem()
    {
        var eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemRoot = new GameObject("TitleEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem = eventSystemRoot.GetComponent<EventSystem>();
        }
        else if (eventSystem.GetComponent<BaseInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
    }

    private static void RemoveDuplicateEventSystems()
    {
        var systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (systems.Length <= 1)
            return;

        // Naninovel creates its own event system after the title scene starts.
        // Once that exists, discard our runtime fallback and let Naninovel own input.
        EventSystem titleFallback = null;
        foreach (var system in systems)
        {
            if (system != null && system.gameObject.name == "TitleEventSystem")
            {
                titleFallback = system;
                break;
            }
        }
        if (titleFallback != null)
        {
            titleFallback.enabled = false;
            Destroy(titleFallback.gameObject);
            return;
        }

        var keep = EventSystem.current != null ? EventSystem.current : systems[0];
        foreach (var system in systems)
        {
            if (system == null || system == keep)
                continue;

            system.enabled = false;
            Destroy(system.gameObject);
        }
    }

    private static RectTransform CreateRoot(string name, Transform parent)
    {
        var root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(parent, false);
        return root.GetComponent<RectTransform>();
    }

    private static CanvasGroup FindGroup(Transform root, string name)
    {
        var child = root.Find(name);
        return child != null ? child.GetComponent<CanvasGroup>() : null;
    }

    private static Button FindButton(Transform root, string name)
    {
        var child = root.Find("Menu/" + name);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private static void Stretch(RectTransform rect, Vector2? minOffset = null, Vector2? maxOffset = null)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = minOffset ?? Vector2.zero;
        rect.offsetMax = maxOffset ?? Vector2.zero;
    }

    private static void SetAnchoredRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetGroup(CanvasGroup group, float alpha, bool interactable)
    {
        if (group == null) return;
        group.alpha = alpha;
        group.interactable = interactable;
        group.blocksRaycasts = interactable;
    }

    private static void SetGraphicAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        var color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private static IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        group.alpha = to;
    }

    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}
