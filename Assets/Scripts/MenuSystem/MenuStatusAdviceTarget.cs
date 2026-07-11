using UnityEngine;
using UnityEngine.EventSystems;

public class MenuStatusAdviceTarget : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private MenuStatusAdviceType adviceType;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MenuReReAdvisor.Instance == null) return;
        MenuReReAdvisor.Instance.ShowStatusHint(adviceType);
    }
}
