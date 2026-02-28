using UnityEditor;
using UnityEngine;

public class OpenPrefab
{
    public static void Execute()
    {
        string path = "Assets/NaninovelData/Resources/UI/MenuRoot_Vertical.prefab";
        Object prefab = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (prefab != null)
        {
            AssetDatabase.OpenAsset(prefab);
            Debug.Log("Opened prefab: " + path);
        }
        else
        {
            Debug.LogError("Prefab not found: " + path);
        }
    }
}
