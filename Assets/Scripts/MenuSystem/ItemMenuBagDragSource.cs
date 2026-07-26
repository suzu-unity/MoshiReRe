using UnityEngine;
using UnityEngine.EventSystems;

public class ItemMenuBagDragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ItemMenuController controller;
    private int slotIndex;

    public void Initialize(ItemMenuController owner, int index)
    {
        controller = owner;
        slotIndex = index;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        controller?.BeginBagDrag(slotIndex, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        controller?.MoveBagDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        controller?.EndBagDrag(slotIndex, eventData);
    }
}
