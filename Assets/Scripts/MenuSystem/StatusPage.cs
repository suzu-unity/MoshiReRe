using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusPage : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RadarChart radarChart;
    [SerializeField] private TMP_Text intelligenceText;
    [SerializeField] private TMP_Text courageText;
    [SerializeField] private TMP_Text strengthText;

    private void Start()
    {
        if (!radarChart) Debug.LogError("[StatusPage] Radar Chart is not assigned!");
        if (StatusManager.Instance == null) Debug.LogError("[StatusPage] StatusManager Instance is null! Make sure StatusManager is in the scene.");
    }

    private void OnEnable()
    {
        UpdateUI();
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.OnStatusChanged += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.OnStatusChanged -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        if (StatusManager.Instance == null) 
        {
            Debug.LogError("[StatusPage] StatusManager.Instance is null!");
            return;
        }

        int intel = StatusManager.Instance.Intelligence;
        int courage = StatusManager.Instance.Courage;
        int strength = StatusManager.Instance.Strength;
        
        Debug.Log($"[StatusPage] UpdateUI called. Stats: I={intel}, C={courage}, S={strength}. RadarChart assigned? {radarChart != null}");

        if (radarChart)
        {
            radarChart.SetValues(intel, courage, strength);
            // レイアウトが確定していない可能性があるため、強制的に更新
            Canvas.ForceUpdateCanvases();
            radarChart.GenerateMesh();
        }
        else
        {
            Debug.LogError("[StatusPage] RadarChart is NOT assigned!");
        }

        if (intelligenceText) intelligenceText.text = $"Intelligence: {intel}";
        if (courageText) courageText.text = $"Courage: {courage}";
        if (strengthText) strengthText.text = $"Strength: {strength}";
    }

    public void Show()
    {
        Debug.Log("[StatusPage] Show() called.");
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
