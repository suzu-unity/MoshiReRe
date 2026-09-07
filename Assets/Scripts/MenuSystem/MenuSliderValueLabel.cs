using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays a slider's normalized value as a compact percentage.</summary>
public sealed class MenuSliderValueLabel : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI label;
    private bool listening;
    private int lastPercent = -1;

    public void Configure(Slider source, TextMeshProUGUI target)
    {
        Unbind();
        slider = source;
        label = target;
        lastPercent = -1;
        Bind();
        Refresh();
    }

    private void OnEnable()
    {
        Bind();
        Refresh();
    }

    private void OnDisable() => Unbind();

    private void OnDestroy() => Unbind();

    private void LateUpdate() => Refresh();

    private void Bind()
    {
        if (!listening && slider)
        {
            slider.onValueChanged.AddListener(UpdateLabel);
            listening = true;
        }
    }

    private void Unbind()
    {
        if (listening && slider)
            slider.onValueChanged.RemoveListener(UpdateLabel);
        listening = false;
    }

    private void Refresh()
    {
        if (slider) UpdateLabel(slider.value);
    }

    private void UpdateLabel(float value)
    {
        if (label && slider)
        {
            var normalized = Mathf.Approximately(slider.minValue, slider.maxValue)
                ? 0f : Mathf.InverseLerp(slider.minValue, slider.maxValue, value);
            var percent = Mathf.RoundToInt(Mathf.Clamp01(normalized) * 100f);
            if (percent != lastPercent)
            {
                label.text = percent + "%";
                lastPercent = percent;
            }
        }
    }
}
