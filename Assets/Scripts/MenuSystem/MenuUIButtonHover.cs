using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MenuUIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float dur = 0.12f;

    Vector3 baseScale;
    bool initialized;

    void Awake()
    {
        baseScale = transform.localScale;
        initialized = true;
    }

    void OnDisable()
    {
        // Page changes can disable a hovered button before PointerExit arrives.
        // Never carry its enlarged/pressed scale into the next menu visit.
        transform.DOKill();
        if (initialized)
            transform.localScale = baseScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(baseScale * hoverScale, dur).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(baseScale, dur).SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(baseScale * pressScale, dur * 0.8f).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(baseScale * hoverScale, dur * 0.8f).SetUpdate(true);
    }
}
