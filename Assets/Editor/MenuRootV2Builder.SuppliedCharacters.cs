using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class MenuRootV2Builder
{
    private static RectTransform SuppliedCharacters(RectTransform parent)
    {
        var page = SuppliedPage(parent, "PageCharacters", "characters_background");
        var database = FindFirstAssetOfType<CharacterDatabase>();
        var characters = database.GetAll().Where(c => c).ToArray();
        SuppliedEmptyPanel(page, "CharacterListSurface", new Rect(247, 171, 336, 672));
        SuppliedEmptyPanel(page, "InformationSurface", new Rect(987, 180, 607, 649));
        var view = page.gameObject.AddComponent<MenuCharacterPresentation>();
        var panel = page.gameObject.AddComponent<CharacterInformationNodePanel>();
        var portraits = new Sprite[characters.Length];
        for (var i = 0; i < characters.Length; i++) portraits[i] = SuppliedMenuAssetLibrary.Get(characters[i].id.ToLowerInvariant() == "rere" ? "rere" : characters[i].id == "Wanabi" ? "portrait_1" : characters[i].category == CharacterCategory.Itadaki ? "portrait_3" : "portrait_0");
        var portrait = SuppliedImage(page, "SelectedCharacterPortrait", portraits[0], new Rect(593, 227, 378, 540));
        var title = SuppliedLabel(page, "SelectedCharacterName", "", new Rect(621, 786, 329, 48), 28, TextAlignmentOptions.Center);
        var listViewport = SuppliedImage(page, "CharacterListViewport", null, new Rect(256, 237, 330, 563));
        listViewport.raycastTarget = true; listViewport.gameObject.AddComponent<RectMask2D>();
        var listScroll = listViewport.gameObject.AddComponent<ScrollRect>(); listScroll.viewport = listViewport.rectTransform; listScroll.horizontal = false;
        var list = RectRoot("CharacterList", listViewport.rectTransform);
        Place(list, 0, 0, 330, 563);
        listScroll.content = list;
        list.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8; layout.childControlWidth = true; layout.childControlHeight = true; layout.childForceExpandHeight = false;
        var rows = new Button[characters.Length];
        var rowSprite = SuppliedMenuAssetLibrary.Slice("settings_final", "character_row_surface", new Rect(1030, 825, 224, 97), new Vector4(14, 14, 47, 18));
        for (var i = 0; i < rows.Length; i++)
        {
            rows[i] = SuppliedButton(list, "CharacterRow" + i, rowSprite, new Rect(0, i * 110, 330, 103));
            rows[i].image.type = Image.Type.Sliced;
            rows[i].gameObject.AddComponent<LayoutElement>().preferredHeight = 103;
            var thumb = characters[i].id.ToLowerInvariant() == "rere" ? 4 : characters[i].id == "Wanabi" ? 1 : characters[i].category == CharacterCategory.Itadaki ? 3 : 0;
            SuppliedImage(rows[i].transform, "Thumbnail", SuppliedMenuAssetLibrary.Get("thumb_" + thumb), new Rect(14, 8, 76, 84));
            SuppliedLabel(rows[i].transform, "Name", CharacterInformationNodePanel.GetDisplayName(characters[i]), new Rect(105, 20, 204, 51), 23);
        }
        var nodes = RectRoot("InformationNodeViewport", page);
        Place(nodes, 993, 208, 603, 625);
        nodes.gameObject.AddComponent<RectMask2D>();
        var inputSurface = nodes.gameObject.AddComponent<Image>(); inputSurface.color = Color.clear; inputSurface.raycastTarget = true;
        var scroll = nodes.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = nodes; scroll.horizontal = false;
        var content = RectRoot("InformationNodeList", nodes);
        Place(content, 0, 0, 600, 620);
        var grid = content.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(283, 265); grid.spacing = new Vector2(12, 12); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 2;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = content;
        var frames = new[] {
            SuppliedMenuAssetLibrary.Slice("characters_parts", "node_basic", new Rect(971, 131, 319, 260)),
            SuppliedMenuAssetLibrary.Slice("characters_parts", "node_self", new Rect(1290, 131, 339, 260)),
            SuppliedMenuAssetLibrary.Slice("characters_parts", "node_desire", new Rect(971, 393, 319, 263)),
            SuppliedMenuAssetLibrary.Slice("characters_parts", "node_risk", new Rect(1290, 393, 339, 263)) };
        var template = SuppliedImage(page, "InformationNodeCard", frames[0], new Rect(0, 0, 283, 265));
        var category = SuppliedLabel(template.transform, "Category", "基本情報", new Rect(66, 22, 205, 37), 25);
        var nodeTitle = SuppliedLabel(template.transform, "Title", "", new Rect(20, 82, 245, 43), 22, TextAlignmentOptions.Center);
        SuppliedEmptyPanel(template.transform, "NodeTextSurface", new Rect(18, 135, 247, 112));
        var nodeContent = SuppliedLabel(template.transform, "Content", "", new Rect(28, 143, 227, 96), 22, TextAlignmentOptions.Center);
        var illustration=SuppliedImage(template.transform, "NodeImage", null, new Rect(28, 148, 82, 82));
        var unknown = SuppliedImage(template.transform, "Unknown", SuppliedMenuAssetLibrary.Slice("characters_parts", "node_unknown", new Rect(958, 795, 107, 107)), new Rect(107, 173, 71, 71));
        var card = template.gameObject.AddComponent<MenuInformationNodeCard>();
        Wire(card, ("frame", template), ("category", category), ("title", nodeTitle), ("content", nodeContent), ("unknown", unknown.gameObject));
        Wire(card, ("illustration", illustration));
        WireArray(card, "categoryFrames", frames);
        var nodePrefab = PrefabUtility.SaveAsPrefabAsset(template.gameObject, SuppliedPrefabFolder + "/InformationNodeCard.prefab");
        UnityEngine.Object.DestroyImmediate(template.gameObject);
        Wire(panel, ("characterDatabase", database), ("selectedCharacterText", title), ("nodeListRoot", content), ("nodeRowPrefab", nodePrefab));
        var hearts = new Image[5];
        var emptyHeart = SuppliedMenuAssetLibrary.Slice("characters_parts", "heart_empty", new Rect(598, 143, 74, 76));
        var fullHeart = SuppliedMenuAssetLibrary.Slice("characters_parts", "heart_full", new Rect(876, 143, 80, 76));
        for (var i = 0; i < hearts.Length; i++) hearts[i] = SuppliedImage(page, "Affinity" + i, emptyHeart, new Rect(675 + i * 48, 184, 39, 38));
        Wire(view, ("portrait", portrait), ("information", panel), ("emptyHeart", emptyHeart), ("filledHeart", fullHeart));
        WireArray(view, "characters", characters); WireArray(view, "rows", rows); WireArray(view, "hearts", hearts);
        WireArray(view, "portraits", portraits);
        var filterSprite = SuppliedMenuAssetLibrary.Slice("characters_parts", "character_filter", new Rect(952, 77, 131, 40));
        var filters = new UnityEngine.Events.UnityAction[] { view.ShowAll, view.ShowOji, view.ShowItadaki };
        var filterLabels = new[] { "ALL", "おぢ", "頂き女子" };
        for (var i = 0; i < 3; i++)
        {
            var button = SuppliedButton(page, "CharacterFilter" + i, filterSprite, new Rect(252 + i * 114, 180, 108, 45), filters[i]);
            SuppliedLabel(button.transform, "Label", filterLabels[i], new Rect(0, 0, 108, 45), 23, TextAlignmentOptions.Center);
        }
        return page;
    }
}
