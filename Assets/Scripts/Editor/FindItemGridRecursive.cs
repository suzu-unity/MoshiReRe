using UnityEngine;
using UnityEditor;

public class FindItemGridRecursive
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

        var itemGrid = prefab.transform.FindRecursive("ItemGrid");
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

    static string GetPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetPath(t.parent) + "/" + t.name;
    }
}

public static class TransformExtensions
{
    public static Transform FindRecursive(this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = child.FindRecursive(name);
            if (result != null) return result;
        }
        return null;
    }
}
