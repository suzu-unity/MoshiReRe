using UnityEngine;
using UnityEditor;

public class FindPageStatus
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

        Transform t = FindRecursive(prefab.transform, "PageStatus");
        if (t)
        {
            Debug.Log($"PageStatus found at {GetPath(t)}");
        }
        else
        {
            Debug.Log("PageStatus not found");
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
