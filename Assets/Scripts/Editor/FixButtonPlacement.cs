using UnityEngine;
using UnityEditor;

public class FixButtonPlacement
{
    public static void Execute()
    {
        Debug.Log("FixButtonPlacement.Execute started.");
        string path = "Assets/NaninovelData/Resources/UI/MenuRoot.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError("Prefab not found at " + path);
            return;
        }
        Debug.Log("Prefab loaded: " + prefab.name);

        var safeArea = prefab.transform.Find("SafeAreaLayout");
        if (!safeArea)
        {
            Debug.LogError("SafeAreaLayout not found");
            return;
        }
        Debug.Log("SafeAreaLayout found.");

        var tabBar = safeArea.Find("TabBar");
        if (!tabBar) Debug.LogError("TabBar not found");
        else Debug.Log("TabBar found.");

        var rereArea = safeArea.Find("ReReArea");
        if (!rereArea) Debug.LogError("ReReArea not found");
        else Debug.Log("ReReArea found.");

        var commonBg = prefab.transform.Find("CommonBackground");
        if (commonBg)
        {
            Debug.Log("CommonBackground found. Moving buttons...");
            Move(commonBg, "BtnToItems", tabBar);
            Move(commonBg, "BtnToCharacters", tabBar);
            Move(commonBg, "ReRePortraitButton", rereArea);
        }
        else
        {
            Debug.LogError("CommonBackground not found");
        }

        PrefabUtility.SavePrefabAsset(prefab);
        Debug.Log("Button placement fixed.");
    }

    static void Move(Transform parent, string name, Transform newParent)
    {
        if (!newParent) return;
        var child = parent.Find(name);
        if (child)
        {
            child.SetParent(newParent, false);
            Debug.Log($"Moved {name} to {newParent.name}");
        }
        else
        {
            Debug.LogWarning($"Could not find {name} in {parent.name}");
        }
    }
}
