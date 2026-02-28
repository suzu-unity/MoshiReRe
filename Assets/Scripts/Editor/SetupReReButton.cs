#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SetupReReButton
{
    [MenuItem("Tools/ReRe/Setup ReRe Button in CommonUIHub")]
    public static void CreateReReButton()
    {
        // CommonUIHub シーンを開く
        string commonUIHubPath = "Assets/Scenes/CommonUIHub.unity";
        var scene = EditorSceneManager.OpenScene(commonUIHubPath, OpenSceneMode.Single);

        // 既存の ReReButtonCanvas を削除
        var existingReReCanvas = Object.FindObjectOfType<Canvas>();
        if (existingReReCanvas != null && existingReReCanvas.name == "ReReButtonCanvas")
        {
            Object.DestroyImmediate(existingReReCanvas.gameObject);
            Debug.Log("[SetupReReButton] Removed existing ReReButtonCanvas");
        }

        // 既存の ReReButton Canvas を検索
        var existingCanvas = Object.FindObjectOfType<Canvas>();
        Canvas targetCanvas = null;

        if (existingCanvas != null)
        {
            targetCanvas = existingCanvas;
            Debug.Log("[SetupReReButton] Using existing Canvas");
        }
        else
        {
            // 新しい Canvas を作成
            var canvasGO = new GameObject("ReReButtonCanvas");
            targetCanvas = canvasGO.AddComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("[SetupReReButton] Created new Canvas");
        }

        // ReReButton GameObject を作成
        var reReButtonGO = new GameObject("ReReButton");

        // RectTransform を追加（Canvas の子として追加する前に）
        var rectTransform = reReButtonGO.AddComponent<RectTransform>();

        // Canvas の子として設定
        reReButtonGO.transform.SetParent(targetCanvas.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(200, 200);

        // Image コンポーネントを追加
        var image = reReButtonGO.AddComponent<Image>();
        image.color = Color.white;

        // Button コンポーネントを追加
        var button = reReButtonGO.AddComponent<Button>();
        button.targetGraphic = image;

        // ReReButton スクリプトを追加
        var reReButton = reReButtonGO.AddComponent<ReReButton>();

        // AdviceBubble を探すか、作成する
        var adviceBubble = Object.FindObjectOfType<AdviceBubble>();
        if (adviceBubble == null)
        {
            // AdviceBubble.prefab から インスタンス化
            var adviceBubblePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NaninovelData/Resources/UI/AdviceBubble.prefab");
            if (adviceBubblePrefab != null)
            {
                var adviceBubbleInstance = Object.Instantiate(adviceBubblePrefab, targetCanvas.transform);
                adviceBubbleInstance.name = "AdviceBubble";
                adviceBubble = adviceBubbleInstance.GetComponent<AdviceBubble>();

                // テキストを最後に配置（Canvas描画順序で前面に来るように調整）
                var textComponent = adviceBubbleInstance.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.transform.SetAsLastSibling();
                    Debug.Log("[SetupReReButton] Text moved to last sibling for proper rendering");
                }

                Debug.Log("[SetupReReButton] AdviceBubble created from prefab");
            }
            else
            {
                Debug.LogWarning("[SetupReReButton] AdviceBubble.prefab not found!");
            }
        }

        // ReReButton に AdviceBubble を割り当て
        if (adviceBubble != null)
        {
            var serializedObject = new SerializedObject(reReButton);
            var bubbleProperty = serializedObject.FindProperty("adviceBubble");
            if (bubbleProperty != null)
            {
                bubbleProperty.objectReferenceValue = adviceBubble;
                serializedObject.ApplyModifiedProperties();
                Debug.Log("[SetupReReButton] AdviceBubble assigned to ReReButton");
                Debug.Log($"[SetupReReButton] AdviceBubble location: {adviceBubble.gameObject.name} in {adviceBubble.gameObject.transform.parent?.name ?? "root"}");
            }
            else
            {
                Debug.LogError("[SetupReReButton] adviceBubble property not found on ReReButton!");
            }
        }
        else
        {
            Debug.LogError("[SetupReReButton] AdviceBubble is null!");
        }

        Debug.Log("[SetupReReButton] ReRe Button created successfully!");
        Debug.Log("[SetupReReButton] Next steps:");
        Debug.Log("1. Assign images to the ReReButton component (stateSprites and hoverSprite)");
        Debug.Log("2. Adjust the button position as needed");

        // シーンを保存
        EditorSceneManager.SaveScene(scene);
    }
}
#endif
