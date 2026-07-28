using UnityEngine;
using TMPro;

public class LocationHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    private void Awake()
    {
        ResolveLabel();
        ApplyText(LocationHUDState.Current);
    }

    private void OnEnable()
    {
        LocationHUDState.OnChanged += ApplyText;
        ApplyText(LocationHUDState.Current);
    }

    private void OnDisable() => LocationHUDState.OnChanged -= ApplyText;

    /// <summary>Updates the shared location value used by the HUD.</summary>
    public void SetText(string text) => LocationHUDState.SetCurrent(text);

    private void ApplyText(string text)
    {
        if (label) label.text = text ?? string.Empty;
    }

    private void ResolveLabel()
    {
        if (label) return;

        foreach (var candidate in GetComponentsInChildren<TextMeshProUGUI>(true))
            if (candidate.name == "LocationText")
            {
                label = candidate;
                return;
            }
    }
}
