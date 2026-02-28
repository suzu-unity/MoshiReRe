using UnityEngine;
using UnityEditor;

public class InspectMainFrame
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

        var mainFrame = prefab.transform.Find("MainFrame");
        if (mainFrame)
        {
            Debug.Log($"MainFrame found. Children:");
            foreach (Transform child in mainFrame)
            {
                Debug.Log($"- {child.name}");
            }
        }
        else
        {
            Debug.Log("MainFrame not found");
        }
    }
}
