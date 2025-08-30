using UnityEngine;
using TMPro;
using DG.Tweening;

public class AdviceBubble : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform rect;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text label;

    [Header("Timings")]
    [SerializeField] private float popScale    = 1.0f;
    [SerializeField] private float popIn       = 0.25f;
    [SerializeField] private float holdSeconds = 5.0f; // ← ここで表示時間を長めに調整
    [SerializeField] private float fadeOut     = 0.20f;
    private Tween _fadeTween, _scaleTween, _delayTween;

    void Reset()  { CacheRefs(); SetupHidden(); }
    void Awake()  { CacheRefs(); SetupHidden(); }

    private void CacheRefs()
    {
        if (!rect)  rect  = GetComponent<RectTransform>();
        if (!group) group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (!label) label = GetComponentInChildren<TMP_Text>(true);
    }

    private void SetupHidden()
    {
        KillTweens();
        if (!group) return;
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        if (rect) rect.localScale = Vector3.one * 0.8f;
    }

    private void KillTweens()
    {
        _fadeTween?.Kill(); _fadeTween = null;
        _scaleTween?.Kill(); _scaleTween = null;
        _delayTween?.Kill(); _delayTween = null;
    }

    /// <summary>
    /// アドバイス表示。autoHideOverride=true なら一定時間で自動クローズ、
    /// false なら出しっぱ。null のときはデフォルト（holdSeconds）。
    /// </summary>
    public void Show(string msg, bool? autoHideOverride = null)
    {
        CacheRefs();
        KillTweens();

        if (label) label.text = msg;

        group.alpha = 1f;
        group.interactable = false;
        group.blocksRaycasts = false;

        if (rect) rect.localScale = Vector3.one * 0.8f;
        _scaleTween = rect?.DOScale(popScale, popIn).SetUpdate(true);

        bool autoHide = autoHideOverride ?? true;
        if (autoHide)
            _delayTween = DOVirtual.DelayedCall(holdSeconds, Hide).SetUpdate(true);
    }

    public void Hide()
    {
        CacheRefs();
        KillTweens();
        _fadeTween = group.DOFade(0f, fadeOut).SetUpdate(true)
            .OnComplete(SetupHidden);
    }

    /// <summary>内容だけ差し替えて軽くポップ（消さない）。</summary>
    public void UpdateText(string msg)
    {
        CacheRefs();
        if (label) label.text = msg;
        if (rect)
        {
            rect.localScale = Vector3.one * 0.95f;
            _scaleTween?.Kill();
            _scaleTween = rect.DOScale(popScale, 0.12f).SetUpdate(true);
        }
    }
}