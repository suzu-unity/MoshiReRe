using UnityEngine;

public enum CharacterCategory { Oj, Itadaki, Other }

[CreateAssetMenu(menuName = "Game/Character Info")]
public class CharacterInfo : ScriptableObject
{
    public string id;
    public string displayName;
    public CharacterCategory category = CharacterCategory.Other;

    public Sprite icon;        // 一覧用の小さめアイコン
    public Sprite portrait;    // 詳細ページ用の一枚絵

    [TextArea] public string summary;     // 吹き出し用の短文
    [TextArea] public string description; // 詳細ページ用の長文
}
