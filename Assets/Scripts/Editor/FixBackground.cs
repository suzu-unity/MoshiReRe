using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixBackground
{
    public static void Execute()
    {
        string path = "Assets/NaninovelData/Resources/UI/MenuRoot.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        var commonBg = prefab.transform.Find("CommonBackground");
        if (commonBg)
        {
            // 1. Add RectTransform to CommonBackground
            var rectBg = commonBg.GetComponent<RectTransform>();
            if (!rectBg) rectBg = commonBg.gameObject.AddComponent<RectTransform>();
            
            rectBg.anchorMin = Vector2.zero;
            rectBg.anchorMax = Vector2.one;
            rectBg.offsetMin = Vector2.zero;
            rectBg.offsetMax = Vector2.zero;
            
            // 2. Fix BGNT
            var bgnt = commonBg.Find("BGNT");
            if (bgnt)
            {
                var rect = bgnt.GetComponent<RectTransform>();
                if (!rect) rect = bgnt.gameObject.AddComponent<RectTransform>();
                
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                var img = bgnt.GetComponent<Image>();
                if (!img) img = bgnt.gameObject.AddComponent<Image>();
                
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/BackGround/BGsmartphone01.png");
                if (sprite)
                {
                    img.sprite = sprite;
                    Debug.Log("Assigned BGsmartphone01 to BGNT");
                }
            }
            
            // 3. Fix MenuFrameIMG
            var frame = commonBg.Find("MenuFrameIMG");
            if (frame)
            {
                var rect = frame.GetComponent<RectTransform>();
                if (!rect) rect = frame.gameObject.AddComponent<RectTransform>();
                
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                // Ensure it's behind content?
                // CommonBackground is usually rendered first if it's first in hierarchy.
                // But SafeAreaLayout is after CommonBackground?
                // Let's check sibling index.
            }
            
            // Ensure CommonBackground is first sibling so it renders behind SafeAreaLayout
            commonBg.SetSiblingIndex(0);
        }
        
        PrefabUtility.SavePrefabAsset(prefab);
        Debug.Log("Background fixed.");
    }
}
