using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class InspectMenuPrefabs
{
    public static void Execute()
    {
        string[] paths = {
            "Assets/NaninovelData/Resources/UI/MenuRoot.prefab",
            "Assets/NaninovelData/Resources/UI/MenuRoot_Vertical.prefab"
        };

        foreach (var path in paths)
        {
            Debug.Log($"--- Inspecting {path} ---");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found at {path}");
                continue;
            }

            var rect = prefab.GetComponent<RectTransform>();
            if (rect != null)
            {
                Debug.Log($"RectTransform: SizeDelta={rect.sizeDelta}, AnchorMin={rect.anchorMin}, AnchorMax={rect.anchorMax}");
            }

            var scaler = prefab.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                Debug.Log($"CanvasScaler: ScaleMode={scaler.uiScaleMode}, RefRes={scaler.referenceResolution}");
            }

            var vlg = prefab.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                Debug.Log($"VerticalLayoutGroup: Found");
            }

            // Check children
            foreach (Transform child in prefab.transform)
            {
                var childRect = child.GetComponent<RectTransform>();
                if (childRect != null)
                {
                    Debug.Log($"Child: {child.name}, Rect: {childRect.sizeDelta}");
                }
                else
                {
                    Debug.Log($"Child: {child.name}, No RectTransform");
                }
            }
        }
    }
}
