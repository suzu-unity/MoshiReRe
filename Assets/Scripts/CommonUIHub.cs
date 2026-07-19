using UnityEngine;
using UnityEngine.SceneManagement;

public class CommonUIHub : MonoBehaviour
{
    public GameObject moneyUIPrefab;
    public GameObject reReButtonPrefab;
    [SerializeField] private bool showReReButton;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "CommonUIHub") return;

        SpawnPrefab(moneyUIPrefab, "moneyUIPrefab");
        if (showReReButton)
            SpawnPrefab(reReButtonPrefab, "reReButtonPrefab");
        EnsureComicDemoOverlay();
    }

    private void SpawnPrefab(GameObject prefab, string fieldName)
    {
        if (!prefab) return;

        var instance = Object.Instantiate((Object)prefab, transform);
        if (instance == null)
            Debug.LogWarning($"[CommonUIHub] Failed to instantiate {fieldName}.");
    }

    private void EnsureComicDemoOverlay()
    {
        if (ComicDemoOverlayController.Instance) return;

        var overlay = new GameObject("ComicDemoOverlay");
        overlay.transform.SetParent(transform, false);
        overlay.AddComponent<ComicDemoOverlayController>();
    }
}
