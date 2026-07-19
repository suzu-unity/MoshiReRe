using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class ReReButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static ReReButtonController Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Image markerImage;
    [SerializeField] private AdviceBubble adviceBubble;
    [SerializeField] private RectTransform screenRect;
    [SerializeField] private Sprite reactionSprite;
    [SerializeField] private Sprite notificationSprite;

    [Header("Idle Animation")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private float idleFps = 6f;

    [Header("Hover")]
    [SerializeField] private Sprite hoverSprite;

    [Header("Advice")]
    [TextArea]
    [SerializeField] private string initialAdvice;
    [SerializeField] private bool clickAutoHide = false;
    [SerializeField] private float clickAutoHideDelay = 3f;

    [Header("Marker Overlay")]
    [SerializeField] private Sprite markerA;
    [SerializeField] private Sprite markerB;
    [SerializeField] private Sprite markerC;

    private string currentAdvice = string.Empty;
    private bool isHovering;
    private int frameIndex;
    private float frameTimer;
    private Coroutine reactionRoutine;
    private Vector3 screenBaseScale = Vector3.one;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (!button) button = GetComponent<Button>();
        if (!buttonImage) buttonImage = GetComponent<Image>();
        if (!screenRect && buttonImage) screenRect = buttonImage.rectTransform;
        if (screenRect) screenBaseScale = screenRect.localScale;

        if (button)
        {
            button.onClick.RemoveListener(OnClicked);
            button.onClick.AddListener(OnClicked);
        }

        currentAdvice = initialAdvice ?? string.Empty;
    }

    private void OnEnable()
    {
        ApplyMarker("", true);
        ApplyCurrentVisual();
    }

    private void Update()
    {
        if (isHovering) return;
        if (idleFrames == null || idleFrames.Length == 0) return;
        if (idleFps <= 0f) return;

        frameTimer += Time.unscaledDeltaTime;
        float interval = 1f / idleFps;
        if (frameTimer < interval) return;

        frameTimer -= interval;
        frameIndex = (frameIndex + 1) % idleFrames.Length;
        if (buttonImage) buttonImage.sprite = idleFrames[frameIndex];
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (button) button.onClick.RemoveListener(OnClicked);
        if (reactionRoutine != null) StopCoroutine(reactionRoutine);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (buttonImage && hoverSprite) buttonImage.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        ApplyCurrentVisual();
    }

    public void SetAdvice(string message, string marker)
    {
        currentAdvice = message ?? string.Empty;
        ApplyMarker(marker, false);
        if (!string.IsNullOrWhiteSpace(marker)) PlayReaction();
    }

    public void SetHint(string message)
    {
        SetAdvice(message, "notification");
    }

    public void SetNotification(bool visible)
    {
        ApplyMarker(visible ? "notification" : string.Empty, !visible);
        if (visible) PlayReaction();
    }

    private void OnClicked()
    {
        if (!adviceBubble) return;
        if (string.IsNullOrEmpty(currentAdvice)) return;

        adviceBubble.Show(currentAdvice, clickAutoHide, clickAutoHideDelay);
    }

    private void ApplyCurrentVisual()
    {
        if (!buttonImage) return;
        if (idleFrames == null || idleFrames.Length == 0) return;

        if (frameIndex < 0 || frameIndex >= idleFrames.Length)
            frameIndex = 0;

        buttonImage.sprite = idleFrames[frameIndex];
    }

    private void ApplyMarker(string marker, bool forceNone)
    {
        if (!markerImage) return;

        Sprite sprite = null;
        if (!forceNone)
        {
            switch ((marker ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "a":
                    sprite = markerA;
                    break;
                case "b":
                    sprite = markerB;
                    break;
                case "c":
                    sprite = markerC;
                    break;
            }
        }

        bool notification = !forceNone && !string.IsNullOrWhiteSpace(marker);
        markerImage.sprite = sprite ? sprite : notificationSprite;
        markerImage.enabled = notification;

        var markerLabel = markerImage.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        if (markerLabel) markerLabel.enabled = sprite == null && notification;
    }

    private void PlayReaction()
    {
        if (!screenRect) return;
        if (reactionRoutine != null) StopCoroutine(reactionRoutine);
        reactionRoutine = StartCoroutine(PlayReactionRoutine());
    }

    private IEnumerator PlayReactionRoutine()
    {
        Sprite previous = buttonImage ? buttonImage.sprite : null;
        if (buttonImage && reactionSprite) buttonImage.sprite = reactionSprite;

        const float popDuration = 0.12f;
        float t = 0f;
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            screenRect.localScale = Vector3.Lerp(screenBaseScale, screenBaseScale * 1.08f, t / popDuration);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.35f);

        t = 0f;
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            screenRect.localScale = Vector3.Lerp(screenBaseScale * 1.08f, screenBaseScale, t / popDuration);
            yield return null;
        }

        if (buttonImage && reactionSprite) buttonImage.sprite = previous;
        ApplyCurrentVisual();
        reactionRoutine = null;
    }
}
