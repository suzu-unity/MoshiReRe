using UnityEngine;
using UnityEditor;

public class InspectItemGrid
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

        var itemGrid = prefab.transform.Find("CommonBackground/ItemGrid");
        if (itemGrid)
        {
            Debug.Log($"ItemGrid found. Components:");
            foreach (var c in itemGrid.GetComponents<Component>())
            {
                Debug.Log($"- {c.GetType().Name}");
            }
        }
        else
        {
            Debug.Log("ItemGrid not found");
        }
    }
}
