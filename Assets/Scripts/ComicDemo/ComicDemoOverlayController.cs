using Naninovel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComicDemoOverlayController : MonoBehaviour
{
    public static ComicDemoOverlayController Instance { get; private set; }

    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private Image pageImage;
    private Image leftDim;
    private Image rightDim;
    private Button nextButton;
    private Button prevButton;
    private TMP_Text pageLabel;

    private string currentScriptPath = string.Empty;
    private string nextLabel = string.Empty;
    private string prevLabel = string.Empty;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildUi();
        HideInstant();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ShowPage(string spritePath, string focusSide, string nextTargetLabel, string prevTargetLabel, string scriptPath)
    {
        var sprite = Resources.Load<Sprite>(spritePath);
        if (!sprite)
        {
            Debug.LogError($"[ComicDemoOverlay] Sprite not found at Resources/{spritePath}");
            return;
        }

        currentScriptPath = scriptPath ?? string.Empty;
        nextLabel = nextTargetLabel ?? string.Empty;
        prevLabel = prevTargetLabel ?? string.Empty;

        pageImage.sprite = sprite;
        pageImage.preserveAspect = true;
        pageLabel.text = sprite.name;

        var focusLeft = string.Equals(focusSide, "left", System.StringComparison.OrdinalIgnoreCase);
        var focusRight = string.Equals(focusSide, "right", System.StringComparison.OrdinalIgnoreCase);

        leftDim.color = new Color(0f, 0f, 0f, focusLeft ? 0.1f : 0.62f);
        rightDim.color = new Color(0f, 0f, 0f, focusRight ? 0.1f : 0.62f);

        nextButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(nextLabel));
        prevButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(prevLabel));

        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public void Hide()
    {
        HideInstant();
    }

    private async void HandleNext()
    {
        if (string.IsNullOrWhiteSpace(currentScriptPath) || string.IsNullOrWhiteSpace(nextLabel) || !Engine.Initialized) return;
        await Engine.GetService<IScriptPlayer>().LoadAndPlayAtLabel(currentScriptPath, nextLabel);
    }

    private async void HandlePrev()
    {
        if (string.IsNullOrWhiteSpace(currentScriptPath) || string.IsNullOrWhiteSpace(prevLabel) || !Engine.Initialized) return;
        await Engine.GetService<IScriptPlayer>().LoadAndPlayAtLabel(currentScriptPath, prevLabel);
    }

    private void HideInstant()
    {
        if (!canvasGroup) return;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);
    }

    private void BuildUi()
    {
        var canvasGo = new GameObject("ComicDemoCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasGo.transform.SetParent(transform, false);

        rootCanvas = canvasGo.GetComponent<Canvas>();
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvas.overrideSorting = true;
        rootCanvas.sortingOrder = 500;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGroup = canvasGo.GetComponent<CanvasGroup>();

        var root = canvasGo.GetComponent<RectTransform>();

        var frame = CreateImage("Frame", root, new Color(0.05f, 0.05f, 0.06f, 0.98f));
        Stretch(frame.rectTransform, new Vector2(70f, 60f), new Vector2(-70f, -90f));

        pageImage = CreateImage("PageImage", frame.rectTransform, Color.white);
        Stretch(pageImage.rectTransform, new Vector2(36f, 36f), new Vector2(-36f, -36f));

        leftDim = CreateImage("LeftDim", pageImage.rectTransform, new Color(0f, 0f, 0f, 0.62f));
        leftDim.rectTransform.anchorMin = new Vector2(0f, 0f);
        leftDim.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        leftDim.rectTransform.offsetMin = Vector2.zero;
        leftDim.rectTransform.offsetMax = Vector2.zero;

        rightDim = CreateImage("RightDim", pageImage.rectTransform, new Color(0f, 0f, 0f, 0.62f));
        rightDim.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rightDim.rectTransform.anchorMax = new Vector2(1f, 1f);
        rightDim.rectTransform.offsetMin = Vector2.zero;
        rightDim.rectTransform.offsetMax = Vector2.zero;

        var divider = CreateImage("Divider", pageImage.rectTransform, new Color(0f, 0f, 0f, 0.9f));
        divider.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        divider.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        divider.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        divider.rectTransform.sizeDelta = new Vector2(10f, 0f);
        divider.rectTransform.anchoredPosition = Vector2.zero;

        pageLabel = CreateText("PageLabel", root, "Comic Demo", 28);
        pageLabel.alignment = TextAlignmentOptions.BottomLeft;
        pageLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
        pageLabel.rectTransform.anchorMax = new Vector2(0f, 0f);
        pageLabel.rectTransform.pivot = new Vector2(0f, 0f);
        pageLabel.rectTransform.anchoredPosition = new Vector2(84f, 24f);
        pageLabel.rectTransform.sizeDelta = new Vector2(600f, 40f);

        prevButton = CreateNavButton("PrevButton", root, "< Prev");
        prevButton.onClick.AddListener(HandlePrev);
        SetNavButtonRect(prevButton.GetComponent<RectTransform>(), new Vector2(84f, 84f), new Vector2(160f, 64f), false);

        nextButton = CreateNavButton("NextButton", root, "Next >");
        nextButton.onClick.AddListener(HandleNext);
        SetNavButtonRect(nextButton.GetComponent<RectTransform>(), new Vector2(-84f, 84f), new Vector2(160f, 64f), true);
    }

    private static Image CreateImage(string name, RectTransform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(string name, RectTransform parent, string value, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.color = Color.white;
        text.enableWordWrapping = false;
        text.font = TMP_Settings.defaultFontAsset;
        return text;
    }

    private static Button CreateNavButton(string name, RectTransform parent, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        image.color = new Color(0.14f, 0.14f, 0.16f, 0.92f);

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        button.colors = colors;

        var text = CreateText("Label", go.GetComponent<RectTransform>(), label, 28);
        text.alignment = TextAlignmentOptions.Center;
        Stretch(text.rectTransform, Vector2.zero, Vector2.zero);
        return button;
    }

    private static void SetNavButtonRect(RectTransform rect, Vector2 anchoredPosition, Vector2 sizeDelta, bool right)
    {
        rect.anchorMin = right ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
        rect.anchorMax = right ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
        rect.pivot = right ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
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
}
