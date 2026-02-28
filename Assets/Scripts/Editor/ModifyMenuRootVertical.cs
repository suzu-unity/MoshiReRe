using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ModifyMenuRootVertical
{
    public static void Execute()
    {
        string prefabPath = "Assets/Prefabs/MenuSystem/MenuRootVertical.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        if (prefabRoot == null)
        {
            Debug.LogError($"Prefab not found at {prefabPath}");
            return;
        }

        try
        {
            // 1. Modify Canvas
            Canvas canvas = prefabRoot.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = prefabRoot.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            Debug.Log("Modified Canvas: RenderMode=ScreenSpaceOverlay, PixelPerfect=false");

            // 2. Modify RectTransform of Root
            RectTransform rootRect = prefabRoot.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
                Debug.Log("Modified Root RectTransform: Anchors(0,0)-(1,1), Offsets(0,0)");
            }

            // 3. Modify CanvasGroup
            CanvasGroup canvasGroup = prefabRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = prefabRoot.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Debug.Log("Modified CanvasGroup: Alpha=1, Interactable=true, BlocksRaycasts=true");

            // 4. Modify SafeAreaLayout
            Transform safeAreaTransform = prefabRoot.transform.Find("SafeAreaLayout");
            if (safeAreaTransform != null)
            {
                RectTransform safeAreaRect = safeAreaTransform.GetComponent<RectTransform>();
                if (safeAreaRect != null)
                {
                    safeAreaRect.anchorMin = Vector2.zero;
                    safeAreaRect.anchorMax = Vector2.one;
                    safeAreaRect.offsetMin = Vector2.zero;
                    safeAreaRect.offsetMax = Vector2.zero;
                    Debug.Log("Modified SafeAreaLayout RectTransform: Anchors(0,0)-(1,1), Offsets(0,0)");
                }
            }
            else
            {
                Debug.LogWarning("SafeAreaLayout child not found!");
            }

            // Save changes
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            Debug.Log("Prefab saved successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error modifying prefab: {e.Message}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}
