using UnityEngine;

[CreateAssetMenu(menuName = "Game/Inventory Item")]
public class InventoryItem : ScriptableObject {
    public string id;
    public Sprite icon;
    [TextArea] public string summary;     // 吹き出し用の短い説明
    [TextArea] public string description; // 詳細ページ用
    public Sprite detailImage;
}
