using UnityEngine;
using UnityEditor;

public class InspectMenuDeep
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

        Debug.Log($"Root: {prefab.name}");
        PrintChildren(prefab.transform, "");
    }

    static void PrintChildren(Transform t, string indent)
    {
        foreach (Transform child in t)
        {
            Debug.Log($"{indent}- {child.name}");
            PrintChildren(child, indent + "  ");
        }
    }
}
