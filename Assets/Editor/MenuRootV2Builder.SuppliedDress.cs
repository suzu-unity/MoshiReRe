using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class MenuRootV2Builder
{
    private static RectTransform SuppliedDress(RectTransform parent)
    {
        var page = SuppliedPage(parent, "PageDressStatus", "dress_background");
        var controller = page.gameObject.AddComponent<DressMenuController>();
        var order = new[] { 3, 0, 2, 1, 4, 5 };
        var standing = new Sprite[6]; var icons = new Sprite[6];
        var buttons = new Button[6]; var highlights = new RectTransform[6]; var iconImages = new Image[6];
        var normal = SuppliedMenuAssetLibrary.Slice("dress_parts", "outfit_card", new Rect(80, 505, 240, 300));
        var selected = SuppliedMenuAssetLibrary.Slice("dress_parts", "outfit_card_selected", new Rect(326, 498, 245, 310));
        var names = new[] { "部屋着", "おでかけ", "仕事着", "サイバー", "フォーマル", "カジュアル" };
        for (var i = 0; i < 6; i++)
        {
            standing[i] = SuppliedMenuAssetLibrary.Get("outfit_" + order[i]);
            icons[i] = SuppliedMenuAssetLibrary.Slice("dress_mock", "closet_icon_" + i, new Rect(746 + i * 140, 656, 115, 106));
            buttons[i] = SuppliedButton(page, "OutfitCard" + i, normal, new Rect(732 + i * 141, 648, 132, 178));
            highlights[i] = SuppliedImage(buttons[i].transform, "SelectedFrame", selected, new Rect(0, 0, 132, 178)).rectTransform;
            iconImages[i] = SuppliedImage(buttons[i].transform, "OutfitIcon", icons[i], new Rect(10, 8, 112, 100));
            SuppliedLabel(buttons[i].transform, "OutfitName", names[i], new Rect(2, 113, 128, 34), 22, TMPro.TextAlignmentOptions.Center);
        }
        var fitting = SuppliedImage(page, "FittingSpriteTint", standing[0], new Rect(275, 143, 419, 716));
        SuppliedImage(page, "SmallReRe", SuppliedMenuAssetLibrary.Get("rere"), new Rect(1317, 365, 213, 250));
        var comment = SuppliedLabel(page, "DressCommentText", "", new Rect(1291, 234, 285, 125), 24, TMPro.TextAlignmentOptions.Center);
        var radarRoot = RectRoot("OutfitRadar", page);
        Place(radarRoot, 866, 281, 226, 226);
        var radar = radarRoot.gameObject.AddComponent<RadarChart>();
        radar.color = new Color(.88f, .38f, .92f, .42f);
        radar.raycastTarget = false;
        radar.SetRadius(108);
        radar.SetMaxValue(10);
        Wire(controller, ("fittingSpriteTint", fitting), ("commentText", comment), ("outfitRadar", radar));
        WireArray(controller, "outfitButtons", buttons);
        WireArray(controller, "outfitHighlights", highlights);
        WireArray(controller, "outfitIconImages", iconImages);
        WireArray(controller, "suppliedStandingSprites", standing);
        WireArray(controller, "suppliedClosetSprites", icons);
        var so = new SerializedObject(controller);
        so.FindProperty("equipOnSelection").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
        SuppliedButton(page, "PreviousOutfit", SuppliedMenuAssetLibrary.Get("nav.previous"), new Rect(278, 445, 66, 70), controller.SelectPreviousOutfit);
        SuppliedButton(page, "NextOutfit", SuppliedMenuAssetLibrary.Get("nav.next"), new Rect(624, 445, 66, 70), controller.SelectNextOutfit);
        return page;
    }
}
