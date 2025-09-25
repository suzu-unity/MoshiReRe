using System.Collections;
using System.Reflection;
using UnityEngine;
using TMPro;

#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

[DisallowMultipleComponent]
public class AdviceBubble : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform bubbleRoot;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Optional (Text Animator for Unity)")]
    [Tooltip("Febucci の TextAnimatorPlayer をここに割り当てると本物のタイプライターを使います。未割り当てなら簡易タイプライターにフォールバックします。")]
    [SerializeField] private MonoBehaviour textAnimatorPlayer; // 例: Febucci.UI.TextAnimatorPlayer

    [Header("Timings")]
    [SerializeField] private float fadeIn = 0.15f;
    [SerializeField] private float fadeOut = 0.15f;
    [SerializeField] private float popScale = 0.12f;   // 0で無効
    [SerializeField] private float defaultDelay = 3f;  // autoHide のデフォルト秒

    [Header("Fallback Typewriter (no TextAnimator)")]
    [SerializeField, Tooltip("TextAnimator が無い場合の1文字あたり秒")] private float charInterval = 0.03f;

    private Coroutine autoHideCo;
    private Coroutine fallbackTypeCo;

#if DOTWEEN_ENABLED
    private Tween fadeTween;
    private Tween scaleTween;
#endif

    void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!bubbleRoot) bubbleRoot = transform as RectTransform;
        if (!text) text = GetComponentInChildren<TextMeshProUGUI>(true);

        if (!text)
            Debug.LogError("[AdviceBubble] TextMeshProUGUI の参照が見つかりません。Inspector の text フィールドに割り当ててください。", this);

        if (text && text.font == null)
            Debug.LogError("[AdviceBubble] TextMeshProUGUI の Font Asset が未設定です。TMP フォントを割り当ててください。", text);

        if (canvasGroup) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    // ===== Public API =====================================================================

    public void Show(string message) => Show(message, true, defaultDelay);
    public void Show(string message, bool autoHide) => Show(message, autoHide, defaultDelay);

    /// <summary>
    /// 表示（autoHide=trueで delay秒後に自動非表示）
    /// </summary>
    public void Show(string message, bool autoHide, float delay)
    {
        // 先行動作の完全停止
        StopAutoHide();
        KillTweens();
        StopFallbackTypewriter();
        ResetTypewriter(); // TextAnimator 側の再生も停止

        // テキストセット（TextAnimator を使う場合は空で開始し、ShowTextに委ねる）
        if (HasTextAnimator())
        {
            if (text) text.text = string.Empty;
        }
        else
        {
            if (text)
            {
                text.text = message;
                text.maxVisibleCharacters = 0; // フォールバック用に0から始める
            }
        }

        // 表示準備
        gameObject.SetActive(true);
        if (canvasGroup) canvasGroup.alpha = 0f;

#if DOTWEEN_ENABLED
        if (fadeIn > 0f && canvasGroup)
            fadeTween = canvasGroup.DOFade(1f, fadeIn).SetUpdate(true);

        if (popScale > 0f && bubbleRoot)
        {
            bubbleRoot.localScale = Vector3.one * 0.95f;
            scaleTween = bubbleRoot
                .DOScale(1f, popScale)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
#else
        if (canvasGroup) canvasGroup.alpha = 1f;
        // DOTween 未使用時の未使用警告回避
        _ = fadeIn; _ = fadeOut; _ = popScale;
#endif

        // タイプライター再生
        PlayTypewriter(message);

        // オート消し（開始タイミングは「表示開始基準」）
        if (autoHide) autoHideCo = StartCoroutine(AutoHide(delay));
    }

    public void Hide()
    {
        StopAutoHide();
        KillTweens();
        StopFallbackTypewriter();
        ResetTypewriter(); // TextAnimator 側も停止

#if DOTWEEN_ENABLED
        if (fadeOut > 0f && canvasGroup)
        {
            fadeTween = canvasGroup
                .DOFade(0f, fadeOut)
                .SetUpdate(true)
                .OnComplete(() => SafeDeactivate());
        }
        else
        {
            if (canvasGroup) canvasGroup.alpha = 0f;
            SafeDeactivate();
        }
#else
        if (canvasGroup) canvasGroup.alpha = 0f;
        SafeDeactivate();
#endif
    }

    // ===== Internals ======================================================================

    private IEnumerator AutoHide(float delay)
    {
        if (delay <= 0f) { Hide(); yield break; }
        yield return new WaitForSeconds(delay);
        Hide();
    }

    private void StopAutoHide()
    {
        if (autoHideCo != null)
        {
            StopCoroutine(autoHideCo);
            autoHideCo = null;
        }
    }

    private void SafeDeactivate()
    {
        gameObject.SetActive(false);
    }

    private void KillTweens()
    {
#if DOTWEEN_ENABLED
        if (fadeTween != null && fadeTween.IsActive()) fadeTween.Kill(true);
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill(true);
        fadeTween = null;
        scaleTween = null;
#endif
    }

    // ── Text Animator（Febucci）連携 & フォールバック ─────────────────────────────

    private bool HasTextAnimator() => textAnimatorPlayer != null;

    private void ResetTypewriter()
    {
        if (!HasTextAnimator()) return;

        // 可能なら SkipTypewriter / StopShowingText を呼ぶ（リフレクションで安全に）
        var tp = textAnimatorPlayer;
        var t = tp.GetType();

        var skip = t.GetMethod("SkipTypewriter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        skip?.Invoke(tp, null);

        var stop = t.GetMethod("StopShowingText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        stop?.Invoke(tp, null);
    }

    private void PlayTypewriter(string message)
    {
        if (HasTextAnimator())
        {
            // Febucci TextAnimatorPlayer に委譲
            var tp = textAnimatorPlayer;
            var t = tp.GetType();

            var show = t.GetMethod("ShowText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
            if (show != null)
            {
                show.Invoke(tp, new object[] { message });
            }
            else
            {
                // ShowText が見つからない場合はフォールバック
                StartFallbackTypewriter(message);
            }
        }
        else
        {
            // フォールバックのタイプライター
            StartFallbackTypewriter(message);
        }
    }

    private void StartFallbackTypewriter(string message)
    {
        if (!text) return;
        StopFallbackTypewriter();
        fallbackTypeCo = StartCoroutine(FallbackTypewriterCo(message));
    }

    private void StopFallbackTypewriter()
    {
        if (fallbackTypeCo != null)
        {
            StopCoroutine(fallbackTypeCo);
            fallbackTypeCo = null;
        }
    }

    private IEnumerator FallbackTypewriterCo(string message)
    {
        // text.text は Show() で設定済み。maxVisibleCharacters を増やしていく
        int total = text.textInfo.characterCount;
        // textInfo は次のフレームで更新されることがあるので初期化待ち
        if (total == 0)
        {
            yield return null;
            total = text.textInfo.characterCount;
        }

        int visible = 0;
        while (visible < total)
        {
            visible++;
            text.maxVisibleCharacters = visible;
            yield return new WaitForSeconds(charInterval);
        }
        text.maxVisibleCharacters = total;
    }
}
