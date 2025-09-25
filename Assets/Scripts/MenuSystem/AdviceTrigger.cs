using UnityEngine;
using UnityEngine.EventSystems;

public class AdviceTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private AdviceBubble bubble;
    [TextArea] [SerializeField] private string message;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (bubble) bubble.Show(message, autoHide: false, forceTypewriter: false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (bubble) bubble.Hide();
    }
}
