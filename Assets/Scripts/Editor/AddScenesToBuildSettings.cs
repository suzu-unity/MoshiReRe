#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AddScenesToBuildSettings
{
    [MenuItem("Tools/Scene/Add Scenes to Build Settings")]
    public static void AddScenes()
    {
        // Build Settings のシーンリストを取得
        var scenes = EditorBuildSettings.scenes;

        // 追加するシーン
        string[] sceneNames = { "Assets/Scenes/TitleScene.unity", "Assets/Scenes/GameScene.unity", "Assets/Scenes/CommonUIHub.unity" };

        foreach (string scenePath in sceneNames)
        {
            // 既に存在するかチェック
            bool alreadyExists = false;
            foreach (var scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                // 新しいシーンを追加
                var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
                System.Array.Copy(scenes, newScenes, scenes.Length);
                newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = newScenes;

                Debug.Log($"[AddScenesToBuildSettings] Added scene: {scenePath}");
                scenes = EditorBuildSettings.scenes;
            }
            else
            {
                Debug.Log($"[AddScenesToBuildSettings] Scene already exists: {scenePath}");
            }
        }

        Debug.Log("[AddScenesToBuildSettings] Build Settings updated successfully");
    }
}
#endif
