using UnityEngine;
using UnityEngine.EventSystems;

public class AdviceClickTrigger : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AdviceBubble bubble;
    [TextArea] [SerializeField] private string message;

    public void OnPointerClick(PointerEventData eventData)
    {
        bubble.Show(message, autoHide:true, autoHideDelay:3f, typewriter:true);
    }
}