using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRelationHintTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CharacterMenuController controller;
    [SerializeField, TextArea] private string message;
    [SerializeField] private Vector2 bubbleOffset = new Vector2(84f, 36f);

    public void Configure(CharacterMenuController owner, string hint, Vector2 offset)
    {
        controller = owner;
        message = hint;
        bubbleOffset = offset;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!controller)
            controller = GetComponentInParent<CharacterMenuController>();

        var rect = transform as RectTransform;
        controller?.ShowRelationHint(message, rect ? rect.anchoredPosition + bubbleOffset : bubbleOffset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        controller?.HideRelationHint();
    }
}
