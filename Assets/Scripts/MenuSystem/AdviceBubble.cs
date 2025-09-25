using System.Collections;                 // ← これが必要（IEnumerator）
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;                     // LayoutRebuilder, Image

[DisallowMultipleComponent]
public class AdviceBubble : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rect;      // バブル本体(ReRetooltip)のRectTransform
    [SerializeField] private Transform scaleTarget;   // rect が無ければ Transform で代用
    [SerializeField] private Image background;        // 9-slice スプライト
    [SerializeField] private TextMeshProUGUI label;   // 吹き出しテキスト

    [Header("Anim")]
    [SerializeField] private float showDuration = 0.18f;
    [SerializeField] private float hideDuration = 0.12f;
    [SerializeField] private float targetScale = 1.00f;
    [SerializeField] private float fromScale   = 0.85f;
    [SerializeField] private Ease  easeIn      = Ease.OutSine;
    [SerializeField] private Ease  easeOut     = Ease.InSine;

    [Header("Typewriter")]
    [SerializeField] private bool  useTypewriter = false; // 既定でタイプ表示にしたい場合は true
    [SerializeField] private float charInterval  = 0.03f; // 1文字の表示間隔(秒)

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

        if (!canvasGroup)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        if (!rect) rect = GetComponent<RectTransform>();
        if (!scaleTarget) scaleTarget = rect ? (Transform)rect : transform;
        if (!label) label = GetComponentInChildren<TextMeshProUGUI>(true);
        if (!background) background = GetComponentInChildren<Image>(true);

        // 9-slice 推奨
        if (background && background.type != Image.Type.Sliced)
            background.type = Image.Type.Sliced;

        // シーケンス作り直し
        KillSequences();

        _showSeq = DOTween.Sequence().SetAutoKill(false).Pause();
        _showSeq.OnPlay(() =>
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
        });
        _showSeq.Append(canvasGroup.DOFade(1f, showDuration))
                .Join(scaleTarget.DOScale(targetScale, showDuration)
                                 .From(fromScale)
                                 .SetEase(easeIn));

        _hideSeq = DOTween.Sequence().SetAutoKill(false).Pause();
        _hideSeq.OnComplete(() => { gameObject.SetActive(false); });
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
/// autoHide = true のとき autoHideDelay 秒後に消える
/// typewriter = true ならタイプライター風表示
/// （引数を省略した場合は Inspector の useTypewriter を参照）
/// </summary>
public void Show(string message, bool autoHide = false, float autoHideDelay = 0f, bool? typewriter = null)
{
    InitIfNeeded();
    StopTypewriterIfAny();

    if (background) background.enabled = true;
    if (!gameObject.activeSelf) gameObject.SetActive(true);
    canvasGroup.alpha = 1f;

    if (label)
    {
        label.enableWordWrapping = true;
        label.textWrappingMode   = TextWrappingModes.Normal;
        label.overflowMode       = TextOverflowModes.Overflow;
    }

    // 引数優先。指定がなければ Inspector の useTypewriter を使う
    bool doTypewriter = typewriter ?? useTypewriter;

    if (doTypewriter)
    {
        _typeRoutine = StartCoroutine(TypewriterRoutine(message ?? string.Empty, autoHide, autoHideDelay, charInterval));
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

    // ========= ここから内部処理 =========

    void PlayShow()
    {
        if (_hideSeq != null && _hideSeq.IsActive()) _hideSeq.Pause();
        _showSeq?.Restart();
    }

    void ScheduleAutoHideIfNeeded(bool autoHide, float autoHideDelay)
    {
        if (!autoHide) return;
        DOVirtual.DelayedCall(autoHideDelay <= 0f ? 1.5f : autoHideDelay, Hide)
                 .SetTarget(this);
    }

    void SetTextImmediate(string s)
    {
        if (!label) return;
        label.text = s;
        label.ForceMeshUpdate();
    }

    IEnumerator TypewriterRoutine(string s, bool autoHide, float autoHideDelay, float interval)
    {
        if (!label)
        {
            PlayShow();
            ScheduleAutoHideIfNeeded(autoHide, autoHideDelay);
            yield break;
        }

        label.text = string.Empty;
        label.ForceMeshUpdate();
        RebuildLayoutNow();
        PlayShow(); // 先に出しとく（文字が出ながら伸びるようにする）

        for (int i = 0; i < s.Length; i++)
        {
            label.text += s[i];
            label.ForceMeshUpdate();

            // 1文字ごとに再計算：バブルが文字量に応じて追従
            RebuildLayoutNow();

            if (interval > 0f)
                yield return new WaitForSeconds(interval);
            else
                yield return null;
        }

        // 全文出し切り後の最終計算
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