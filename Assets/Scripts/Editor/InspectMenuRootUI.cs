using UnityEngine;
using UnityEditor;

public class InspectMenuRootUI
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

        var menuRootUI = prefab.GetComponent<MenuRootUI>();
        if (menuRootUI)
        {
            Debug.Log($"MenuRootUI found.");
            // Use reflection to access private fields if needed, or just check if they are assigned in serialized properties
            var so = new SerializedObject(menuRootUI);
            var pageItems = so.FindProperty("pageItems");
            var pageCharacters = so.FindProperty("pageCharacters");
            var pageStatus = so.FindProperty("pageStatus");

            Debug.Log($"pageItems: {pageItems.objectReferenceValue}");
            Debug.Log($"pageCharacters: {pageCharacters.objectReferenceValue}");
            Debug.Log($"pageStatus: {pageStatus.objectReferenceValue}");
        }
        else
        {
            Debug.Log("MenuRootUI component not found on root.");
        }
    }
}
