using UnityEngine;
using UnityEditor;

public class FindItemGridSimple
{
    public static void Execute()
    {
        string path = "Assets/NaninovelData/Resources/UI/MenuRoot.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError("Prefab not found");
            return;
        }

        Transform itemGrid = FindRecursive(prefab.transform, "ItemGrid");
        if (itemGrid)
        {
            Debug.Log($"ItemGrid found at {GetPath(itemGrid)}");
            foreach (var c in itemGrid.GetComponents<Component>())
            {
                Debug.Log($"- {c.GetType().Name}");
            }
        }
        else
        {
            Debug.Log("ItemGrid not found recursively");
        }
    }

    static Transform FindRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }

    static string GetPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetPath(t.parent) + "/" + t.name;
    }
}
