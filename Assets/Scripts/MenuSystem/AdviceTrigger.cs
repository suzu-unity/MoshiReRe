using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// マウスオーバーでメッセージを表示するトリガー。
/// </summary>
public class AdviceTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private AdviceClickTrigger clickTrigger;
    [TextArea] public string hoverMessage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (clickTrigger != null)
        {
            // ホバー時は自動的に数秒で消えるように autoHide=true を指定
            clickTrigger.ShowAdvice(hoverMessage, true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        clickTrigger?.HideAdvice();
    }
}
