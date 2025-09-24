using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AdviceBubble : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rect;      // あれば使う
    [SerializeField] private Transform scaleTarget;   // rect が無ければ Transform で代用
    [SerializeField] private Image background;        // 9-slice スプライト
    [SerializeField] private TextMeshProUGUI label;   // 吹き出しテキスト

    [Header("Anim")]
    [SerializeField] private float showDuration = 0.18f;
    [SerializeField] private float hideDuration = 0.12f;
    [SerializeField] private float targetScale = 1.00f; // 表示後の最終スケール
    [SerializeField] private float fromScale   = 0.85f; // 表示開始スケール
    [SerializeField] private Ease  easeIn      = Ease.OutSine;
    [SerializeField] private Ease  easeOut     = Ease.InSine;

    Sequence _showSeq;
    Sequence _hideSeq;
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
        HideImmediate(); // 実行開始時は非表示
    }

    void OnDisable()  => KillSequences();
    void OnDestroy()  => KillSequences();

    void InitIfNeeded()
    {
        if (_initialized) return;
        _initialized = true;

        // Refs
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

        // 既存シーケンス破棄
        KillSequences();

        // Show シーケンス（再利用）
        _showSeq = DOTween.Sequence().SetAutoKill(false).Pause();
        _showSeq.OnPlay(() => { if (!gameObject.activeSelf) gameObject.SetActive(true); });

        _showSeq.Append(canvasGroup.DOFade(1f, showDuration))
                .Join(scaleTarget.DOScale(targetScale, showDuration)
                                 .From(fromScale)
                                 .SetEase(easeIn));

        // Hide シーケンス（再利用）
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

    /// <summary>吹き出しを表示（autoHide=false で出しっぱなし）</summary>
    public void Show(string message, bool autoHide = false, float autoHideDelay = 0f)
    {
        InitIfNeeded();
            if (label) { label.enabled = true; label.text = message ?? string.Empty; }
    if (background) background.enabled = true;
    if (!gameObject.activeSelf) gameObject.SetActive(true);
    canvasGroup.alpha = 1f;
    
        if (label) label.text = message ?? string.Empty;

        // 先に逆側を止める
        if (_hideSeq != null && _hideSeq.IsActive()) _hideSeq.Pause();

        // 値を整える
        canvasGroup.alpha = 1f;
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        _showSeq?.Restart();

        if (autoHide)
        {
            DOVirtual.DelayedCall(autoHideDelay <= 0f ? 1.5f : autoHideDelay, Hide)
                     .SetTarget(this);
        }
    }

    public void Hide()
    {
        InitIfNeeded();
        if (_showSeq != null && _showSeq.IsActive()) _showSeq.Pause();
        _hideSeq?.Restart();
    }
}
