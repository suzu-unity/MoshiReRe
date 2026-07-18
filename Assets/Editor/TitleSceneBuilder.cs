using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Rebuilds the TitleScene from code for repeatable title UI setup.</summary>
public static class TitleSceneBuilder
{
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";

    // Keep the intended source locations in one place. Missing clips are allowed
    // until the audio files are copied into Assets by the content pipeline.
    private const string BackgroundPath = "Assets/Art/Title/Title_Background_Light_1920x1080.png";
    private const string GhostPath = "Assets/Art/Title/Title_Character_Ghost_1920x1080.png";
    private const string LogoPath = "Assets/Art/Title/Title_Logo_EN_1920x1080.png";
    private const string CompactSubtitlePath = "Assets/Art/Title/Title_Subtitle_JP_Compact_1920x1080.png";
    private const string GlintPath = "Assets/Art/Title/Title_Glint.png";
    private const string LightSweepPath = "Assets/Art/Title/Title_LightSweep.png";
    private const string BgmPath = "Assets/Audio/BGM/title_bgm.mp3";
    private const string SelectSfxPath = "Assets/Audio/SFX/Title/title_cursor_move.mp3";
    private const string ConfirmSfxPath = "Assets/Audio/SFX/Title/title_confirm.mp3";

    [MenuItem("Tools/MoshiReRe/Build Title Scene")]
    public static void BuildTitleScene()
    {
        var scene = EditorSceneManager.OpenScene(TitleScenePath, OpenSceneMode.Single);
        var sceneController = Object.FindFirstObjectByType<TitleSceneController>();
        if (sceneController == null)
        {
            Debug.LogError("[TitleSceneBuilder] TitleSceneController was not found; scene was not modified.");
            return;
        }

        var cameraRoot = sceneController.gameObject;

        var presentation = cameraRoot.GetComponent<TitleScreenPresentation>();
        if (presentation == null)
            presentation = cameraRoot.AddComponent<TitleScreenPresentation>();
        presentation.RebuildPresentation();
        AssignArtwork(presentation.transform, "Background", BackgroundPath);
        AssignArtwork(presentation.transform, "GhostCharacters", GhostPath);
        AssignArtwork(presentation.transform, "TitleArtwork/EnglishLogo", LogoPath);
        AssignArtwork(presentation.transform, "TitleArtwork/CompactSubtitle", CompactSubtitlePath);
        AssignArtwork(presentation.transform, "TitleArtwork/Glint", GlintPath);
        AssignArtwork(presentation.transform, "TitleArtwork/LightSweep", LightSweepPath);

        var sfxSource = cameraRoot.AddComponent<AudioSource>();
        var bgmRoot = new GameObject("TitleBGM", typeof(AudioSource));
        bgmRoot.transform.SetParent(cameraRoot.transform, false);
        var serializedPresentation = new SerializedObject(presentation);
        serializedPresentation.FindProperty("sfxSource").objectReferenceValue = sfxSource;
        serializedPresentation.FindProperty("bgmSource").objectReferenceValue = bgmRoot.GetComponent<AudioSource>();
        serializedPresentation.FindProperty("titleBgm").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(BgmPath);
        serializedPresentation.FindProperty("selectSfx").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SelectSfxPath);
        serializedPresentation.FindProperty("confirmSfx").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(ConfirmSfxPath);
        serializedPresentation.ApplyModifiedPropertiesWithoutUndo();

        // Touch all artwork/audio paths so missing or renamed source files are
        // reported in the editor log when this builder is invoked.
        ValidateAsset(BackgroundPath);
        ValidateAsset(GhostPath);
        ValidateAsset(LogoPath);
        ValidateAsset(CompactSubtitlePath);
        ValidateAsset(GlintPath);
        ValidateAsset(LightSweepPath);
        ValidateAsset(BgmPath, false);
        ValidateAsset(SelectSfxPath, false);
        ValidateAsset(ConfirmSfxPath, false);

        EditorSceneManager.SaveScene(scene, TitleScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[TitleSceneBuilder] Built " + TitleScenePath);
    }

    private static void ValidateAsset(string path, bool required = true)
    {
        if (AssetDatabase.LoadMainAssetAtPath(path) != null || !required)
            return;

        Debug.LogWarning("[TitleSceneBuilder] Missing required title asset: " + path);
    }

    private static void AssignArtwork(Transform presentationRoot, string hierarchyPath, string assetPath)
    {
        var imageTransform = presentationRoot.Find("TitleScreenCanvas/" + hierarchyPath);
        var image = imageTransform != null ? imageTransform.GetComponent<Image>() : null;
        if (image != null)
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}
