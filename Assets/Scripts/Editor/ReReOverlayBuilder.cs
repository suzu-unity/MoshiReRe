using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// メニュー: MoshiReRe > Build ReReOverlay Prefab
/// ReReOverlay.prefab を Assets/NaninovelData/Resources/UI/ に生成する。
/// </summary>
public static class ReReOverlayBuilder
{
    private const string SavePath = "Assets/NaninovelData/Resources/UI/ReReOverlay.prefab";

    [MenuItem("MoshiReRe/Build ReReOverlay Prefab")]
    public static void Build()
    {
        // ── フォント読み込み ──────────────────────────────────────────
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Font/PixelMplus12-Regular SDF.asset");
        if (font == null)
            Debug.LogWarning("[ReReOverlayBuilder] PixelMplus12 フォントが見つかりません。TMP テキストはデフォルトフォントを使います。");

        // ── ルート ────────────────────────────────────────────────────
        var root = new GameObject("ReReOverlay");

        // Canvas が必須（Naninovel の container は素の GameObject なので自前で用意）
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400; // MenuRoot(500) より後ろに配置

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();

        var rootCG = root.AddComponent<CanvasGroup>();
        rootCG.alpha = 1f;

        var overlay = root.AddComponent<ReReOverlay>();

        // ── ReReButton（仮置き四角） ──────────────────────────────────
        var btnGO = new GameObject("ReReButton");
        btnGO.transform.SetParent(root.transform, false);

        var btnRect = btnGO.AddComponent<RectTransform>();
        // テキストウィンドウ上縁に座るイメージ。Y は後で調整。
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot     = new Vector2(0.5f, 0f);
        btnRect.anchoredPosition = new Vector2(-400f, 160f); // ← 左寄りに仮置き
        btnRect.sizeDelta = new Vector2(80f, 100f);

        var btnImage = btnGO.AddComponent<Image>();
        btnImage.color = new Color(0.9f, 0.4f, 0.7f, 1f); // 仮置きピンク

        var btn = btnGO.AddComponent<Button>();

        // ── AdviceBubble コンテナ ─────────────────────────────────────
        var bubbleGO = new GameObject("AdviceBubble");
        bubbleGO.transform.SetParent(root.transform, false);

        var bubbleRect = bubbleGO.AddComponent<RectTransform>();
        // ボタンの上に吹き出しを出す
        bubbleRect.anchorMin = new Vector2(0.5f, 0f);
        bubbleRect.anchorMax = new Vector2(0.5f, 0f);
        bubbleRect.pivot     = new Vector2(0.5f, 0f);
        bubbleRect.anchoredPosition = new Vector2(-260f, 275f);
        bubbleRect.sizeDelta = new Vector2(300f, 90f);

        var bubbleCG = bubbleGO.AddComponent<CanvasGroup>();
        bubbleCG.alpha = 0f; // 最初は非表示

        var bubbleBG = bubbleGO.AddComponent<Image>();
        bubbleBG.color = new Color(1f, 1f, 1f, 1f); // 白（既存AdviceBubble.prefabに合わせ）

        var adviceBubble = bubbleGO.AddComponent<AdviceBubble>();

        // ── BubbleText ────────────────────────────────────────────────
        var textGO = new GameObject("BubbleText");
        textGO.transform.SetParent(bubbleGO.transform, false);

        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 12f);
        textRect.offsetMax = new Vector2(-12f, -12f);

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "サンプルアドバイス";
        tmp.fontSize = 24f; // 既存AdviceBubble.prefabに合わせ
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.enableWordWrapping = true;
        if (font != null) tmp.font = font;

        // ── Inspector 参照をセット ────────────────────────────────────
        var so = new SerializedObject(overlay);
        so.FindProperty("rereButton").objectReferenceValue   = btn;
        so.FindProperty("adviceBubble").objectReferenceValue = adviceBubble;
        // タイトル画面では非表示。.naniスクリプトから @showUI ReReOverlay で表示する
        var visibleProp = so.FindProperty("visibleOnAwake");
        if (visibleProp != null) visibleProp.boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();

        // AdviceBubble の bubbleRoot / canvasGroup / text を自動解決させるため
        // Awake が走る前でも SerializedObject でセット
        var abSo = new SerializedObject(adviceBubble);
        abSo.FindProperty("canvasGroup").objectReferenceValue = bubbleCG;
        abSo.FindProperty("bubbleRoot").objectReferenceValue  = bubbleRect;
        abSo.FindProperty("text").objectReferenceValue        = tmp;
        abSo.ApplyModifiedPropertiesWithoutUndo();

        // ── プレハブ保存（既存を上書き） ──────────────────────────────
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(SavePath);
        if (existing != null) AssetDatabase.DeleteAsset(SavePath);

        bool success;
        PrefabUtility.SaveAsPrefabAsset(root, SavePath, out success);
        Object.DestroyImmediate(root);

        if (success)
            Debug.Log($"[ReReOverlayBuilder] プレハブを作成しました: {SavePath}");
        else
            Debug.LogError($"[ReReOverlayBuilder] プレハブの保存に失敗しました: {SavePath}");

        AssetDatabase.Refresh();
    }
}
