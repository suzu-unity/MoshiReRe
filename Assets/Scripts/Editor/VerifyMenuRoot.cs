using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class VerifyMenuRoot
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

        Debug.Log($"--- Verifying {prefab.name} ---");

        // 1. Canvas Size
        var rect = prefab.GetComponent<RectTransform>();
        Debug.Log($"Canvas Size: {rect.sizeDelta} (Expected: 1080x1920)");

        // 2. Canvas Scaler
        var scaler = prefab.GetComponent<CanvasScaler>();
        if (scaler)
        {
            Debug.Log($"Canvas Scaler: Mode={scaler.uiScaleMode}, RefRes={scaler.referenceResolution}");
        }
        else
        {
            Debug.LogError("Canvas Scaler missing!");
        }

        // 3. Layout Structure
        var safeArea = prefab.transform.Find("SafeAreaLayout");
        if (safeArea)
        {
            Debug.Log("SafeAreaLayout found.");
            var vlg = safeArea.GetComponent<VerticalLayoutGroup>();
            Debug.Log($"SafeArea VLG: {(vlg ? "Found" : "Missing")}");

            var tabBar = safeArea.Find("TabBar");
            if (tabBar)
            {
                Debug.Log("TabBar found.");
                var hlg = tabBar.GetComponent<HorizontalLayoutGroup>();
                Debug.Log($"TabBar HLG: {(hlg ? "Found" : "Missing")}");
                var le = tabBar.GetComponent<LayoutElement>();
                Debug.Log($"TabBar Height: {(le ? le.preferredHeight.ToString() : "Missing LE")}");
            }
            else Debug.LogError("TabBar missing!");

            var mainContent = safeArea.Find("MainContent");
            if (mainContent)
            {
                Debug.Log("MainContent found.");
                var le = mainContent.GetComponent<LayoutElement>();
                Debug.Log($"MainContent FlexibleHeight: {(le ? le.flexibleHeight.ToString() : "Missing LE")}");
                
                // Check Pages
                var status = mainContent.GetComponentInChildren<StatusPage>(true);
                Debug.Log($"StatusPage in MainContent: {(status ? "Yes" : "No")}");
                
                var inventory = mainContent.GetComponentInChildren<InventoryPage>(true);
                Debug.Log($"InventoryPage in MainContent: {(inventory ? "Yes" : "No")}");
                
                var character = mainContent.GetComponentInChildren<CharacterPage>(true);
                Debug.Log($"CharacterPage in MainContent: {(character ? "Yes" : "No")}");
            }
            else Debug.LogError("MainContent missing!");

            var rereArea = safeArea.Find("ReReArea");
            if (rereArea)
            {
                Debug.Log("ReReArea found.");
                var le = rereArea.GetComponent<LayoutElement>();
                Debug.Log($"ReReArea Height: {(le ? le.preferredHeight.ToString() : "Missing LE")}");
            }
            else Debug.LogError("ReReArea missing!");
        }
        else Debug.LogError("SafeAreaLayout missing!");

        // 5. Important Checks
        Debug.Log($"Canvas ActiveSelf: {prefab.activeSelf}");
        
        var cg = prefab.GetComponent<CanvasGroup>();
        if (cg)
        {
            Debug.Log($"CanvasGroup Alpha: {cg.alpha}");
            if (cg.alpha == 0) Debug.LogWarning("CanvasGroup Alpha is 0! It might be invisible.");
        }
        else
        {
            Debug.Log("CanvasGroup not found (OK if not needed, but check if it was expected).");
        }

        // Check CommonBackground
        var commonBg = prefab.transform.Find("CommonBackground");
        if (commonBg)
        {
            Debug.Log($"CommonBackground exists. Active: {commonBg.gameObject.activeSelf}");
            var rectBg = commonBg.GetComponent<RectTransform>();
            if (rectBg)
            {
                Debug.Log($"CommonBackground Rect: {rectBg.anchorMin} - {rectBg.anchorMax}");
            }
            else
            {
                Debug.Log("CommonBackground has NO RectTransform.");
            }
            
            Debug.Log("CommonBackground Children:");
            foreach (Transform child in commonBg)
            {
                Debug.Log($"- {child.name}");
            }
        }
    }
}
