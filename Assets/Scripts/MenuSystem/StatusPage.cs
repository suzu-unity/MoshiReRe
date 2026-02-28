using UnityEngine;

public class StatusPage : MonoBehaviour, IMenuPage
{
    [SerializeField] private RadarChartRenderer radarChart;

    private static readonly float[] DefaultValues = { 2f, 4f, 6f, 1f, 5f };

    private void Start()
    {
        ApplyValues(DefaultValues);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ApplyValues(DefaultValues);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ApplyValues(float[] vals)
    {
        if (radarChart != null)
            radarChart.SetValues(vals);
    }
}
