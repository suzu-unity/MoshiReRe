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
        if (StatusManager.Instance == null) return;

        int intel = StatusManager.Instance.Intelligence;
        int courage = StatusManager.Instance.Courage;
        int strength = StatusManager.Instance.Strength;

        if (radarChart)
        {
            radarChart.SetValues(intel, courage, strength);
        }

        if (intelligenceText) intelligenceText.text = $"Intelligence: {intel}";
        if (courageText) courageText.text = $"Courage: {courage}";
        if (strengthText) strengthText.text = $"Strength: {strength}";
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
