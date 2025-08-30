using UnityEngine;
using UnityEngine.UI;

public class ItemCell : MonoBehaviour
{
    [SerializeField] private Image icon; // ← Inspectorでつなぐ

    // 外からアイコンを差し替える用
    public void SetIcon(Sprite sprite)
    {
        if (icon != null) icon.sprite = sprite;
    }
}