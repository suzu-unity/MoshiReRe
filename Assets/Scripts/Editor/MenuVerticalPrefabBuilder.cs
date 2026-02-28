#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// 縦スマホUI用プレハブの自動生成スクリプト
/// Menu: Tools → Menu/Create Vertical Prefabs
/// </summary>
public class MenuVerticalPrefabBuilder : MonoBehaviour
{
    private const string PrefabPath = "Assets/Prefabs/MenuSystem/";

    [MenuItem("Tools/Menu/Create StatusItemRow Prefab")]
    public static void CreateStatusItemRowPrefab()
    {
        // ディレクトリが存在しなければ作成
        EnsurePrefabDirectory();

        string assetPath = PrefabPath + "StatusItemRow.prefab";

        // 既に存在する場合はスキップ
        if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null)
        {
            Debug.Log("StatusItemRow.prefab は既に存在します。");
            return;
        }

        // 新規 Canvas 上に UI を構築
        var canvas = new GameObject("__TempCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        try
        {
            // StatusItemRow ボタンを作成
            var buttonObj = new GameObject("StatusItemRow");
            buttonObj.transform.SetParent(canvas.transform, false);

            var button = buttonObj.AddComponent<Button>();
            var image = buttonObj.AddComponent<Image>();
            var rectTransform = buttonObj.GetComponent<RectTransform>();

            image.color = new Color(100f / 255f, 100f / 255f, 100f / 255f, 120f / 255f);
            rectTransform.sizeDelta = new Vector2(600, 60);

            var layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60;
            layoutElement.preferredWidth = -1;

            // テキスト子要素を作成
            CreateTextChild(buttonObj, "IconText", "💪", TextAlignmentOptions.Left);
            CreateTextChild(buttonObj, "NameText", "力", TextAlignmentOptions.Center);
            CreateTextChild(buttonObj, "ValueText", "Lv.1", TextAlignmentOptions.Right);

            // StatusItemRow.cs をアタッチ
            var statusItemRow = buttonObj.AddComponent<StatusItemRow>();
            statusItemRow.iconText = buttonObj.transform.Find("IconText").GetComponent<TextMeshProUGUI>();
            statusItemRow.nameText = buttonObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            statusItemRow.valueText = buttonObj.transform.Find("ValueText").GetComponent<TextMeshProUGUI>();
            statusItemRow.layoutElement = layoutElement;

            // プレハブ化
            PrefabUtility.SaveAsPrefabAsset(buttonObj, assetPath);
            DestroyImmediate(buttonObj);
            DestroyImmediate(canvas.gameObject);

            AssetDatabase.Refresh();
            Debug.Log("StatusItemRow.prefab を作成しました。\n" + assetPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("StatusItemRow.prefab 作成エラー: " + ex.Message);
            DestroyImmediate(canvas.gameObject);
        }
    }

    [MenuItem("Tools/Menu/Create MenuRootVertical Prefab")]
    public static void CreateMenuRootVerticalPrefab()
    {
        // ディレクトリが存在しなければ作成
        EnsurePrefabDirectory();

        string assetPath = PrefabPath + "MenuRootVertical.prefab";

        if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null)
        {
            Debug.Log("MenuRootVertical.prefab は既に存在します。");
            return;
        }

        try
        {
            // Canvas を作成
            var canvasObj = new GameObject("MenuRootVertical");
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
            canvasRect.sizeDelta = new Vector2(1080, 1920);

            // SafeAreaLayout を作成
            var safeAreaObj = new GameObject("SafeAreaLayout");
            safeAreaObj.transform.SetParent(canvasObj.transform, false);
            var safeAreaRect = safeAreaObj.AddComponent<RectTransform>();
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;

            var safeAreaVLG = safeAreaObj.AddComponent<VerticalLayoutGroup>();
            safeAreaVLG.childForceExpandHeight = false;
            safeAreaVLG.childForceExpandWidth = true;

            // TabBar を作成
            var tabBarObj = new GameObject("TabBar");
            tabBarObj.transform.SetParent(safeAreaObj.transform, false);
            var tabBarImage = tabBarObj.AddComponent<Image>();
            tabBarImage.color = new Color(180f / 255f, 180f / 255f, 180f / 255f, 200f / 255f);

            var tabBarRect = tabBarObj.GetComponent<RectTransform>();
            tabBarRect.sizeDelta = new Vector2(600, 80);

            var tabBarLE = tabBarObj.AddComponent<LayoutElement>();
            tabBarLE.preferredHeight = 80;

            var tabBarHLG = tabBarObj.AddComponent<HorizontalLayoutGroup>();
            tabBarHLG.childForceExpandHeight = true;
            tabBarHLG.childForceExpandWidth = true;
            tabBarHLG.spacing = 10;

            // 4つのタブボタンを作成
            string[] tabNames = { "📊", "📦", "👥", "🗺️" };
            string[] tabButtonNames = { "TabButton_Status", "TabButton_Items", "TabButton_Characters", "TabButton_Map" };

            for (int i = 0; i < 4; i++)
            {
                CreateTabButton(tabBarObj, tabButtonNames[i], tabNames[i]);
            }

            // Content を作成
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(safeAreaObj.transform, false);
            var contentVLG = contentObj.AddComponent<VerticalLayoutGroup>();
            contentVLG.childForceExpandWidth = true;
            contentVLG.childForceExpandHeight = false;
            contentVLG.spacing = 20;

            var contentLE = contentObj.AddComponent<LayoutElement>();
            contentLE.flexibleHeight = 1;

            // StatusPageVertical を作成
            var statusPageObj = new GameObject("StatusPageVertical");
            statusPageObj.transform.SetParent(contentObj.transform, false);
            var statusPageLE = statusPageObj.AddComponent<LayoutElement>();
            statusPageLE.preferredHeight = 800;
            statusPageLE.flexibleWidth = 1;

            // Hero Portrait を作成
            var portraitObj = new GameObject("HeroPortrait");
            portraitObj.transform.SetParent(statusPageObj.transform, false);
            var portraitImage = portraitObj.AddComponent<Image>();
            portraitImage.color = Color.white;
            var portraitRect = portraitObj.GetComponent<RectTransform>();
            portraitRect.sizeDelta = new Vector2(300, 300);

            // Radar Chart Container を作成
            var radarObj = new GameObject("RadarChartContainer");
            radarObj.transform.SetParent(statusPageObj.transform, false);
            var radarImage = radarObj.AddComponent<Image>();
            radarImage.color = new Color(1, 1, 1, 0.3f);
            var radarRect = radarObj.GetComponent<RectTransform>();
            radarRect.sizeDelta = new Vector2(250, 250);

            // Radar Chart Renderer (Child)
            var radarRendererObj = new GameObject("RadarChart");
            radarRendererObj.transform.SetParent(radarObj.transform, false);
            var radarRenderer = radarRendererObj.AddComponent<RadarChartRenderer>();
            var radarRendererRect = radarRendererObj.GetComponent<RectTransform>();
            radarRendererRect.anchorMin = Vector2.zero;
            radarRendererRect.anchorMax = Vector2.one;
            radarRendererRect.offsetMin = Vector2.zero;
            radarRendererRect.offsetMax = Vector2.zero;

            // StatusItemsContainer を作成
            var containerObj = new GameObject("StatusItemsContainer");
            containerObj.transform.SetParent(statusPageObj.transform, false);
            var containerVLG = containerObj.AddComponent<VerticalLayoutGroup>();
            containerVLG.childForceExpandWidth = true;
            containerVLG.childForceExpandHeight = false;
            containerVLG.spacing = 5;

            var containerLE = containerObj.AddComponent<LayoutElement>();
            containerLE.flexibleHeight = 1;

            // StatusPageVertical.cs をアタッチ
            var statusPageComponent = statusPageObj.AddComponent<StatusPageVertical>();
            statusPageComponent.heroPortrait = portraitImage;
            statusPageComponent.radarChart = radarRenderer;
            statusPageComponent.statusItemsContainer = containerObj.transform;

            var statusItemRowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "StatusItemRow.prefab");
            if (statusItemRowPrefab)
            {
                statusPageComponent.statusItemPrefab = statusItemRowPrefab.GetComponent<StatusItemRow>();
            }

            // ReReOverlay を作成
            var rereObj = new GameObject("ReReOverlay");
            rereObj.transform.SetParent(safeAreaObj.transform, false);
            var rereImage = rereObj.AddComponent<Image>();
            rereImage.color = Color.white;
            var rereButton = rereObj.AddComponent<Button>();
            rereButton.targetGraphic = rereImage;

            var rereRect = rereObj.GetComponent<RectTransform>();
            rereRect.sizeDelta = new Vector2(120, 120);
            rereRect.anchoredPosition = new Vector2(960 - 540, 100 - 960);

            var rereLE = rereObj.AddComponent<LayoutElement>();
            rereLE.preferredHeight = 120;
            rereLE.preferredWidth = 120;

            // ReRePortrait を子要素として作成
            var portraitChildObj = new GameObject("ReRePortrait");
            portraitChildObj.transform.SetParent(rereObj.transform, false);
            var portraitChildImage = portraitChildObj.AddComponent<Image>();
            portraitChildImage.color = Color.white;
            var portraitChildRect = portraitChildObj.GetComponent<RectTransform>();
            portraitChildRect.anchorMin = Vector2.zero;
            portraitChildRect.anchorMax = Vector2.one;
            portraitChildRect.offsetMin = Vector2.zero;
            portraitChildRect.offsetMax = Vector2.zero;

            // AdviceBubbleContainer を作成
            var adviceContainerObj = new GameObject("AdviceBubbleContainer");
            adviceContainerObj.transform.SetParent(safeAreaObj.transform, false);
            var adviceVLG = adviceContainerObj.AddComponent<VerticalLayoutGroup>();
            adviceVLG.childForceExpandWidth = true;
            adviceVLG.childForceExpandHeight = false;

            // MenuRootVertical.cs をアタッチ
            var menuRootComponent = canvasObj.AddComponent<MenuRootVertical>();
            menuRootComponent.pageStatus = statusPageComponent;
            menuRootComponent.tabButtons = new Button[4]
            {
                tabBarObj.transform.Find("TabButton_Status").GetComponent<Button>(),
                tabBarObj.transform.Find("TabButton_Items").GetComponent<Button>(),
                tabBarObj.transform.Find("TabButton_Characters").GetComponent<Button>(),
                tabBarObj.transform.Find("TabButton_Map").GetComponent<Button>()
            };
            menuRootComponent.rereButton = rereButton;

            // プレハブ化
            PrefabUtility.SaveAsPrefabAsset(canvasObj, assetPath);
            DestroyImmediate(canvasObj);

            AssetDatabase.Refresh();
            Debug.Log("MenuRootVertical.prefab を作成しました。\n" + assetPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("MenuRootVertical.prefab 作成エラー: " + ex.Message);
        }
    }

    private static void CreateTextChild(GameObject parent, string name, string text, TextAlignmentOptions alignment)
    {
        var textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        var textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.alignment = alignment;
        textComponent.fontSize = 36;

        var rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(10, 0);
        rectTransform.offsetMax = new Vector2(-10, 0);
    }

    private static void CreateTabButton(GameObject parent, string buttonName, string buttonText)
    {
        var buttonObj = new GameObject(buttonName);
        buttonObj.transform.SetParent(parent.transform, false);

        var button = buttonObj.AddComponent<Button>();
        var image = buttonObj.AddComponent<Image>();
        image.color = new Color(100f / 255f, 100f / 255f, 100f / 255f, 150f / 255f);
        button.targetGraphic = image;

        // テキスト子要素
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = buttonText;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 40;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private static void EnsurePrefabDirectory()
    {
        // Assets/Prefabs が存在しなければ作成
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Assets/Prefabs/MenuSystem が存在しなければ作成
        if (!AssetDatabase.IsValidFolder(PrefabPath.TrimEnd('/')))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "MenuSystem");
        }

        AssetDatabase.Refresh();
    }
}
#endif
