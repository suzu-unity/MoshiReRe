using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterMenuController : MonoBehaviour
{
    [SerializeField] private Button contactsTabButton;
    [SerializeField] private Button relationTabButton;
    [SerializeField] private Button ojiTabButton;
    [SerializeField] private Button itadakiTabButton;
    [SerializeField] private GameObject contactsPage;
    [SerializeField] private GameObject relationPage;
    [SerializeField] private GameObject ojiListRoot;
    [SerializeField] private GameObject itadakiListRoot;
    [SerializeField] private RectTransform relationHintBubble;
    [SerializeField] private TMP_Text relationHintText;

    private void Awake()
    {
        Bind();
        ShowContacts();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void OnEnable()
    {
        ShowContacts();
    }

    private void Bind()
    {
        Unbind();
        if (contactsTabButton) contactsTabButton.onClick.AddListener(ShowContacts);
        if (relationTabButton) relationTabButton.onClick.AddListener(ShowRelation);
        if (ojiTabButton) ojiTabButton.onClick.AddListener(ShowOji);
        if (itadakiTabButton) itadakiTabButton.onClick.AddListener(ShowItadaki);
    }

    private void Unbind()
    {
        if (contactsTabButton) contactsTabButton.onClick.RemoveListener(ShowContacts);
        if (relationTabButton) relationTabButton.onClick.RemoveListener(ShowRelation);
        if (ojiTabButton) ojiTabButton.onClick.RemoveListener(ShowOji);
        if (itadakiTabButton) itadakiTabButton.onClick.RemoveListener(ShowItadaki);
    }

    public void ShowContacts()
    {
        SetActive(contactsPage, true);
        SetActive(relationPage, false);
        HideRelationHint();
        ShowOji();
    }

    public void ShowRelation()
    {
        SetActive(contactsPage, false);
        SetActive(relationPage, true);
        HideRelationHint();
    }

    public void ShowOji()
    {
        SetActive(ojiListRoot, true);
        SetActive(itadakiListRoot, false);
    }

    public void ShowItadaki()
    {
        SetActive(ojiListRoot, false);
        SetActive(itadakiListRoot, true);
    }

    public void ShowRelationHint(string message, Vector2 anchoredPosition)
    {
        if (!relationHintBubble || !relationHintText)
            return;

        relationHintBubble.anchoredPosition = anchoredPosition;
        relationHintText.text = string.IsNullOrWhiteSpace(message) ? "ReRe is checking this contact." : message;
        relationHintBubble.gameObject.SetActive(true);
    }

    public void HideRelationHint()
    {
        if (relationHintBubble)
            relationHintBubble.gameObject.SetActive(false);
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target)
            target.SetActive(active);
    }
}
