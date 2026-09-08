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
    [SerializeField] private Image illustration;

    public void Bind(CharacterInformationNodeState.NodeView node)
    {
        if (category) category.text = CharacterInformationNodePanel.GetCategoryLabel(node.Category);
        if (title) title.text = node.Title;
        var hasImage = !node.IsHidden && node.Image;
        if (illustration) { illustration.sprite=node.Image; illustration.gameObject.SetActive(hasImage); }
        if (content)
        {
            content.text = node.IsHidden ? "" : node.Content;
            content.gameObject.SetActive(!node.IsHidden);
            content.rectTransform.anchoredPosition = new Vector2(hasImage ? 119 : 28, -143);
            content.rectTransform.sizeDelta = new Vector2(hasImage ? 136 : 227, 96);
        }
        if (unknown) unknown.SetActive(node.IsHidden);
        var index = node.Category == CharacterInformationNodeCategory.SelfImage ? 1 : node.Category == CharacterInformationNodeCategory.Desire ? 2 : node.Category == CharacterInformationNodeCategory.Risk ? 3 : 0;
        if (frame && categoryFrames != null && index < categoryFrames.Length) frame.sprite = categoryFrames[index];
    }
}
