using UnityEngine;
using UnityEditor;

public class InspectTabs
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

        var tabs = prefab.transform.Find("MainFrame/Tabs");
        if (tabs)
        {
            Debug.Log($"Tabs found under MainFrame. Children:");
            foreach (Transform child in tabs)
            {
                Debug.Log($"- {child.name}");
            }
        }
        else
        {
            Debug.Log("Tabs not found under MainFrame");
        }

        var common = prefab.transform.Find("CommonBackground");
        if (common)
        {
            Debug.Log($"CommonBackground children:");
            foreach (Transform child in common)
            {
                Debug.Log($"- {child.name}");
            }
        }
    }
}
