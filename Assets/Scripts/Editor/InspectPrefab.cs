using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class InspectPrefab
{
    public static void Execute()
    {
        GameObject root = GameObject.Find("MenuRoot_Vertical");
        if (root == null)
        {
            Debug.LogError("Root object not found");
            return;
        }

        Debug.Log("Root: " + root.name);
        foreach (Component c in root.GetComponents<Component>())
        {
            Debug.Log(" - Component: " + c.GetType().Name);
        }

        foreach (Transform t in root.transform)
        {
            Debug.Log("Child: " + t.name);
            foreach (Component c in t.GetComponents<Component>())
            {
                Debug.Log("   - Component: " + c.GetType().Name);
            }
        }
    }
}
