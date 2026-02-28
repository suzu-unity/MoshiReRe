using UnityEngine;
using UnityEditor;

public class CheckBGNT
{
    public static void Execute()
    {
        string path = "Assets/NaninovelData/Resources/UI/MenuRoot.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        var bgnt = prefab.transform.Find("CommonBackground/BGNT");
        if (bgnt)
        {
            var rect = bgnt.GetComponent<RectTransform>();
            if (rect)
            {
                Debug.Log($"BGNT Rect: {rect.anchorMin} - {rect.anchorMax}, Size: {rect.sizeDelta}");
            }
            else
            {
                Debug.Log("BGNT has no RectTransform");
            }
        }
        else
        {
            Debug.Log("BGNT not found");
        }
    }
}
