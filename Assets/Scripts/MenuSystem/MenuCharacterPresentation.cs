using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MenuCharacterPresentation : MonoBehaviour
{
    [SerializeField] private CharacterInfo[] characters;
    [SerializeField] private Button[] rows;
    [SerializeField] private Sprite[] portraits;
    [SerializeField] private Image portrait;
    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Sprite filledHeart;
    [SerializeField] private CharacterInformationNodePanel information;
    private int[] affinity;
    private int selected;

    private void Awake()
    {
        affinity = new int[characters.Length];
        for (var i = 0; i < rows.Length; i++)
        {
            var index = i;
            if (rows[i]) rows[i].onClick.AddListener(() => Select(index));
        }
    }
    private void Start() { if (characters.Length > 0) Select(0); }
    public void Select(int index)
    {
        if (index < 0 || index >= characters.Length) return;
        selected = index;
        if (portrait && portraits.Length > 0) portrait.sprite = portraits[index % portraits.Length];
        if (information) information.SelectCharacterByIndex(index);
        RefreshHearts();
    }
    public void SetAffinity(int characterIndex, int value)
    {
        if (affinity == null || characterIndex < 0 || characterIndex >= affinity.Length) return;
        affinity[characterIndex] = Mathf.Clamp(value, 0, hearts.Length);
        if (selected == characterIndex) RefreshHearts();
    }
    private void RefreshHearts()
    {
        for (var i = 0; i < hearts.Length; i++)
            if (hearts[i]) hearts[i].sprite = affinity != null && i < affinity[selected] ? filledHeart : emptyHeart;
    }
    public void ShowAll() => Filter(-1);
    public void ShowOji() => Filter((int)CharacterCategory.Oj);
    public void ShowItadaki() => Filter((int)CharacterCategory.Itadaki);
    private void Filter(int category)
    {
        for (var i = 0; i < rows.Length; i++)
            if (rows[i]) rows[i].gameObject.SetActive(category < 0 || characters[i] && (int)characters[i].category == category);
    }
}
