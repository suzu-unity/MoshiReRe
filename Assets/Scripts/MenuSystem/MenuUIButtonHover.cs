using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MenuUIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float dur = 0.12f;

    Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
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