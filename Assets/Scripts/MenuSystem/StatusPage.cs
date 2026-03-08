using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusPage : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RadarChart radarChart;
    [SerializeField] private Image protagonistPortrait;
    [SerializeField] private TMP_Text gutsText;
    [SerializeField] private TMP_Text intelligenceText;
    [SerializeField] private TMP_Text attentionText;
    [SerializeField] private TMP_Text techniqueText;
    [SerializeField] private TMP_Text strengthText;

    private void OnEnable()
    {
        UpdateUI();
        if (StatusManager.Instance != null)
            StatusManager.Instance.OnStatusChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (StatusManager.Instance != null)
            StatusManager.Instance.OnStatusChanged -= UpdateUI;
    }

    public void Configure(
        RadarChart chart,
        Image portrait,
        TMP_Text guts,
        TMP_Text intelligence,
        TMP_Text attention,
        TMP_Text technique,
        TMP_Text strength)
    {
        radarChart = chart;
        protagonistPortrait = portrait;
        gutsText = guts;
        intelligenceText = intelligence;
        attentionText = attention;
        techniqueText = technique;
        strengthText = strength;
    }

    public void SetPortrait(Sprite sprite)
    {
        if (protagonistPortrait)
            protagonistPortrait.sprite = sprite;
    }

    private void UpdateUI()
    {
        if (StatusManager.Instance == null) return;

        int guts = StatusManager.Instance.Guts;
        int intelligence = StatusManager.Instance.Intelligence;
        int attention = StatusManager.Instance.Attention;
        int technique = StatusManager.Instance.Technique;
        int strength = StatusManager.Instance.Strength;

        if (radarChart)
        {
            radarChart.SetValues(guts, intelligence, attention, technique, strength);
            radarChart.GenerateMesh();
        }

        if (gutsText) gutsText.text = $"閭・鴨: {guts}";
        if (intelligenceText) intelligenceText.text = $"遏･蜉・ {intelligence}";
        if (attentionText) attentionText.text = $"豕ｨ諢丞鴨: {attention}";
        if (techniqueText) techniqueText.text = $"謚陦灘鴨: {technique}";
        if (strengthText) strengthText.text = $"遲句鴨: {strength}";
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
