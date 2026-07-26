using UnityEngine;
using UnityEngine.EventSystems;

public class ItemMenuDragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ItemMenuController controller;
    private int itemIndex;

    public void Initialize(ItemMenuController owner, int index)
    {
        controller = owner;
        itemIndex = index;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        controller?.BeginItemDrag(itemIndex, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        controller?.MoveItemDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        controller?.EndItemDrag(itemIndex, eventData);
    }
}
