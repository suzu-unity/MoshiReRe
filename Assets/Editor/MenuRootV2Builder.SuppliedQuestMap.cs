using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class MenuRootV2Builder
{
    private static RectTransform SuppliedQuest(RectTransform parent)
    {
        var page = SuppliedPage(parent, "PageQuest", "quest_background");
        var controller = page.gameObject.AddComponent<QuestMenuController>();
        var route = page.gameObject.AddComponent<MenuQuestRouteController>();
        var title = SuppliedLabel(page, "QuestTitle", "", new Rect(939, 226, 625, 100), 31);
        var objective = SuppliedLabel(page, "Objective", "", new Rect(747, 585, 466, 139), 25);
        var hint = SuppliedLabel(page, "Hint", "", new Rect(1267, 585, 302, 139), 24);
        var progress = SuppliedLabel(page, "Progress", "", new Rect(941, 335, 613, 48), 25);
        var reward = SuppliedLabel(page, "Reward", "", new Rect(744, 751, 291, 85), 23);
        var list = SuppliedButton(page, "ActiveQuest", SuppliedMenuAssetLibrary.Slice("quest_parts", "quest_card", new Rect(37, 92, 663, 265)), new Rect(266, 252, 393, 156));
        var listTitle = SuppliedLabel(list.transform, "Title", "進行中のクエスト", new Rect(117, 36, 222, 82), 25);
        SuppliedImage(list.transform, "Portrait", SuppliedMenuAssetLibrary.Get("thumb_0"), new Rect(18, 26, 90, 97));
        SuppliedImage(page, "TargetPortrait", SuppliedMenuAssetLibrary.Get("portrait_0"), new Rect(721, 207, 182, 180));
        var nodes = new Image[5]; var sprites = new Sprite[5];
        var crops = new[] { new Rect(27, 408, 235, 222), new Rect(269, 408, 238, 222), new Rect(527, 414, 226, 208), new Rect(1200, 414, 221, 205), new Rect(27, 408, 235, 222) };
        var labels = new[] { "人物", "情報", "持ち物", "マップ", "会話" };
        for (var i = 0; i < nodes.Length; i++)
        {
            sprites[i] = SuppliedMenuAssetLibrary.Slice("quest_parts", "step_" + i, crops[i]);
            nodes[i] = SuppliedImage(page, "Step" + i, sprites[i], new Rect(768 + i * 165, 421, 89, 86));
            SuppliedLabel(page, "StepLabel" + i, labels[i], new Rect(743 + i * 165, 517, 140, 35), 23, TextAlignmentOptions.Center);
        }
        SuppliedButton(page, "GoButton", SuppliedMenuAssetLibrary.Slice("quest_parts", "go", new Rect(327, 855, 548, 218)), new Rect(1060, 743, 282, 111), route.Go);
        Wire(controller, ("activeQuestTitleText", title), ("activeQuestObjectiveText", objective), ("activeQuestProgressText", progress), ("activeQuestHintText", hint), ("activeQuestRewardText", reward));
        WireArray(controller, "inboxQuestButtons", new Object[] { list });
        Wire(route, ("menuRoot", parent.GetComponentInParent<MenuRootV2UI>()), ("conversationRoute", SuppliedCafeRoute(page, null, "OjisanArrival")));
        WireArray(route, "nodes", nodes); WireArray(route, "stepSprites", sprites);
        return page;
    }

    private static MapRouteLauncher SuppliedCafeRoute(RectTransform parent, MapMenuController controller, string label)
    {
        var launcher = parent.gameObject.AddComponent<MapRouteLauncher>();
        var so = new SerializedObject(launcher);
        SetObject(so, "mapController", controller);
        var routes = so.FindProperty("routes"); routes.arraySize = 1;
        var item = routes.GetArrayElementAtIndex(0);
        SetChildBool(item, "enabled", true); SetChildInt(item, "locationIndex", 3);
        SetChildString(item, "routeId", "papa_cafe"); SetChildString(item, "mapId", "papa_cafe");
        SetChildString(item, "sceneName", ""); SetChildString(item, "entryScriptPath", "Scenario/PapaQuestDemo");
        SetChildString(item, "entryLabel", label);
        so.ApplyModifiedPropertiesWithoutUndo();
        return launcher;
    }

    private static RectTransform SuppliedMap(RectTransform parent)
    {
        var page = SuppliedPage(parent, "PageMap", "map_background");
        var controller = page.gameObject.AddComponent<MapMenuController>();
        SuppliedEmptyPanel(page, "MapDetailsSurface", new Rect(1190, 184, 399, 645));
        var viewport = SuppliedImage(page, "MapViewport", null, new Rect(249, 210, 918, 615));
        viewport.raycastTarget = true; viewport.gameObject.AddComponent<RectMask2D>();
        var map = SuppliedImage(viewport.transform, "DraggableMap", SuppliedMenuAssetLibrary.Get("map"), new Rect(0, 0, 1320, 990));
        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport.rectTransform; scroll.content = map.rectTransform;
        scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 30;
        var pin = SuppliedMenuAssetLibrary.Slice("map_parts", "pin", new Rect(955, 10, 186, 219));
        var selectedPin = SuppliedMenuAssetLibrary.Slice("map_parts", "selected_pin", new Rect(1192, 10, 190, 219));
        var positions = new[] { new Vector2(580, 480), new Vector2(205, 360), new Vector2(835, 185), new Vector2(775, 574), new Vector2(435, 215), new Vector2(1060, 360) };
        var names = new[] { "駅前", "図書館", "会社", "カフェ", "公園", "ホテル" };
        var buttons = new Button[6]; var images = new Image[6];
        for (var i = 0; i < buttons.Length; i++)
        {
            buttons[i] = SuppliedButton(map.transform, "Location" + i, pin, new Rect(positions[i].x, positions[i].y, 78, 92));
            images[i] = buttons[i].image;
            var caption = SuppliedImage(buttons[i].transform, "Caption", SuppliedMenuAssetLibrary.Get("home_parts.bubble"), new Rect(-24, 88, 128, 46));
            SuppliedLabel(caption.transform, "Name", names[i], new Rect(8, 3, 112, 35), 23, TextAlignmentOptions.Center);
        }
        var title = SuppliedLabel(page, "LocationName", "", new Rect(1213, 220, 349, 51), 32);
        var description = SuppliedLabel(page, "Description", "", new Rect(1213, 285, 349, 158), 25);
        var hint = SuppliedLabel(page, "ReReHint", "", new Rect(1213, 473, 349, 155), 25);
        var go = SuppliedButton(page, "GoButton", SuppliedMenuAssetLibrary.Slice("quest_parts", "go", new Rect(327, 855, 548, 218)), new Rect(1246, 719, 282, 111));
        Wire(controller, ("detailNameText", title), ("detailDescriptionText", description), ("rereHintText", hint), ("normalPinSprite", pin), ("selectedPinSprite", selectedPin));
        WireArray(controller, "locationButtons", buttons); WireArray(controller, "locationImages", images); WireArray(controller, "goButtons", new Object[] { go });
        var so = new SerializedObject(controller); ConfigureMapLocations(so);
        so.FindProperty("initialLocationIndex").intValue = 3;
        so.FindProperty("onlyAllowConfiguredRoutes").boolValue = true;
        SetIntArray(so, "goButtonLocationIndexes", new[] { -1 }); so.ApplyModifiedPropertiesWithoutUndo();
        SuppliedCafeRoute(page, controller, "");
        return page;
    }
}
