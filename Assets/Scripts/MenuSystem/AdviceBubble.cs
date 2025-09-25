using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AdviceBubble : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Transform scaleTarget;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Anim")]
    [SerializeField] private float showDuration = 0.18f;
    [SerializeField] private float hideDuration = 0.12f;
    [SerializeField] private float targetScale = 1.0f;
    [SerializeField] private float fromScale = 0.85f;
    [SerializeField] private Ease easeIn = Ease.OutSine;
    [SerializeField] private Ease easeOut = Ease.InSine;

    [Header("Typewriter")]
    [SerializeField] private bool useTypewriter = false;
    [SerializeField] private float charInterval = 0.03f;

    Sequence _showSeq;
    Sequence _hideSeq;
    Coroutine _typeRoutine;
    bool _initialized;

    void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect        = GetComponent<RectTransform>();
        scaleTarget = rect ? (Transform)rect : transform;
        label       = GetComponentInChildren<TextMeshProUGUI>(true);
        background  = GetComponentInChildren<Image>(true);
    }

    void Awake()
    {
        InitIfNeeded();
        HideImmediate();
    }

    void OnDisable()  => KillSequences();
    void OnDestroy()  => KillSequences();

    void InitIfNeeded()
    {
        if (_initialized) return;
        _initialized = true;

        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (!rect) rect = GetComponent<RectTransform>();
        if (!scaleTarget) scaleTarget = rect ? (Transform)rect : transform;
        if (!label) label = GetComponentInChildren<TextMeshProUGUI>(true);
        if (!background) background = GetComponentInChildren<Image>(true);

        if (background && background.type != Image.Type.Sliced)
            background.type = Image.Type.Sliced;

        KillSequences();

        _showSeq = DOTween.Sequence().SetAutoKill(false).Pause();
        _showSeq.OnPlay(() => gameObject.SetActive(true));
        _showSeq.Append(canvasGroup.DOFade(1f, showDuration))
                .Join(scaleTarget.DOScale(targetScale, showDuration)
                                 .From(fromScale)
                                 .SetEase(easeIn));

        _hideSeq = DOTween.Sequence().SetAutoKill(false).Pause();
        _hideSeq.OnComplete(() => gameObject.SetActive(false));
        _hideSeq.Append(scaleTarget.DOScale(fromScale, hideDuration).SetEase(easeOut))
                .Join(canvasGroup.DOFade(0f, hideDuration));
    }

    void KillSequences()
    {
        if (_showSeq != null) { _showSeq.Kill(); _showSeq = null; }
        if (_hideSeq != null) { _hideSeq.Kill(); _hideSeq = null; }
    }

    void HideImmediate()
    {
        if (!canvasGroup) return;
        canvasGroup.alpha = 0f;
        if (!scaleTarget) scaleTarget = rect ? (Transform)rect : transform;
        if (scaleTarget) scaleTarget.localScale = Vector3.one * fromScale;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 吹き出しを表示
    /// </summary>
    public void Show(string message, bool autoHide = false, float autoHideDelay = 0f, bool? forceTypewriter = null)
    {
        InitIfNeeded();
        StopTypewriterIfAny();

        background.enabled = true;
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;

        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode     = TextOverflowModes.Overflow;

        bool doTypewriter = forceTypewriter ?? useTypewriter;

        if (doTypewriter)
        {
            _typeRoutine = StartCoroutine(TypewriterRoutine(message ?? string.Empty, autoHide, autoHideDelay));
        }
        else
        {
            SetTextImmediate(message ?? string.Empty);
            RebuildLayoutNow();
            PlayShow();
            ScheduleAutoHideIfNeeded(autoHide, autoHideDelay);
        }
    }

    public void Hide()
    {
        InitIfNeeded();
        StopTypewriterIfAny();
        if (_showSeq != null && _showSeq.IsActive()) _showSeq.Pause();
        _hideSeq?.Restart();
    }

    // ==== 内部処理 ====

    void PlayShow()
    {
        if (_hideSeq != null && _hideSeq.IsActive()) _hideSeq.Pause();
        _showSeq?.Restart();
    }

    void ScheduleAutoHideIfNeeded(bool autoHide, float autoHideDelay)
    {
        if (!autoHide) return;
        DOVirtual.DelayedCall(autoHideDelay <= 0f ? 1.5f : autoHideDelay, Hide).SetTarget(this);
    }

    void SetTextImmediate(string s)
    {
        if (!label) return;
        label.text = s;
        label.ForceMeshUpdate();
    }

    IEnumerator TypewriterRoutine(string s, bool autoHide, float autoHideDelay)
    {
        if (!label)
        {
            PlayShow();
            ScheduleAutoHideIfNeeded(autoHide, autoHideDelay);
            yield break;
        }

        label.text = s;                   // 全文セット
        label.maxVisibleCharacters = 0;   // 非表示状態で開始
        RebuildLayoutNow();
        PlayShow();

        for (int i = 0; i <= s.Length; i++)
        {
            label.maxVisibleCharacters = i;
            label.ForceMeshUpdate();
            RebuildLayoutNow();
            yield return new WaitForSeconds(charInterval);
        }

        RebuildLayoutNow();
        ScheduleAutoHideIfNeeded(autoHide, autoHideDelay);
        _typeRoutine = null;
    }

    void RebuildLayoutNow()
    {
        Canvas.ForceUpdateCanvases();
        if (rect) LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        if (label) LayoutRebuilder.ForceRebuildLayoutImmediate(label.rectTransform);
    }

    void StopTypewriterIfAny()
    {
        if (_typeRoutine != null)
        {
            StopCoroutine(_typeRoutine);
            _typeRoutine = null;
        }
    }
}
