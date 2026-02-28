using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 簡易マウスオーバー検知（EventTrigger の代替）。
/// </summary>
public class UIPointerEvents : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public System.Action onEnter;
    public System.Action onExit;

    public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke();
    public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke();
}