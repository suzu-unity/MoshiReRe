using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class RestructureMenuRoot
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

        // 1. Setup Canvas & Scaler
        var rect = prefab.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(1080, 1920);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var scaler = prefab.GetComponent<CanvasScaler>();
        if (!scaler) scaler = prefab.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        // 2. Create Structure
        var safeArea = GetOrCreate("SafeAreaLayout", prefab.transform);
        var safeAreaRect = safeArea.GetComponent<RectTransform>();
        if (!safeAreaRect) safeAreaRect = safeArea.AddComponent<RectTransform>();
        safeAreaRect.anchorMin = Vector2.zero;
        safeAreaRect.anchorMax = Vector2.one;
        safeAreaRect.offsetMin = Vector2.zero;
        safeAreaRect.offsetMax = Vector2.zero;
        
        var safeAreaVLG = safeArea.GetComponent<VerticalLayoutGroup>();
        if (!safeAreaVLG) safeAreaVLG = safeArea.AddComponent<VerticalLayoutGroup>();
        safeAreaVLG.childForceExpandHeight = false;
        safeAreaVLG.childForceExpandWidth = true;

        var tabBar = GetOrCreate("TabBar", safeArea.transform);
        var tabBarRect = tabBar.GetComponent<RectTransform>();
        if (!tabBarRect) tabBarRect = tabBar.AddComponent<RectTransform>();
        
        var tabBarLE = tabBar.GetComponent<LayoutElement>();
        if (!tabBarLE) tabBarLE = tabBar.AddComponent<LayoutElement>();
        tabBarLE.preferredHeight = 80;
        
        var tabBarHLG = tabBar.GetComponent<HorizontalLayoutGroup>();
        if (!tabBarHLG) tabBarHLG = tabBar.AddComponent<HorizontalLayoutGroup>();
        tabBarHLG.childForceExpandHeight = true;
        tabBarHLG.childForceExpandWidth = true;

        var mainContent = GetOrCreate("MainContent", safeArea.transform);
        var mainContentRect = mainContent.GetComponent<RectTransform>();
        if (!mainContentRect) mainContentRect = mainContent.AddComponent<RectTransform>();
        
        var mainContentLE = mainContent.GetComponent<LayoutElement>();
        if (!mainContentLE) mainContentLE = mainContent.AddComponent<LayoutElement>();
        mainContentLE.flexibleHeight = 1;
        
        var mainContentVLG = mainContent.GetComponent<VerticalLayoutGroup>();
        if (!mainContentVLG) mainContentVLG = mainContent.AddComponent<VerticalLayoutGroup>();
        mainContentVLG.childForceExpandWidth = true;
        mainContentVLG.childForceExpandHeight = false;

        var rereArea = GetOrCreate("ReReArea", safeArea.transform);
        var rereAreaRect = rereArea.GetComponent<RectTransform>();
        if (!rereAreaRect) rereAreaRect = rereArea.AddComponent<RectTransform>();
        
        var rereAreaLE = rereArea.GetComponent<LayoutElement>();
        if (!rereAreaLE) rereAreaLE = rereArea.AddComponent<LayoutElement>();
        rereAreaLE.preferredHeight = 120;

        // 3. Move Existing Objects
        var commonBg = prefab.transform.Find("CommonBackground");
        var mainFrame = prefab.transform.Find("MainFrame");
        var panel = prefab.transform.Find("Panel");

        // Move Tab Buttons
        if (commonBg)
        {
            MoveChild(commonBg, "BtnToItems", tabBar.transform);
            MoveChild(commonBg, "BtnToCharacters", tabBar.transform);
            MoveChild(commonBg, "ReRePortraitButton", rereArea.transform);
        }

        // Create Status Button if missing
        if (!tabBar.transform.Find("BtnToStatus"))
        {
            var btn = new GameObject("BtnToStatus");
            btn.transform.SetParent(tabBar.transform, false);
            btn.AddComponent<RectTransform>();
            btn.AddComponent<Button>();
            btn.AddComponent<Image>().color = Color.gray;
            var txt = new GameObject("Text");
            txt.transform.SetParent(btn.transform, false);
            var txtRect = txt.AddComponent<RectTransform>();
            var tmp = txt.AddComponent<TextMeshProUGUI>();
            tmp.text = "Status";
            tmp.alignment = TextAlignmentOptions.Center;
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            
            // Move to first position
            btn.transform.SetSiblingIndex(0);
        }

        // Move Status Page
        StatusPage statusPageComp = null;
        if (panel)
        {
            var page = panel.Find("Page");
            if (page)
            {
                var pageStatus = page.Find("PageStatus");
                if (pageStatus)
                {
                    pageStatus.SetParent(mainContent.transform, false);
                    statusPageComp = pageStatus.GetComponent<StatusPage>();
                }
            }
        }

        // Create Inventory Page
        var inventoryPageObj = GetOrCreate("InventoryPage", mainContent.transform);
        if (!inventoryPageObj.GetComponent<RectTransform>()) inventoryPageObj.AddComponent<RectTransform>();
        var invPageComp = inventoryPageObj.GetComponent<InventoryPage>();
        if (!invPageComp) invPageComp = inventoryPageObj.AddComponent<InventoryPage>();
        
        if (mainFrame)
        {
            var itemGrid = mainFrame.Find("ItemGrid");
            if (itemGrid)
            {
                itemGrid.SetParent(inventoryPageObj.transform, false);
                var so = new SerializedObject(invPageComp);
                so.FindProperty("gridItemsRoot").objectReferenceValue = itemGrid;
                so.ApplyModifiedProperties();
            }
            
            var itemDetail = mainFrame.Find("ItemDetailPanel");
            if (itemDetail)
            {
                itemDetail.SetParent(inventoryPageObj.transform, false);
                var so = new SerializedObject(invPageComp);
                so.FindProperty("itemDetailPanel").objectReferenceValue = itemDetail.gameObject;
                so.FindProperty("itemDetailImage").objectReferenceValue = FindRecursive(itemDetail, "ItemDetailImage")?.GetComponent<Image>();
                so.FindProperty("itemDetailTitle").objectReferenceValue = FindRecursive(itemDetail, "ItemDetailTitle")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("itemDetailDescription").objectReferenceValue = FindRecursive(itemDetail, "ItemDetailDescription")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("itemDetailCloseButton").objectReferenceValue = FindRecursive(itemDetail, "CloseButton")?.GetComponent<Button>();
                so.ApplyModifiedProperties();
            }
        }
        
        // Assign Inventory DB
        var invDB = AssetDatabase.LoadAssetAtPath<InventoryDatabase>("Assets/Database/Items/InventoryDatabase.asset");
        if (invDB)
        {
            var so = new SerializedObject(invPageComp);
            so.FindProperty("inventoryDB").objectReferenceValue = invDB;
            so.ApplyModifiedProperties();
        }

        // Create Character Page
        var charPageObj = GetOrCreate("CharacterPage", mainContent.transform);
        if (!charPageObj.GetComponent<RectTransform>()) charPageObj.AddComponent<RectTransform>();
        var charPageComp = charPageObj.GetComponent<CharacterPage>();
        if (!charPageComp) charPageComp = charPageObj.AddComponent<CharacterPage>();
        
        if (mainFrame)
        {
            var charDetail = mainFrame.Find("CharacterDetailPanel");
            if (charDetail)
            {
                charDetail.SetParent(charPageObj.transform, false);
                var so = new SerializedObject(charPageComp);
                so.FindProperty("characterDetailPanel").objectReferenceValue = charDetail.gameObject;
                so.FindProperty("characterPortraitImage").objectReferenceValue = FindRecursive(charDetail, "CharacterPortraitImage")?.GetComponent<Image>();
                so.FindProperty("characterNameText").objectReferenceValue = FindRecursive(charDetail, "CharacterNameText")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("characterDescriptionText").objectReferenceValue = FindRecursive(charDetail, "CharacterDetailDescription")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("characterDetailCloseButton").objectReferenceValue = FindRecursive(charDetail, "CloseButton")?.GetComponent<Button>();
                so.ApplyModifiedProperties();
            }
            
            var charGrid = GetOrCreate("CharacterGrid", charPageObj.transform);
            if (!charGrid.GetComponent<RectTransform>()) charGrid.AddComponent<RectTransform>();
            if (!charGrid.GetComponent<GridLayoutGroup>()) charGrid.AddComponent<GridLayoutGroup>();
            var so2 = new SerializedObject(charPageComp);
            so2.FindProperty("gridCharactersRoot").objectReferenceValue = charGrid.transform;
            so2.ApplyModifiedProperties();
        }
        
        // Assign Character DB
        var charDB = AssetDatabase.LoadAssetAtPath<CharacterDatabase>("Assets/Database/Characters/CharacterDatabase.asset");
        if (charDB)
        {
            var so = new SerializedObject(charPageComp);
            so.FindProperty("characterDB").objectReferenceValue = charDB;
            so.ApplyModifiedProperties();
        }

        // Update MenuRootUI
        var menuRootUI = prefab.GetComponent<MenuRootUI>();
        if (menuRootUI)
        {
            var so = new SerializedObject(menuRootUI);
            so.FindProperty("pageItems").objectReferenceValue = invPageComp;
            so.FindProperty("pageCharacters").objectReferenceValue = charPageComp;
            so.FindProperty("pageStatus").objectReferenceValue = statusPageComp;
            so.ApplyModifiedProperties();
        }

        PrefabUtility.SavePrefabAsset(prefab);
        Debug.Log("MenuRoot restructured.");
    }

    static GameObject GetOrCreate(string name, Transform parent)
    {
        var child = parent.Find(name);
        if (child) return child.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    static void MoveChild(Transform parent, string name, Transform newParent)
    {
        var child = parent.Find(name);
        if (child)
        {
            child.SetParent(newParent, false);
        }
        else
        {
            Debug.LogWarning($"Child {name} not found in {parent.name}");
        }
    }

    static Transform FindRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
