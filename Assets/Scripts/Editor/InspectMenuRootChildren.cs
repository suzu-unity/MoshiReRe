using UnityEngine;
using UnityEditor;

public class InspectMenuRootChildren
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
        foreach (Transform child in prefab.transform)
        {
            Debug.Log($"- {child.name}");
        }
    }
}
