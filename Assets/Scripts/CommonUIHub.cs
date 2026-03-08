using UnityEngine;
using UnityEngine.SceneManagement;

public class CommonUIHub : MonoBehaviour
{
    public GameObject moneyUIPrefab;
    public GameObject reReButtonPrefab;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "CommonUIHub") return;

        SpawnPrefab(moneyUIPrefab, "moneyUIPrefab");
        SpawnPrefab(reReButtonPrefab, "reReButtonPrefab");
    }

    private void SpawnPrefab(GameObject prefab, string fieldName)
    {
        if (!prefab) return;

        var instance = Object.Instantiate((Object)prefab, transform);
        if (instance == null)
            Debug.LogWarning($"[CommonUIHub] Failed to instantiate {fieldName}.");
    }
}
