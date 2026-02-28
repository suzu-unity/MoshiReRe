using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CheckMenuRootVerticalPrefab
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
            RectTransform rootRect = prefabRoot.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                Debug.Log($"Prefab Root Anchors: Min{rootRect.anchorMin} Max{rootRect.anchorMax}");
            }

            CanvasGroup group = prefabRoot.GetComponent<CanvasGroup>();
            if (group != null)
            {
                Debug.Log($"Prefab CanvasGroup Alpha: {group.alpha}");
            }
            else
            {
                Debug.LogError("Prefab CanvasGroup component missing.");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}
