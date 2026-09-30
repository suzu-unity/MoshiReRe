using System.Collections.Generic;
using DG.Tweening;
using Naninovel;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation-only feedback for the item page: items arc into a bag slot, land with a squash,
/// the bag flashes and sparkles; removed items hop back to the list; a full bag shakes.
/// ItemMenuController owns the data and calls into this component.
/// </summary>
[DisallowMultipleComponent]
public class ItemBagStowAnimator : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField, Min(0.05f)] private float flightSeconds = 0.42f;
    [SerializeField, Min(0f), Tooltip("Minimum arc height in page units.")]
    private float arcHeight = 110f;
    [SerializeField] private Vector2 flyerSize = new Vector2(96f, 96f);

    [Header("Landing")]
    [SerializeField, Min(0.05f)] private float landingSeconds = 0.3f;
    [SerializeField, Range(0, 16)] private int sparkleCount = 8;
    [SerializeField] private Color sparkleColorA = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color sparkleColorB = new Color(1f, 0.72f, 0.9f, 1f);
    [SerializeField, Tooltip("Naninovel SFX resource played when an item lands. Empty = silent.")]
    private string landSfxPath = "SFX/チリン";
    [SerializeField, Range(0f, 1f)] private float landSfxVolume = 0.55f;

    [Header("Bag Flash")]
    [SerializeField, Tooltip("Silhouette drawn over the bag artwork and flashed white on landing.")]
    private Sprite bagSilhouette;
    [SerializeField, Tooltip("Top-left position of the bag artwork in page units.")]
    private Vector2 bagSilhouettePosition = new Vector2(914f, -436f);
    [SerializeField] private Vector2 bagSilhouetteSize = new Vector2(710f, 431f);
    [SerializeField, Range(0f, 1f)] private float bagFlashAlpha = 0.35f;

    private readonly List<GameObject> flyers = new();
    private readonly List<Image> hiddenIcons = new();
    private RectTransform page;
    private Image bagFlash;

    public bool IsAnimating => flyers.Count > 0;

    private void Awake() => page = transform as RectTransform;

    private void OnDisable()
    {
        // Menu closed or page switched mid-animation: land everything instantly.
        for (var i = 0; i < flyers.Count; i++)
            if (flyers[i]) Destroy(flyers[i]);
        flyers.Clear();
        for (var i = 0; i < hiddenIcons.Count; i++)
            if (hiddenIcons[i]) hiddenIcons[i].canvasRenderer.SetAlpha(1f);
        hiddenIcons.Clear();
        if (bagFlash) bagFlash.canvasRenderer.SetAlpha(0f);
    }

    /// <summary>Flies an item from a world position into a slot icon, which stays hidden until it lands.</summary>
    public void PlayStow(Vector3 fromWorld, Image slotIcon, Sprite sprite, Color placeholderColor, RectTransform bagArea)
    {
        if (!isActiveAndEnabled || !page || !slotIcon)
            return;

        var slot = slotIcon.transform.parent as RectTransform;
        var from = ToPageLocal(fromWorld);
        var to = ToPageLocal(slotIcon.rectTransform.TransformPoint(slotIcon.rectTransform.rect.center));
        var endScale = slotIcon.rectTransform.rect.width / Mathf.Max(1f, flyerSize.x);

        slotIcon.canvasRenderer.SetAlpha(0f);
        hiddenIcons.Add(slotIcon);

        var flyer = CreateFlyer(sprite, placeholderColor, from);
        var rect = (RectTransform)flyer.transform;
        var control = CalculateArcControl(from, to, arcHeight);
        var progress = 0f;
        var sequence = DOTween.Sequence().SetUpdate(true).SetLink(flyer);
        sequence.Append(DOTween.To(() => progress, value =>
        {
            progress = value;
            rect.anchoredPosition = QuadraticBezier(from, control, to, value);
            var lift = Mathf.Sin(value * Mathf.PI);
            var scale = Mathf.Lerp(1f, endScale, value) * (1f + 0.18f * lift);
            rect.localScale = new Vector3(scale, scale, 1f);
            rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Sign(to.x - from.x) * -18f * lift);
        }, 1f, flightSeconds).SetEase(Ease.InOutSine));
        sequence.OnComplete(() =>
        {
            flyers.Remove(flyer);
            Destroy(flyer);
            hiddenIcons.Remove(slotIcon);
            if (slotIcon) slotIcon.canvasRenderer.SetAlpha(1f);
            PlayLanding(slotIcon, slot, bagArea, to);
        });
    }

    /// <summary>Sends an item that left the bag back toward its card in the list.</summary>
    public void PlayReturn(Vector3 fromWorld, Vector3 toWorld, Sprite sprite, Color placeholderColor)
    {
        if (!isActiveAndEnabled || !page)
            return;

        var from = ToPageLocal(fromWorld);
        var to = ToPageLocal(toWorld);
        var flyer = CreateFlyer(sprite, placeholderColor, from);
        var rect = (RectTransform)flyer.transform;
        var group = flyer.GetComponent<CanvasGroup>();
        var control = CalculateArcControl(from, to, arcHeight * 0.6f);
        var progress = 0f;
        var sequence = DOTween.Sequence().SetUpdate(true).SetLink(flyer);
        sequence.Append(DOTween.To(() => progress, value =>
        {
            progress = value;
            rect.anchoredPosition = QuadraticBezier(from, control, to, value);
            var scale = Mathf.Lerp(0.85f, 0.55f, value);
            rect.localScale = new Vector3(scale, scale, 1f);
            group.alpha = 1f - Mathf.Clamp01((value - 0.6f) / 0.4f);
        }, 1f, flightSeconds * 0.85f).SetEase(Ease.OutQuad));
        sequence.OnComplete(() =>
        {
            flyers.Remove(flyer);
            Destroy(flyer);
        });
    }

    /// <summary>Shakes the bag when it cannot take another item.</summary>
    public void PlayRejected(RectTransform bagArea, Graphic statusText)
    {
        if (!isActiveAndEnabled)
            return;

        if (bagArea)
        {
            bagArea.DOKill(true);
            bagArea.DOShakeAnchorPos(0.35f, new Vector2(12f, 0f), 14, 0f, false, true)
                .SetUpdate(true).SetLink(bagArea.gameObject);
        }

        if (statusText)
        {
            var original = statusText.color;
            statusText.DOKill(true);
            statusText.color = new Color(0.93f, 0.25f, 0.42f, original.a);
            statusText.DOColor(original, 0.6f).SetDelay(0.25f).SetUpdate(true).SetLink(statusText.gameObject);
        }
    }

    private void PlayLanding(Image slotIcon, RectTransform slot, RectTransform bagArea, Vector2 at)
    {
        if (slotIcon)
        {
            var iconRect = slotIcon.rectTransform;
            iconRect.DOKill(true);
            iconRect.localScale = new Vector3(1.28f, 0.74f, 1f);
            DOTween.Sequence().SetUpdate(true).SetLink(slotIcon.gameObject)
                .Append(iconRect.DOScale(new Vector3(0.9f, 1.12f, 1f), landingSeconds * 0.4f).SetEase(Ease.OutQuad))
                .Append(iconRect.DOScale(Vector3.one, landingSeconds * 0.6f).SetEase(Ease.OutBack, 2.5f));
        }

        if (slot)
        {
            slot.DOKill(true);
            slot.localScale = Vector3.one;
            slot.DOPunchScale(new Vector3(0.14f, 0.14f, 0f), landingSeconds, 6, 0.6f)
                .SetUpdate(true).SetLink(slot.gameObject);
        }

        if (bagArea)
        {
            bagArea.DOKill(true);
            bagArea.DOPunchAnchorPos(new Vector2(0f, -7f), landingSeconds * 1.2f, 5, 0.5f)
                .SetUpdate(true).SetLink(bagArea.gameObject);
        }

        FlashBag();
        SpawnSparkles(at);
        PlayLandSfx();
    }

    private void FlashBag()
    {
        if (!bagSilhouette || bagFlashAlpha <= 0f)
            return;

        if (!bagFlash)
        {
            var go = new GameObject("BagStowFlash", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(page, false);
            // Directly above the page artwork, below cards and slots.
            rect.SetSiblingIndex(Mathf.Min(1, page.childCount - 1));
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = bagSilhouettePosition;
            rect.sizeDelta = bagSilhouetteSize;
            bagFlash = go.GetComponent<Image>();
            bagFlash.sprite = bagSilhouette;
            bagFlash.preserveAspect = true;
            bagFlash.raycastTarget = false;
            bagFlash.color = Color.white;
        }

        bagFlash.DOKill();
        bagFlash.canvasRenderer.SetAlpha(0f);
        var alpha = 0f;
        DOTween.Sequence().SetUpdate(true).SetLink(bagFlash.gameObject)
            .Append(DOTween.To(() => alpha, v => { alpha = v; bagFlash.canvasRenderer.SetAlpha(v); }, bagFlashAlpha, 0.06f))
            .Append(DOTween.To(() => alpha, v => { alpha = v; bagFlash.canvasRenderer.SetAlpha(v); }, 0f, 0.32f).SetEase(Ease.OutQuad));
    }

    private void SpawnSparkles(Vector2 at)
    {
        for (var i = 0; i < sparkleCount; i++)
        {
            var go = new GameObject("StowSparkle", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(page, false);
            rect.SetAsLastSibling();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = at;
            var size = Random.Range(9f, 16f);
            rect.sizeDelta = new Vector2(size, size);
            rect.localEulerAngles = new Vector3(0f, 0f, 45f);
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            image.color = i % 2 == 0 ? sparkleColorA : sparkleColorB;

            var angle = (i / (float)Mathf.Max(1, sparkleCount)) * Mathf.PI * 2f + Random.Range(-0.25f, 0.25f);
            var distance = Random.Range(58f, 104f);
            var target = at + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            var duration = Random.Range(0.36f, 0.5f);
            DOTween.Sequence().SetUpdate(true).SetLink(go)
                .Join(rect.DOAnchorPos(target, duration).SetEase(Ease.OutCubic))
                .Join(rect.DOScale(0f, duration).SetEase(Ease.InQuad))
                .Join(rect.DORotate(new Vector3(0f, 0f, 45f + Random.Range(90f, 200f)), duration, RotateMode.FastBeyond360))
                .OnComplete(() => Destroy(go));
        }
    }

    private void PlayLandSfx()
    {
        if (string.IsNullOrWhiteSpace(landSfxPath) || !Engine.Initialized)
            return;
        if (Engine.TryGetService<IAudioManager>(out var audio) && audio != null)
            audio.PlaySfxFast(landSfxPath, landSfxVolume, null, true, true);
    }

    private GameObject CreateFlyer(Sprite sprite, Color placeholderColor, Vector2 at)
    {
        var go = new GameObject("StowFlyer", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        var rect = (RectTransform)go.transform;
        rect.SetParent(page, false);
        rect.SetAsLastSibling();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = flyerSize;
        rect.anchoredPosition = at;
        var group = go.GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        var image = go.GetComponent<Image>();
        image.raycastTarget = false;
        image.sprite = sprite;
        image.preserveAspect = sprite;
        image.color = sprite ? Color.white : placeholderColor;
        flyers.Add(go);
        return go;
    }

    /// <summary>Converts a world position into the page's top-left anchored coordinate space.</summary>
    private Vector2 ToPageLocal(Vector3 world)
    {
        var local = (Vector2)page.InverseTransformPoint(world);
        // Children are anchored at the page's top-left corner.
        var rect = page.rect;
        return new Vector2(local.x - rect.xMin, local.y - rect.yMax);
    }

    /// <summary>Control point above the higher end so the item rises before dropping into the bag.</summary>
    public static Vector2 CalculateArcControl(Vector2 from, Vector2 to, float minimumHeight)
    {
        var mid = (from + to) * 0.5f;
        var height = Mathf.Max(minimumHeight, Vector2.Distance(from, to) * 0.45f);
        return new Vector2(mid.x, Mathf.Max(from.y, to.y) + height);
    }

    public static Vector2 QuadraticBezier(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        t = Mathf.Clamp01(t);
        var a = Vector2.Lerp(start, control, t);
        var b = Vector2.Lerp(control, end, t);
        return Vector2.Lerp(a, b, t);
    }
}
