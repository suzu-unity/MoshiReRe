using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MenuInformationNodeCard : MonoBehaviour
{
    [SerializeField] private Image frame;
    [SerializeField] private Sprite[] categoryFrames;
    [SerializeField] private TMP_Text category;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text content;
    [SerializeField] private GameObject unknown;

    public void Bind(CharacterInformationNodeState.NodeView node)
    {
        if (category) category.text = CharacterInformationNodePanel.GetCategoryLabel(node.Category);
        if (title) title.text = node.Title;
        if (content) { content.text = node.IsHidden ? "" : node.Content; content.gameObject.SetActive(!node.IsHidden); }
        if (unknown) unknown.SetActive(node.IsHidden);
        var index = node.Category == CharacterInformationNodeCategory.SelfImage ? 1 : node.Category == CharacterInformationNodeCategory.Desire ? 2 : node.Category == CharacterInformationNodeCategory.Risk ? 3 : 0;
        if (frame && categoryFrames != null && index < categoryFrames.Length) frame.sprite = categoryFrames[index];
    }
}
