using UnityEngine;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 簡易マウスオーバー検知用コンポーネント。
/// IPointerEnterHandler / IPointerExitHandler を利用して
/// onEnter / onExit のコールバックを発火します。
/// </summary>
public class UIPointerEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action onEnter;
    public Action onExit;

    public void OnPointerEnter(PointerEventData eventData)
        => onEnter?.Invoke();

    public void OnPointerExit(PointerEventData eventData)
        => onExit?.Invoke();
}
