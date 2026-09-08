using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class MenuRootV2Builder
{
    private static RectTransform SuppliedSave(RectTransform parent)
    {
        var page = SuppliedPage(parent, "PageSave", "save_background");
        SuppliedEmptyPanel(page, "SlotSurface", new Rect(242, 178, 1352, 668));
        var controller = page.gameObject.AddComponent<MenuSaveLoadController>();
        var saveNormal = SuppliedMenuAssetLibrary.Slice("save_parts", "save_normal", new Rect(319, 235, 630, 140));
        var saveSelected = SuppliedMenuAssetLibrary.Slice("save_parts", "save_selected", new Rect(319, 83, 630, 137));
        var loadNormal = SuppliedMenuAssetLibrary.Slice("save_parts", "load_normal", new Rect(946, 83, 596, 139));
        var loadSelected = SuppliedMenuAssetLibrary.Slice("save_parts", "load_selected", new Rect(946, 235, 596, 140));
        var save = SuppliedButton(page, "SaveTab", saveSelected, new Rect(288, 185, 584, 127));
        var load = SuppliedButton(page, "LoadTab", loadNormal, new Rect(871, 185, 553, 127));
        var slotSprite = SuppliedMenuAssetLibrary.Slice("settings_final", "save_slot_surface", new Rect(1030, 825, 224, 97), new Vector4(14, 14, 47, 18));
        var template = SuppliedButton(page, "SaveSlot", slotSprite, new Rect(0, 0, 407, 185));
        template.image.type = Image.Type.Sliced;
        SuppliedLabel(template.transform, "Detail", "", new Rect(139, 21, 251, 119), 21);
        var previewArea = RectRoot("PreviewArea", (RectTransform)template.transform); Place(previewArea, 13, 32, 115, 90);
        var previewObject = new GameObject("Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(AspectRatioFitter));
        previewObject.transform.SetParent(previewArea, false);
        var previewAspect = previewObject.GetComponent<AspectRatioFitter>(); previewAspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent; previewAspect.aspectRatio = 16f / 9f;
        previewObject.GetComponent<RawImage>().raycastTarget = false;
        var prefab = PrefabUtility.SaveAsPrefabAsset(template.gameObject, SuppliedPrefabFolder + "/SaveSlot.prefab");
        Object.DestroyImmediate(template.gameObject);
        var views = new MenuSaveLoadController.SlotView[9]; var checks = new Toggle[9];
        var uncheckedSprite = SuppliedMenuAssetLibrary.Slice("save_extra", "unchecked", new Rect(146, 113, 133, 136));
        var checkedSprite = SuppliedMenuAssetLibrary.Slice("save_extra", "checked", new Rect(325, 101, 157, 164));
        for (var i = 0; i < views.Length; i++)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, page);
            instance.name = "SaveSlot" + (i + 1);
            Place(instance.transform, 276 + (i % 3) * 430, 314 + (i / 3) * 166, 407, 157);
            var detail = instance.GetComponentInChildren<TextMeshProUGUI>(); detail.text = $"SLOT {i + 1:00}\n空きスロット";
            views[i] = new MenuSaveLoadController.SlotView(instance.GetComponent<Button>(), null, detail, instance.GetComponentInChildren<RawImage>(true));
            var check = SuppliedImage(instance.transform, "DeleteSelection", uncheckedSprite, new Rect(6, 7, 40, 40));
            check.raycastTarget = true; checks[i] = check.gameObject.AddComponent<Toggle>(); checks[i].targetGraphic = check;
            checks[i].graphic = SuppliedImage(check.transform, "Selected", checkedSprite, new Rect(0, 0, 40, 40));
            checks[i].SetIsOnWithoutNotify(false); check.gameObject.SetActive(false);
        }
        SuppliedButton(page, "DeleteMode", SuppliedMenuAssetLibrary.Slice("save_parts", "trash", new Rect(1168, 390, 414, 149)), new Rect(1351, 814, 201, 72), controller.EnterDeleteMode);
        var actions = RectRoot("DeleteActions", page); Stretch(actions, Vector2.zero, Vector2.zero);
        SuppliedButton(actions, "DeleteSelected", SuppliedMenuAssetLibrary.Slice("save_extra", "delete_selected", new Rect(532, 291, 681, 148)), new Rect(444, 813, 520, 81), controller.RequestBulkDelete);
        SuppliedButton(actions, "CancelSelection", SuppliedMenuAssetLibrary.Slice("save_extra", "cancel", new Rect(1235, 291, 392, 148)), new Rect(976, 813, 261, 81), controller.ExitDeleteMode);
        actions.gameObject.SetActive(false);
        var confirmation = SuppliedImage(page, "Confirmation", SuppliedMenuAssetLibrary.Get("home_parts.bubble"), new Rect(513, 348, 748, 290));
        confirmation.raycastTarget = true;
        var confirmationLabel = SuppliedLabel(confirmation.transform, "Message", "", new Rect(57, 30, 633, 127), 28, TextAlignmentOptions.Center);
        var confirm = SuppliedButton(confirmation.transform, "Confirm", SuppliedMenuAssetLibrary.Get("nav.selected"), new Rect(101, 159, 215, 104));
        SuppliedLabel(confirm.transform, "Label", "実行", new Rect(0, 0, 215, 104), 27, TextAlignmentOptions.Center);
        var cancel = SuppliedButton(confirmation.transform, "Cancel", SuppliedMenuAssetLibrary.Slice("save_extra", "cancel", new Rect(1235, 291, 392, 148)), new Rect(402, 178, 260, 82));
        confirmation.gameObject.SetActive(false);
        controller.Configure(views, save, load, null, confirm, cancel, confirmation.gameObject, confirmationLabel, null);
        WireArray(controller, "deleteSelections", checks);
        Wire(controller, ("deleteActions", actions.gameObject), ("saveNormalSprite", saveNormal), ("saveSelectedSprite", saveSelected), ("loadNormalSprite", loadNormal), ("loadSelectedSprite", loadSelected));
        return page;
    }

    private static RectTransform SuppliedSettings(RectTransform parent)
    {
        var page = SuppliedPage(parent, "PageSettings", "quest_background");
        // Empty supplied decorative panel covers the quest-specific interior, retaining the common phone shell.
        SuppliedEmptyPanel(page, "SettingsInterior", new Rect(244, 166, 1350, 680));
        var tabs = new Button[3]; var panels = new GameObject[3];
        var crops = new[] { new Rect(1215, 200, 181, 64), new Rect(1215, 125, 181, 66), new Rect(1215, 274, 181, 65) };
        for (var i = 0; i < 3; i++)
        {
            tabs[i] = SuppliedButton(page, "Category" + i, SuppliedMenuAssetLibrary.Slice("settings_final", "category_" + i, crops[i]), new Rect(272, 265 + i * 114, 287, 102));
            var panel = RectRoot("CategoryPanel" + i, page); Stretch(panel, Vector2.zero, Vector2.zero); panels[i] = panel.gameObject;
        }
        var sliders = new Slider[5];
        var labels = new[] { "BGM音量", "SE音量", "ボイス音量", "文字表示速度", "オート送り速度" };
        var trackSprite = SuppliedMenuAssetLibrary.Slice("settings_final", "slider_rail", new Rect(1591, 107, 61, 16), new Vector4(3, 3, 6, 3));
        var knobSprite = SuppliedMenuAssetLibrary.Slice("settings_final", "slider_handle", new Rect(1560, 59, 29, 31));
        for (var i = 0; i < 5; i++)
        {
            var panel = panels[i < 3 ? 0 : 1].transform;
            var y = 298 + (i < 3 ? i : i - 3) * 139;
            SuppliedLabel(panel, "Label" + i, labels[i], new Rect(611, y, 359, 54), 29);
            var track = SuppliedImage(panel, "Slider" + i, trackSprite, new Rect(962, y + 8, 443, 47));
            track.type = Image.Type.Sliced;
            track.raycastTarget = true; sliders[i] = track.gameObject.AddComponent<Slider>();
            var handleArea = RectRoot("HandleArea", track.rectTransform); Stretch(handleArea, new Vector2(20, 0), new Vector2(-20, 0));
            var handle = SuppliedImage(handleArea, "Handle", knobSprite, new Rect(0, 0, 48, 50));
            handle.rectTransform.pivot = new Vector2(.5f, .5f);
            sliders[i].handleRect = handle.rectTransform; sliders[i].targetGraphic = handle;
            sliders[i].minValue = 0; sliders[i].maxValue = 1; sliders[i].value = 1;
            var value = SuppliedLabel(panel, "Value" + i, "100%", new Rect(1422, y, 124, 54), 28);
            value.gameObject.AddComponent<MenuSliderValueLabel>().Configure(sliders[i], value);
        }
        var display = panels[2].transform;
        SuppliedLabel(display, "FullscreenLabel", "フルスクリーン", new Rect(611, 305, 399, 54), 29);
        var toggleImage = SuppliedImage(display, "FullscreenToggle", SuppliedMenuAssetLibrary.Slice("settings_final", "checkbox", new Rect(816, 704, 33, 35)), new Rect(1170, 306, 52, 55));
        toggleImage.raycastTarget = true;
        var toggle = toggleImage.gameObject.AddComponent<Toggle>(); toggle.targetGraphic = toggleImage;
        toggle.graphic = SuppliedImage(toggleImage.transform, "Check", SuppliedMenuAssetLibrary.Slice("settings_final", "checkbox_checked", new Rect(701, 704, 37, 37)), new Rect(0, 0, 52, 55));
        var reset = SuppliedButton(page, "Reset", SuppliedMenuAssetLibrary.Slice("settings_final", "reset", new Rect(20, 687, 218, 57)), new Rect(1145, 735, 366, 96));
        page.gameObject.AddComponent<MenuSettingsController>().Configure(sliders[0], sliders[1], sliders[2], sliders[3], sliders[4], toggle, reset, null);
        var categories = page.gameObject.AddComponent<MenuSettingsTabs>(); WireArray(categories, "buttons", tabs); WireArray(categories, "panels", panels);
        return page;
    }
}
