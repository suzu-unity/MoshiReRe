#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromTitleScene
{
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";

    static PlayFromTitleScene()
    {
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(TitleScenePath);
        if (scene == null)
        {
            Debug.LogWarning($"[PlayFromTitleScene] Scene not found: {TitleScenePath}");
            return;
        }

        EditorSceneManager.playModeStartScene = scene;
    }
}
#endif
