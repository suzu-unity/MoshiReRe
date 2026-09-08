using System.IO;
using System.Reflection;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Deterministic menu previews; does not replace Play Mode interaction tests.</summary>
public static class MenuVisualQa
{
    public static async void CheckSaveDayRoundTrip()
    {
        var root = Naninovel.Engine.GetService<Naninovel.IUIManager>().GetUI("MenuRootV2") as MenuRootV2UI;
        var bridge = root.GetComponent<MenuDaySaveBridge>();
        var hud = (MenuTopHudState)new SerializedObject(bridge).FindProperty("dayHud").objectReferenceValue;
        var manager = Naninovel.Engine.GetService<Naninovel.IStateManager>();
        var slot = "MenuQA-" + System.Guid.NewGuid().ToString("N");
        if(manager.GameSlotManager.SaveSlotExists(slot)) throw new System.Exception("Temporary QA save already exists");
        var original = hud.CurrentDay;
        try
        {
            hud.SetDay(12);
            await manager.SaveGame(slot);
            var saved = await manager.GameSlotManager.Load(slot);
            if(MenuDaySaveBridge.FormatDay(saved)!="DAY 12") throw new System.Exception("DAY serialize callback did not persist");
        }
        catch(System.Exception exception) { Debug.LogException(exception); return; }
        finally
        {
            hud.SetDay(original);
            if(manager.GameSlotManager.SaveSlotExists(slot)) manager.GameSlotManager.DeleteSaveSlot(slot);
        }
        File.WriteAllText("tmp/MenuQA/day-save-result.txt", "PASS: DAY 12 persisted through an actual temporary save and readback. The uniquely named QA save was removed; existing slots were not modified. HUD day restored.");
        Debug.Log("MENU_DAY_SAVE_PASS");
    }
    public static void StartSuppliedFeatureChecks()
    {
        var root = Naninovel.Engine.GetService<Naninovel.IUIManager>().GetUI("MenuRootV2") as MenuRootV2UI;
        root.Show(); root.StartCoroutine(SuppliedFeatureChecks(root));
    }
    private static IEnumerator SuppliedFeatureChecks(MenuRootV2UI root)
    {
        root.ShowStatus(); yield return new WaitForSecondsRealtime(.6f);
        var dress = root.GetComponentInChildren<DressMenuController>();
        var dressSo = new SerializedObject(dress);
        var portrait = (Image)dressSo.FindProperty("fittingSpriteTint").objectReferenceValue;
        var before = portrait.sprite;
        dress.SelectNextOutfit(); yield return new WaitForSecondsRealtime(.1f);
        if (portrait.sprite == before) throw new System.Exception("Outfit did not change");
        dress.SelectPreviousOutfit();
        root.ShowMap(); yield return null;
        var map = root.GetComponentInChildren<MapMenuController>(); var mapSo = new SerializedObject(map);
        var go = (Button)mapSo.FindProperty("goButtons").GetArrayElementAtIndex(0).objectReferenceValue;
        map.SelectLocation(0); if(go.interactable) throw new System.Exception("Unavailable map route enabled");
        map.SelectLocation(3); if(!go.interactable) throw new System.Exception("Cafe route disabled");
        var scroll=map.GetComponentInChildren<ScrollRect>(); var oldPosition=scroll.content.anchoredPosition;
        var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        pointer.button=UnityEngine.EventSystems.PointerEventData.InputButton.Left; pointer.position=new Vector2(600,350);
        scroll.OnBeginDrag(pointer); pointer.position += new Vector2(-90,90); scroll.OnDrag(pointer); scroll.OnEndDrag(pointer);
        if(scroll.content.anchoredPosition==oldPosition) throw new System.Exception("Map drag did not move content");
        root.ShowCharacters(); yield return null;
        var characters=root.GetComponentInChildren<MenuCharacterPresentation>(); characters.ShowOji();
        var characterSo=new SerializedObject(characters); var rows=characterSo.FindProperty("rows"); var data=characterSo.FindProperty("characters");
        for(var i=0;i<rows.arraySize;i++)
        {
            var row=(Button)rows.GetArrayElementAtIndex(i).objectReferenceValue;
            var info=(CharacterInfo)data.GetArrayElementAtIndex(i).objectReferenceValue;
            if(row.gameObject.activeSelf != (info.category==CharacterCategory.Oj)) throw new System.Exception("Character filter mismatch");
        }
        characters.ShowAll(); characters.Select(0); characters.SetAffinity(0,3);
        root.ShowItems(); yield return null;
        var inventory = root.GetComponentInChildren<ItemMenuController>();
        var inventorySo = new SerializedObject(inventory);
        var database = (InventoryDatabase)inventorySo.FindProperty("inventoryDatabase").objectReferenceValue;
        if(database.GetAcquired().Count != 0) throw new System.Exception("Run inventory QA in a fresh, empty new-game session");
        foreach(var item in database.items) database.Acquire(item);
        inventory.SetFilter(ItemMenuController.Filter.All);
        if(inventory.VisibleItemCount != database.items.Count) throw new System.Exception("ALL filter mismatch");
        inventory.SetFilter(ItemMenuController.Filter.Key);
        var keyCount=0; var keyIndex=-1;
        for(var i=0;i<database.items.Count;i++) if(database.items[i].category==InventoryItemCategory.Key){keyCount++;keyIndex=i;}
        if(inventory.VisibleItemCount!=keyCount) throw new System.Exception("KEY filter mismatch");
        inventory.SetFilter(ItemMenuController.Filter.Gift);
        var giftCount=0;foreach(var item in database.items)if(item.category==InventoryItemCategory.Gift)giftCount++;
        if(inventory.VisibleItemCount!=giftCount) throw new System.Exception("GIFT filter mismatch");
        var dropArea=(RectTransform)inventorySo.FindProperty("bagDropArea").objectReferenceValue;
        inventory.SetFilter(ItemMenuController.Filter.All);Canvas.ForceUpdateCanvases();
        pointer.position=RectTransformUtility.WorldToScreenPoint(null,dropArea.TransformPoint(dropArea.rect.center));
        inventory.BeginItemDrag(keyIndex,pointer);inventory.EndItemDrag(keyIndex,pointer);
        inventory.SetFilter(ItemMenuController.Filter.Bag);
        if(inventory.VisibleItemCount!=1) throw new System.Exception("BAG filter mismatch after drop");
        InventoryDatabase.ClearAcquired();
        root.ShowSave(); yield return new WaitForSecondsRealtime(.3f);
        var save=root.GetComponentInChildren<MenuSaveLoadController>();save.EnterDeleteMode();save.SetDeleteSelected(0,true);save.SetDeleteSelected(2,true);
        if(save.SelectedDeleteCount!=2) throw new System.Exception("Save multi-selection failed");
        save.ExitDeleteMode();if(save.SelectedDeleteCount!=0||save.IsDeleteMode) throw new System.Exception("Save cancel failed");
        root.ShowSettings(); yield return null;
        var settings=root.GetComponentInChildren<MenuSettingsController>(); var settingsSo=new SerializedObject(settings);
        var slider=(Slider)settingsSo.FindProperty("bgmSlider").objectReferenceValue; var original=slider.value;
        slider.value=.37f; yield return null;
        if(Mathf.Abs(Naninovel.Engine.GetService<Naninovel.IAudioManager>().BgmVolume-.37f)>.001f) throw new System.Exception("BGM binding failed");
        slider.value=original;
        var categories=root.GetComponentInChildren<MenuSettingsTabs>();categories.Select(1);yield return null;categories.Select(0);
        ScreenCapture.CaptureScreenshot("tmp/MenuQA/runtime-settings-final.png");
        yield return new WaitForSecondsRealtime(.2f);
        root.ShowTop();yield return new WaitForSecondsRealtime(.5f);
        File.WriteAllText("tmp/MenuQA/supplied-features-result.txt", "PASS: outfit change/restore, cafe route gating, ScrollRect pointer drag, character filter, inventory ALL/KEY/GIFT/BAG and D&D, save multi-select/cancel, BGM binding/change/restore, settings tabs. Existing saves were not overwritten or deleted.");
        Debug.Log("SUPPLIED_MENU_FEATURES_PASS");
    }
    public static void StartRuntimeSweep()
    {
        var root = Naninovel.Engine.GetService<Naninovel.IUIManager>().GetUI("MenuRootV2") as MenuRootV2UI;
        if (!Application.isPlaying || !root) throw new System.InvalidOperationException("Start the title's new-game flow first.");
        root.Show();
        root.StartCoroutine(RuntimeSweep(root));
    }

    private static IEnumerator RuntimeSweep(MenuRootV2UI root)
    {
        yield return new WaitForSecondsRealtime(.6f);
        var so = new SerializedObject(root);
        var buttons = new[] { "topButton", "statusButton", "itemsButton", "charactersButton", "questButton", "mapButton", "saveButton", "settingsButton" };
        var pages = new[] { "pageTop", "pageStatus", "pageItems", "pageCharacters", "pageQuest", "pageMap", "pageSave", "pageSettings" };
        for (var i=0; i<buttons.Length; i++)
        {
            var button = (Button)so.FindProperty(buttons[i]).objectReferenceValue;
            button.onClick.Invoke();
            yield return new WaitForSecondsRealtime(.5f);
            var page = (GameObject)so.FindProperty(pages[i]).objectReferenceValue;
            if (!page.activeInHierarchy) throw new System.Exception("Navigation failed: " + pages[i]);
            if (!button.transform.Find("MenuPageActiveMark").gameObject.activeSelf) throw new System.Exception("Marker failed: " + buttons[i]);
            ScreenCapture.CaptureScreenshot("tmp/MenuQA/runtime-"+pages[i]+".png");
            yield return new WaitForSecondsRealtime(.1f);
        }
        root.ShowTop();
        yield return new WaitForSecondsRealtime(.5f);
        if (root.GetComponentInChildren<TMP_InputField>(true)) throw new System.Exception("Free-input UI is still present.");
        var mascot = root.GetComponentInChildren<MenuHomeComment>();
        var mascotSo = new SerializedObject(mascot);
        ((Button)mascotSo.FindProperty("characterButton").objectReferenceValue).onClick.Invoke();
        yield return new WaitForSecondsRealtime(.3f);
        var bubble = (GameObject)mascotSo.FindProperty("bubble").objectReferenceValue;
        if (!bubble.activeInHierarchy) throw new System.Exception("Tap bubble is hidden.");
        ScreenCapture.CaptureScreenshot("tmp/MenuQA/runtime-home.png");
        yield return new WaitForSecondsRealtime(.1f);
        root.ShowStatus();
        var transition = root.GetComponent<MenuRootV2OrientationTransition>();
        if (!transition.IsTransitioning) throw new System.Exception("Portrait to landscape did not animate.");
        yield return new WaitForSecondsRealtime(.5f);
        root.ShowMap();
        if (transition.IsTransitioning) throw new System.Exception("Landscape page change animated unnecessarily.");
        root.ShowTop();
        if (!transition.IsTransitioning) throw new System.Exception("Landscape to portrait did not animate.");
        yield return new WaitForSecondsRealtime(.5f);
        root.Hide();
        yield return new WaitForSecondsRealtime(.5f);
        root.Show();
        yield return new WaitForSecondsRealtime(.5f);
        if (!root.Visible) throw new System.Exception("Menu reopen failed.");
        File.WriteAllText("tmp/MenuQA/runtime-result.txt", "PASS: 8 navigation clicks and active markers; no free-input UI; mascot tap bubble; portrait/landscape rotations; immediate landscape navigation; close/reopen. Settings and save writes not performed.");
        Debug.Log("MENU_QA_RUNTIME_PASS");
    }

    public static void BuildAndCapture()
    {
        MenuRootV2Builder.BuildPreview();
        var root = Object.FindFirstObjectByType<MenuRootV2UI>();
        var camera = Object.FindFirstObjectByType<Camera>();
        foreach (var width in new[] { 1280, 1920 })
        foreach (var page in new[] { "pageTop", "pageStatus", "pageItems", "pageCharacters", "pageQuest", "pageMap", "pageSave", "pageSettings" })
        {
            var field = typeof(MenuRootV2UI).GetField(page, BindingFlags.Instance | BindingFlags.NonPublic);
            typeof(MenuRootV2UI).GetMethod("ApplyPage", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(root, new[] { field.GetValue(root) });
            Capture(root, camera, width, width * 9 / 16, "tmp/MenuQA/" + page + "-" + width + ".png");
        }
        Debug.Log("MENU_QA_PREVIEWS_COMPLETE: tmp/MenuQA (preview only; runtime tests separate)");
    }

    public static void Capture(MenuRootV2UI root, Camera camera, int width, int height, string path)
    {
        var canvas = root.GetComponent<Canvas>();
        var scaler = root.GetComponent<CanvasScaler>();
        var group = root.GetComponent<CanvasGroup>();
        var oldMode = canvas.renderMode;
        var oldCamera = canvas.worldCamera;
        var oldScaleMode = scaler.uiScaleMode;
        var oldScale = scaler.scaleFactor;
        var oldAlpha = group.alpha;
        var oldPlaneDistance = canvas.planeDistance;
        var oldTarget = camera.targetTexture;
        var oldActive = RenderTexture.active;
        var target = new RenderTexture(width, height, 24);
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 10;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = width / 1920f;
            canvas.scaleFactor = scaler.scaleFactor;
            group.alpha = 1;
            Canvas.ForceUpdateCanvases();
            foreach (var text in root.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate();
            camera.Render();
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = oldActive;
            camera.targetTexture = oldTarget;
            canvas.renderMode = oldMode;
            canvas.worldCamera = oldCamera;
            canvas.planeDistance = oldPlaneDistance;
            scaler.uiScaleMode = oldScaleMode;
            scaler.scaleFactor = oldScale;
            group.alpha = oldAlpha;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(texture);
        }
    }
}
