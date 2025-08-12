using UnityEngine;
using TMPro;

public class LocationHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    private void Awake ()
    {
        if (label) label.text = ""; // 初期は空
    }

    public void SetText (string text)
    {
        if (label) label.text = text;
    }
}