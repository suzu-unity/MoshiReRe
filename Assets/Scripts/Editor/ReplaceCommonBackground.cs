using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ReplaceCommonBackground
{
    public static void Execute()
    {
        string path = "Assets/NaninovelData/Resources/UI/MenuRoot.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        var oldBg = prefab.transform.Find("CommonBackground");
        if (oldBg)
        {
            // Check if it already has RectTransform
            if (oldBg.GetComponent<RectTransform>())
            {
                Debug.Log("CommonBackground already has RectTransform.");
                return;
            }

            Debug.Log("Replacing CommonBackground with RectTransform version...");
            
            var newBg = new GameObject("CommonBackground");
            newBg.AddComponent<RectTransform>();
            newBg.transform.SetParent(prefab.transform, false);
            
            // Move children
            var children = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in oldBg) children.Add(child);
            
            foreach (var child in children)
            {
                child.SetParent(newBg.transform, false);
            }
            
            // Destroy old
            Object.DestroyImmediate(oldBg.gameObject, true);
            
            // Setup new RectTransform
            var rect = newBg.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Ensure sibling index
            newBg.transform.SetSiblingIndex(0);
            
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log("CommonBackground replaced.");
        }
    }
}
