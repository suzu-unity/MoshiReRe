using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class MenuRootV2Builder
{
    private static RectTransform SuppliedItems(RectTransform parent)
    {
        var page = SuppliedPage(parent, "PageItems", "items_background");
        var controller = page.gameObject.AddComponent<ItemMenuController>();
        var database = FindFirstAssetOfType<InventoryDatabase>();
        var cell = SuppliedMenuAssetLibrary.Slice("items_parts", "item_cell", new Rect(207, 238, 202, 188));
        var selection = SuppliedMenuAssetLibrary.Slice("items_parts", "item_selected", new Rect(410, 228, 232, 203));
        var buttons = new Button[12]; var icons = new Image[12]; var labels = new TMP_Text[12]; var highlights = new RectTransform[12]; var supplied = new Sprite[12];
        for (var i = 0; i < 12; i++)
        {
            var x = 278 + i % 4 * 176; var y = 245 + i / 4 * 181;
            buttons[i] = SuppliedButton(page, "ItemCard" + i, cell, new Rect(x, y, 157, 163));
            highlights[i] = SuppliedImage(buttons[i].transform, "Selection", selection, new Rect(0, 0, 157, 163)).rectTransform;
            supplied[i] = SuppliedMenuAssetLibrary.Slice("items_mock", "item_icon_" + i, new Rect(285 + i % 4 * 173, 249 + i / 4 * 182, 122, 102));
            icons[i] = SuppliedImage(buttons[i].transform, "ItemIcon" + i, supplied[i], new Rect(25, 13, 107, 100));
            labels[i] = SuppliedLabel(buttons[i].transform, "ItemName" + i, "", new Rect(6, 118, 145, 39), 22, TextAlignmentOptions.Center);
        }
        var detailIcon = SuppliedImage(page, "DetailItemIcon", null, new Rect(1067, 195, 142, 149));
        var title = SuppliedLabel(page, "DetailTitle", "", new Rect(1231, 192, 348, 48), 30);
        var description = SuppliedLabel(page, "DetailDescription", "", new Rect(1231, 248, 337, 124), 25);
        var bag = SuppliedImage(page, "BagDropArea", SuppliedMenuAssetLibrary.Slice("items_parts", "bag_art", new Rect(931, 409, 741, 431)), new Rect(1024, 449, 575, 337));
        var bagSlots = new Image[8]; var bagLabels = new TMP_Text[8];
        var slotSprite = SuppliedMenuAssetLibrary.Slice("items_parts", "bag_slot", new Rect(352, 738, 169, 161));
        for (var i = 0; i < 8; i++)
        {
            var slot = SuppliedImage(bag.transform, "BagSlot" + i, slotSprite, new Rect(112 + i % 4 * 84, 92 + i / 4 * 85, 73, 72));
            bagSlots[i] = SuppliedImage(slot.transform, "BagSlotIcon" + i, null, new Rect(8, 4, 57, 46));
            bagLabels[i] = SuppliedLabel(slot.transform, "BagSlotName" + i, "", new Rect(2, 48, 69, 22), 12, TextAlignmentOptions.Center);
        }
        var status = SuppliedLabel(page, "BagStatusText", "0 / 4", new Rect(1160, 805, 360, 40), 24, TextAlignmentOptions.Center);
        Wire(controller, ("inventoryDatabase", database), ("detailIconImage", detailIcon), ("detailTitleText", title), ("detailDescriptionText", description), ("bagDropArea", bag.rectTransform), ("bagStatusText", status));
        WireArray(controller, "itemButtons", buttons);
        WireArray(controller, "itemHighlights", highlights);
        WireArray(controller, "itemIconImages", icons);
        WireArray(controller, "itemNameTexts", labels);
        WireArray(controller, "suppliedItemIcons", supplied);
        WireArray(controller, "bagSlotImages", bagSlots);
        WireArray(controller, "bagSlotTexts", bagLabels);
        return page;
    }
}
