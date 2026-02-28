#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupMenuToggleInput
{
    [MenuItem("Tools/Menu/Setup MenuToggleInput in CommonUIHub")]
    public static void SetupMenuToggle()
    {
        // CommonUIHub シーンを開く
        string commonUIHubPath = "Assets/Scenes/CommonUIHub.unity";
        var scene = EditorSceneManager.OpenScene(commonUIHubPath, OpenSceneMode.Single);

        // 既存の MenuToggleInput を検索
        var existingToggle = Object.FindObjectOfType<MenuToggleInput>();
        if (existingToggle != null)
        {
            Debug.Log("[SetupMenuToggleInput] MenuToggleInput already exists in scene");
            return;
        }

        // 新しい GameObject を作成
        var menuToggleGO = new GameObject("MenuToggleInput");
        menuToggleGO.AddComponent<MenuToggleInput>();

        Debug.Log("[SetupMenuToggleInput] Created MenuToggleInput GameObject in CommonUIHub");

        // シーンを保存
        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.OpenScene("Assets/Scenes/TitleScene.unity", OpenSceneMode.Single);

        Debug.Log("[SetupMenuToggleInput] Setup complete!");
    }
}
#endif
