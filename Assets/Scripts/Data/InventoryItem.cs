using UnityEngine;

[CreateAssetMenu(menuName = "Game/Inventory Item")]
public class InventoryItem : ScriptableObject {
    public string id;
    [Tooltip("UI display label. Falls back to id, then the asset name when empty.")]
    public string displayName;
    public Sprite icon;
    [TextArea] public string summary;     // 吹き出し用の短い説明
    [TextArea] public string description; // 詳細ページ用
    public Sprite detailImage;

    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(displayName)) return displayName;
        if (!string.IsNullOrWhiteSpace(id)) return id;
        return name;
    }
}
