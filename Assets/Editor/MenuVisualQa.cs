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
        var mascot = root.GetComponentInChildren<MenuTopReReMascot>();
        var mascotSo = new SerializedObject(mascot);
        ((Button)mascotSo.FindProperty("mascotButton").objectReferenceValue).onClick.Invoke();
        yield return new WaitForSecondsRealtime(.3f);
        var bubble = (RectTransform)mascotSo.FindProperty("bubble").objectReferenceValue;
        if (!bubble.gameObject.activeInHierarchy) throw new System.Exception("Tap bubble is hidden.");
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
