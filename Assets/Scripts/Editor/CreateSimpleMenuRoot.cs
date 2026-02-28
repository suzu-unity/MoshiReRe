#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// シンプルな MenuRoot prefab を生成するエディタスクリプト
/// Tools → Menu/Create Simple MenuRoot
/// </summary>
public class CreateSimpleMenuRoot
{
    private const string PrefabPath = "Assets/NaninovelData/Resources/UI/MenuRoot.prefab";

    [MenuItem("Tools/Menu/Create Simple MenuRoot")]
    public static void CreateMenuRoot()
    {
        // 既存prefabがあれば削除
        var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (existingPrefab != null)
        {
            AssetDatabase.DeleteAsset(PrefabPath);
            Debug.Log("[CreateSimpleMenuRoot] Deleted existing MenuRoot.prefab");
        }

        // Canvas を作成
        var canvasObj = new GameObject("MenuRoot");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);

        canvasObj.AddComponent<GraphicRaycaster>();

        var canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        // TabBar を作成
        var tabBarObj = new GameObject("TabBar");
        tabBarObj.transform.SetParent(canvasObj.transform, false);
        var tabBarImage = tabBarObj.AddComponent<Image>();
        tabBarImage.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        var tabBarRect = tabBarObj.GetComponent<RectTransform>();
        tabBarRect.anchorMin = new Vector2(0, 1);
        tabBarRect.anchorMax = new Vector2(1, 1);
        tabBarRect.offsetMin = new Vector2(0, -80);
        tabBarRect.offsetMax = Vector2.zero;

        var tabBarLayout = tabBarObj.AddComponent<HorizontalLayoutGroup>();
        tabBarLayout.childForceExpandHeight = true;
        tabBarLayout.childForceExpandWidth = true;
        tabBarLayout.spacing = 5;
        tabBarLayout.padding = new RectOffset(5, 5, 5, 5);

        // タブボタンを4個作成
        string[] tabNames = { "Status", "Items", "Characters", "Map" };
        string[] tabButtonNames = { "TabButton_Status", "TabButton_Items", "TabButton_Characters", "TabButton_Map" };
        for (int i = 0; i < 4; i++)
        {
            CreateTabButton(tabBarObj, tabButtonNames[i], tabNames[i]);
        }

        // MainContent を作成
        var mainContentObj = new GameObject("MainContent");
        mainContentObj.transform.SetParent(canvasObj.transform, false);
        var mainContentImage = mainContentObj.AddComponent<Image>();
        mainContentImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        var mainContentRect = mainContentObj.GetComponent<RectTransform>();
        mainContentRect.anchorMin = Vector2.zero;
        mainContentRect.anchorMax = Vector2.one;
        mainContentRect.offsetMin = new Vector2(0, 120);
        mainContentRect.offsetMax = new Vector2(0, -80);

        // StatusPage を作成
        var statusPageObj = new GameObject("StatusPage");
        statusPageObj.transform.SetParent(mainContentObj.transform, false);
        var statusPageRect = statusPageObj.AddComponent<RectTransform>();
        statusPageRect.anchorMin = Vector2.zero;
        statusPageRect.anchorMax = Vector2.one;
        statusPageRect.offsetMin = Vector2.zero;
        statusPageRect.offsetMax = Vector2.zero;
        statusPageObj.AddComponent<StatusPage>();
        statusPageObj.SetActive(true);

        // InventoryPage を作成
        var inventoryPageObj = new GameObject("InventoryPage");
        inventoryPageObj.transform.SetParent(mainContentObj.transform, false);
        var inventoryPageRect = inventoryPageObj.AddComponent<RectTransform>();
        inventoryPageRect.anchorMin = Vector2.zero;
        inventoryPageRect.anchorMax = Vector2.one;
        inventoryPageRect.offsetMin = Vector2.zero;
        inventoryPageRect.offsetMax = Vector2.zero;
        inventoryPageObj.AddComponent<InventoryPage>();
        inventoryPageObj.SetActive(false);

        // CharacterPage を作成
        var characterPageObj = new GameObject("CharacterPage");
        characterPageObj.transform.SetParent(mainContentObj.transform, false);
        var characterPageRect = characterPageObj.AddComponent<RectTransform>();
        characterPageRect.anchorMin = Vector2.zero;
        characterPageRect.anchorMax = Vector2.one;
        characterPageRect.offsetMin = Vector2.zero;
        characterPageRect.offsetMax = Vector2.zero;
        characterPageObj.AddComponent<CharacterPage>();
        characterPageObj.SetActive(false);

        // ReReArea を作成（空のコンテナ）
        var reReAreaObj = new GameObject("ReReArea");
        reReAreaObj.transform.SetParent(canvasObj.transform, false);
        var reReAreaImage = reReAreaObj.AddComponent<Image>();
        reReAreaImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        var reReAreaRect = reReAreaObj.GetComponent<RectTransform>();
        reReAreaRect.anchorMin = new Vector2(0, 0);
        reReAreaRect.anchorMax = new Vector2(1, 0);
        reReAreaRect.offsetMin = Vector2.zero;
        reReAreaRect.offsetMax = new Vector2(0, 120);

        // MenuRootUI をアタッチ
        var menuRootUI = canvasObj.AddComponent<MenuRootUI>();

        // hideOnLoad と visibleOnAwake を設定（リフレクション使用）
        var customUIType = typeof(MenuRootUI).BaseType; // CustomUI
        Debug.Log($"[CreateSimpleMenuRoot] CustomUI type: {customUIType?.Name}");

        var hideOnLoadField = customUIType.GetField("hideOnLoad", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var visibleOnAwakeField = customUIType.GetField("visibleOnAwake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Debug.Log($"[CreateSimpleMenuRoot] hideOnLoadField found: {hideOnLoadField != null}");
        Debug.Log($"[CreateSimpleMenuRoot] visibleOnAwakeField found: {visibleOnAwakeField != null}");

        if (hideOnLoadField != null)
        {
            hideOnLoadField.SetValue(menuRootUI, true);
            Debug.Log("[CreateSimpleMenuRoot] hideOnLoad set to true");
        }
        if (visibleOnAwakeField != null)
        {
            visibleOnAwakeField.SetValue(menuRootUI, false);
            Debug.Log("[CreateSimpleMenuRoot] visibleOnAwake set to false");
        }
        else
        {
            Debug.LogWarning("[CreateSimpleMenuRoot] visibleOnAwakeField not found - trying alternative approach");
            // 別の方法を試す - visibleOnAwakeが見つからない場合
            var allFields = customUIType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Debug.Log($"[CreateSimpleMenuRoot] Available private fields: {string.Join(", ", System.Array.ConvertAll(allFields, f => f.Name))}");
        }

        // タブボタンを割り当て（リフレクションで private フィールドに設定）
        var statusBtn = tabBarObj.transform.Find("TabButton_Status")?.GetComponent<Button>();
        var itemsBtn = tabBarObj.transform.Find("TabButton_Items")?.GetComponent<Button>();
        var charsBtn = tabBarObj.transform.Find("TabButton_Characters")?.GetComponent<Button>();
        var mapBtn = tabBarObj.transform.Find("TabButton_Map")?.GetComponent<Button>();

        var menuRootType = menuRootUI.GetType();

        // ページ参照を設定
        var pageTopField = menuRootType.GetField("pageTop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pageItemsField = menuRootType.GetField("pageItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pageCharsField = menuRootType.GetField("pageCharacters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (pageTopField != null) pageTopField.SetValue(menuRootUI, statusPageObj);
        if (pageItemsField != null) pageItemsField.SetValue(menuRootUI, inventoryPageObj);
        if (pageCharsField != null) pageCharsField.SetValue(menuRootUI, characterPageObj);

        // タブボタンにクリックリスナーを直接設定
        if (statusBtn != null)
        {
            statusBtn.onClick.RemoveAllListeners();
            statusBtn.onClick.AddListener(() => menuRootUI.ShowPageTop());
            Debug.Log("[CreateSimpleMenuRoot] Status tab button listener added");
        }
        if (itemsBtn != null)
        {
            itemsBtn.onClick.RemoveAllListeners();
            itemsBtn.onClick.AddListener(() => menuRootUI.ShowPageItems());
            Debug.Log("[CreateSimpleMenuRoot] Items tab button listener added");
        }
        if (charsBtn != null)
        {
            charsBtn.onClick.RemoveAllListeners();
            charsBtn.onClick.AddListener(() => menuRootUI.ShowPageCharacters());
            Debug.Log("[CreateSimpleMenuRoot] Characters tab button listener added");
        }
        // Map ボタンはひとまず Status に統一
        if (mapBtn != null)
        {
            mapBtn.onClick.RemoveAllListeners();
            mapBtn.onClick.AddListener(() => menuRootUI.ShowPageTop());
            Debug.Log("[CreateSimpleMenuRoot] Map tab button listener added");
        }

        // Prefab を保存
        PrefabUtility.SaveAsPrefabAsset(canvasObj, PrefabPath);
        Object.DestroyImmediate(canvasObj);

        AssetDatabase.Refresh();
        Debug.Log("[CreateSimpleMenuRoot] 新しい MenuRoot.prefab を作成しました: " + PrefabPath);
    }

    private static void CreateTabButton(GameObject parent, string buttonName, string buttonText)
    {
        var buttonObj = new GameObject(buttonName);
        buttonObj.transform.SetParent(parent.transform, false);

        var button = buttonObj.AddComponent<Button>();
        var image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        button.targetGraphic = image;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = buttonText;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 36;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}
#endif
