using UnityEngine;
using UnityEditor;

public class FindMenuComponents
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

        var inventory = prefab.GetComponentInChildren<InventoryPage>(true);
        Debug.Log($"InventoryPage found on: {(inventory ? inventory.name : "null")}");

        var character = prefab.GetComponentInChildren<CharacterPage>(true);
        Debug.Log($"CharacterPage found on: {(character ? character.name : "null")}");

        var status = prefab.GetComponentInChildren<StatusPage>(true);
        Debug.Log($"StatusPage found on: {(status ? status.name : "null")}");

        var menuRootUI = prefab.GetComponent<MenuRootUI>();
        if (menuRootUI)
        {
            Debug.Log("MenuRootUI found on root");
        }
    }
}
