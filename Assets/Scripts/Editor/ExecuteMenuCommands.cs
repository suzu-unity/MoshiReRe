using UnityEditor;
using UnityEngine;

public class ExecuteMenuCommands
{
    public static void Execute()
    {
        Debug.Log("Starting prefab generation...");
        MenuVerticalPrefabBuilder.CreateStatusItemRowPrefab();
        MenuVerticalPrefabBuilder.CreateMenuRootVerticalPrefab();
        Debug.Log("Prefab generation completed.");
    }
}
