using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterPage : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform gridCharactersRoot;
    [SerializeField] private GameObject characterCellPrefab;
    [SerializeField] private GameObject characterDetailPanel;
    [SerializeField] private Image characterPortraitImage;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text characterDescriptionText;
    [SerializeField] private Button characterDetailCloseButton;

    [Header("Data")]
    [SerializeField] private CharacterDatabase characterDB;

    [Header("External Dependencies")]
    [SerializeField] private AdviceClickTrigger sharedAdviceTrigger;

    private void Awake()
    {
        if (characterDetailCloseButton)
        {
            characterDetailCloseButton.onClick.RemoveAllListeners();
            characterDetailCloseButton.onClick.AddListener(() => characterDetailPanel.SetActive(false));
        }
        if (characterDetailPanel) characterDetailPanel.SetActive(false);
    }

    private void Start()
    {
        if (!gridCharactersRoot) Debug.LogError("[CharacterPage] Grid Characters Root is not assigned!");
        if (!characterCellPrefab) Debug.LogError("[CharacterPage] Character Cell Prefab is not assigned!");
        if (!characterDB) Debug.LogError("[CharacterPage] Character Database is not assigned!");
    }

    public void Show()
    {
        gameObject.SetActive(true);
        PopulateCharacters();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (characterDetailPanel) characterDetailPanel.SetActive(false);
    }

    private void PopulateCharacters()
    {
        if (!gridCharactersRoot || !characterCellPrefab) return;

        ClearChildren(gridCharactersRoot);

        var list = characterDB ? characterDB.GetAll() : null;
        if (list == null || list.Count == 0) return;

        foreach (var ch in list)
        {
            var go = Instantiate(characterCellPrefab, gridCharactersRoot);
            var img = go.GetComponentInChildren<Image>(true);
            if (img) img.sprite = ch.icon;

            var btn = go.GetComponent<Button>();
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowCharacterDetail(ch));
            }

            // マウスオーバー時のアドバイス表示
            var pointer = go.GetComponent<UIPointerEvents>();
            if (!pointer) pointer = go.AddComponent<UIPointerEvents>();

            pointer.onEnter = () =>
            {
                if (sharedAdviceTrigger) sharedAdviceTrigger.ShowAdvice(ch.summary, true);
            };
            pointer.onExit = () =>
            {
                sharedAdviceTrigger?.HideAdvice();
            };
        }
    }

    private void ShowCharacterDetail(CharacterInfo ch)
    {
        if (!characterDetailPanel) return;

        if (characterPortraitImage)
            characterPortraitImage.sprite = ch.portrait ? ch.portrait : ch.icon;

        if (characterNameText)
            characterNameText.text = string.IsNullOrEmpty(ch.displayName) ? ch.id : ch.displayName;

        if (characterDescriptionText)
            characterDescriptionText.text = ch.description;

        characterDetailPanel.SetActive(true);
    }

    private void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }

    public void SetAdviceTrigger(AdviceClickTrigger trigger)
    {
        sharedAdviceTrigger = trigger;
    }
}
