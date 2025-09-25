using UnityEngine;
using UnityEngine.EventSystems;

public class AdviceTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private AdviceBubble bubble;
    [TextArea] [SerializeField] private string message;

    public void OnPointerEnter(PointerEventData eventData)
    {
        bubble.Show(message, autoHide:false, typewriter:true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bubble.Hide();
    }
}