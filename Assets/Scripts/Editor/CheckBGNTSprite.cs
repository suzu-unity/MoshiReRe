using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CheckBGNTSprite
{
    public static void Execute()
    {
        string path = "Assets/NaninovelData/Resources/UI/MenuRoot.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        var bgnt = prefab.transform.Find("CommonBackground/BGNT");
        if (bgnt)
        {
            var img = bgnt.GetComponent<Image>();
            if (img && img.sprite)
            {
                Debug.Log($"BGNT Sprite: {img.sprite.name}, Size: {img.sprite.rect.width}x{img.sprite.rect.height}");
            }
            else
            {
                Debug.Log("BGNT has no Image or Sprite");
            }
        }
    }
}
