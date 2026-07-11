using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class MenuLayoutPreviewTools
{
    private const string PreviewScenePath = "Assets/Scenes/MenuLayoutPreview.unity";
    private const string MenuRootPrefabPath = "Assets/NaninovelData/Resources/UI/MenuRoot.prefab";
    private const string PreviewMenuName = "MenuRootPreview";

    [MenuItem("Tools/MoshiReRe/Open Menu Layout Preview")]
    public static void OpenMenuLayoutPreview()
    {
        var scene = EnsurePreviewScene();
        EnsurePreviewObjects(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static Scene EnsurePreviewScene()
    {
        if (File.Exists(PreviewScenePath))
            return EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Single);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, PreviewScenePath);
        return scene;
    }

    private static void EnsurePreviewObjects(Scene scene)
    {
        CleanupLegacyPreviewCanvas();

        if (!Object.FindFirstObjectByType<EventSystem>())
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
        }

        var menuPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MenuRootPrefabPath);
        if (!menuPrefab)
        {
            Debug.LogError($"MenuRoot prefab not found: {MenuRootPrefabPath}");
            return;
        }

        var menuPreview = GameObject.Find(PreviewMenuName);
        if (!menuPreview)
        {
            menuPreview = PrefabUtility.InstantiatePrefab(menuPrefab) as GameObject;
            if (!menuPreview) return;
            SceneManager.MoveGameObjectToScene(menuPreview, scene);
            menuPreview.name = PreviewMenuName;
        }

        NormalizeForPreview(menuPreview);

        var menuRootUI = menuPreview.GetComponent<MenuRootUI>();
        if (menuRootUI)
        {
            menuRootUI.Show();
            menuRootUI.ShowPageStatus();
        }

        Selection.activeObject = menuPreview;
        EditorGUIUtility.PingObject(menuPreview);
    }

    private static void CleanupLegacyPreviewCanvas()
    {
        var canvas = GameObject.Find("MenuPreviewCanvas");
        if (canvas)
            Object.DestroyImmediate(canvas);
    }

    private static void NormalizeForPreview(GameObject root)
    {
        root.SetActive(true);

        var rt = root.transform as RectTransform;
        if (rt)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(900f, 1600f);
            rt.localScale = Vector3.one;
        }

        var rects = root.GetComponentsInChildren<RectTransform>(true);
        foreach (var childRt in rects)
        {
            if (childRt.localScale == Vector3.zero)
                childRt.localScale = Vector3.one;
        }

        var groups = root.GetComponentsInChildren<CanvasGroup>(true);
        foreach (var group in groups)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
            group.ignoreParentGroups = false;
        }
    }
}
