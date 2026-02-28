using UnityEngine;
using UnityEditor;

public class CheckLayoutChildren
{
    public static void Execute()
    {
        Debug.Log("CheckLayoutChildren.Execute started.");
        string path = "Assets/NaninovelData/Resources/UI/MenuRoot.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError("Prefab not found");
            return;
        }

        var safeArea = prefab.transform.Find("SafeAreaLayout");
        if (safeArea)
        {
            Debug.Log("SafeAreaLayout found.");
            var tabBar = safeArea.Find("TabBar");
            if (tabBar)
            {
                Debug.Log($"TabBar Children ({tabBar.childCount}):");
                foreach (Transform child in tabBar) Debug.Log($"- {child.name}");
            }
            else Debug.LogError("TabBar not found");
            
            var rereArea = safeArea.Find("ReReArea");
            if (rereArea)
            {
                Debug.Log($"ReReArea Children ({rereArea.childCount}):");
                foreach (Transform child in rereArea) Debug.Log($"- {child.name}");
            }
            else Debug.LogError("ReReArea not found");
        }
        else Debug.LogError("SafeAreaLayout not found");
    }
}
