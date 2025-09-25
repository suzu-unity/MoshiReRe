using UnityEngine;
using UnityEngine.EventSystems;

public class AdviceClickTrigger : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private AdviceBubble bubble;
    [TextArea] [SerializeField] private string[] messages;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!bubble || messages == null || messages.Length == 0) return;

        string msg = messages[Random.Range(0, messages.Length)];
        bubble.Show(msg, autoHide: true, autoHideDelay: 3f, forceTypewriter: true);
    }
}
