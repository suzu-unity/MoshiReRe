using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class NosebleedFx : MonoBehaviour
{
    public float fallDistance = 80f;
    public float fallTime = 0.6f;
    public float fadeTime = 0.35f;
    [Range(0f,1f)] public float fadeStartRatio = 0.7f;

    RectTransform rt;
    CanvasGroup cg;
    Sequence seq; // ← フィールドで保持

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // 念のため前面に
        transform.SetAsLastSibling();

        if (!rt || !cg) return; // 早期リターン（保険）
        cg.alpha = 1f;

        var start = rt.anchoredPosition;
        var end   = start + new Vector2(0f, -fallDistance);

        // 既存のTweenが残っていたらKill
        KillTweens();

        seq = DOTween.Sequence();

        // 落下
        seq.Append(rt.DOAnchorPos(end, fallTime).SetEase(Ease.InQuad));

        // 終盤でフェード
        seq.Insert(fallTime * fadeStartRatio, cg.DOFade(0f, fadeTime));

        // 破棄時に自動Kill（これが超重要）
        seq.SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        // 完了時に後片付け
        seq.OnKill(() => seq = null);
    }

    void OnDisable() => KillTweens();
    void OnDestroy() => KillTweens();

    void KillTweens()
    {
        if (seq != null && seq.IsActive()) seq.Kill(false); // 完了扱いにしないKill
        // ターゲット別Kill（万一の取りこぼし対策）
        if (rt) DOTween.Kill(rt, complete: false);
        if (cg) DOTween.Kill(cg, complete: false);
    }
}
