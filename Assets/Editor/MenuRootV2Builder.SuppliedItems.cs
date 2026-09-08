using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class MenuRootV2Builder
{
    private static RectTransform SuppliedItems(RectTransform parent)
    {
        var page = SuppliedPage(parent, "PageItems", "items_background");
        SuppliedEmptyPanel(page, "ItemGridSurface", new Rect(255, 277, 667, 554));
        var controller = page.gameObject.AddComponent<ItemMenuController>();
        var database = FindFirstAssetOfType<InventoryDatabase>();
        var filterButtons = new Button[4]; var normalTabs = new Sprite[4]; var selectedTabs = new Sprite[4];
        var tabX = new[] { 209, 413, 619, 821 };
        for(var i=0;i<4;i++)
        {
            normalTabs[i] = SuppliedMenuAssetLibrary.Slice("items_parts", "item_filter_normal_"+i, new Rect(tabX[i], 147, 197, 73));
            selectedTabs[i] = SuppliedMenuAssetLibrary.Slice("items_parts", "item_filter_selected_"+i, new Rect(tabX[i], 63, 197, 82));
            filterButtons[i] = SuppliedButton(page, "ItemFilter"+i, i==0?selectedTabs[i]:normalTabs[i], new Rect(266+i*160, 215, 145, 59));
        }
        var emptyText=SuppliedLabel(page, "EmptyItems", "該当するアイテムはありません", new Rect(294, 448, 582, 80), 27, TextAlignmentOptions.Center);
        WireArray(controller, "filterButtons", filterButtons); WireArray(controller, "filterNormalSprites", normalTabs); WireArray(controller, "filterSelectedSprites", selectedTabs);
        Wire(controller, ("filterEmptyText", emptyText));
        var cell = SuppliedMenuAssetLibrary.Slice("items_parts", "item_cell", new Rect(207, 238, 202, 188));
        var selection = SuppliedMenuAssetLibrary.Slice("items_parts", "item_selected", new Rect(410, 228, 232, 203));
        var buttons = new Button[12]; var icons = new Image[12]; var labels = new TMP_Text[12]; var highlights = new RectTransform[12]; var supplied = new Sprite[database.items.Count];
        for (var i=0;i<database.items.Count;i++)
        {
            var item = database.items[i]; if (!item) continue;
            supplied[i] = item.id == "cafe_ticket" ? SuppliedMenuAssetLibrary.Slice("items_mock", "cafe_ticket_icon", new Rect(282, 312, 116, 87))
                : item.id == "old_key" ? SuppliedMenuAssetLibrary.Slice("items_mock", "old_key_icon", new Rect(780, 304, 100, 91)) : item.icon ? item.icon : SuppliedMenuAssetLibrary.Get("nav.quest");
        }
        for (var i = 0; i < 12; i++)
        {
            var x = 269 + i % 4 * 160; var y = 289 + i / 4 * 174;
            buttons[i] = SuppliedButton(page, "ItemCard" + i, cell, new Rect(x, y, 151, 163));
            highlights[i] = SuppliedImage(buttons[i].transform, "Selection", selection, new Rect(0, 0, 151, 163)).rectTransform;
            icons[i] = SuppliedImage(buttons[i].transform, "ItemIcon" + i, null, new Rect(22, 13, 107, 100));
            labels[i] = SuppliedLabel(buttons[i].transform, "ItemName" + i, "", new Rect(6, 118, 145, 39), 22, TextAlignmentOptions.Center);
        }
        var detailIcon = SuppliedImage(page, "DetailItemIcon", null, new Rect(966, 213, 155, 156));
        var title = SuppliedLabel(page, "DetailTitle", "", new Rect(1146, 202, 421, 48), 30);
        var description = SuppliedLabel(page, "DetailDescription", "", new Rect(1146, 265, 421, 119), 25);
        var bag = SuppliedImage(page, "BagDropArea", null, new Rect(948, 461, 650, 364));
        var bagSlots = new Image[8]; var bagLabels = new TMP_Text[8];
        for (var i = 0; i < 8; i++)
        {
            var slot = SuppliedImage(bag.transform, "BagSlot" + i, null, new Rect(119 + i % 4 * 100, 87 + i / 4 * 96, 85, 85));
            bagSlots[i] = SuppliedImage(slot.transform, "BagSlotIcon" + i, null, new Rect(8, 8, 69, 69));
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
