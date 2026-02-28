using UnityEngine;
using UnityEditor;

public class CheckCommonBackground
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

        var commonBg = prefab.transform.Find("CommonBackground");
        if (commonBg)
        {
            Debug.Log($"CommonBackground found. Child count: {commonBg.childCount}");
            foreach (Transform child in commonBg)
            {
                Debug.Log($"- {child.name}");
            }
        }
        else
        {
            Debug.Log("CommonBackground not found");
        }
    }
}
