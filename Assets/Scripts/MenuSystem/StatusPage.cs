using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusPage : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RadarChart radarChart;
    [SerializeField] private Image protagonistPortrait;
    [SerializeField] private TMP_Text gutsText;
    [SerializeField] private TMP_Text intelligenceText;
    [SerializeField] private TMP_Text attentionText;
    [SerializeField] private TMP_Text techniqueText;
    [SerializeField] private TMP_Text strengthText;

    [Header("Parameter Hub")]
    [SerializeField] private Button itemsShortcutButton;
    [SerializeField] private Button charactersShortcutButton;
    [SerializeField] private Button mapShortcutButton;
    [SerializeField] private Transform characterIconRoot;
    [SerializeField] private GameObject characterIconPrefab;
    [SerializeField] private CharacterDatabase characterDB;
    [SerializeField] private AdviceClickTrigger sharedAdviceTrigger;

    private Action openItems;
    private Action openCharacters;
    private Action openMap;

    private void Awake()
    {
        BindShortcutButtons();
    }

    private void OnEnable()
    {
        UpdateUI();
        PopulateCharacterIcons();
        BindShortcutButtons();

        if (StatusManager.Instance != null)
            StatusManager.Instance.OnStatusChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (StatusManager.Instance != null)
            StatusManager.Instance.OnStatusChanged -= UpdateUI;

        UnbindShortcutButtons();
    }

    public void Configure(
        RadarChart chart,
        Image portrait,
        TMP_Text guts,
        TMP_Text intelligence,
        TMP_Text attention,
        TMP_Text technique,
        TMP_Text strength)
    {
        radarChart = chart;
        protagonistPortrait = portrait;
        gutsText = guts;
        intelligenceText = intelligence;
        attentionText = attention;
        techniqueText = technique;
        strengthText = strength;
    }

    public void ConfigureParameterHub(
        Button itemsButton,
        Button charactersButton,
        Button mapButton,
        Transform iconRoot,
        GameObject iconPrefab,
        CharacterDatabase database,
        AdviceClickTrigger adviceTrigger)
    {
        UnbindShortcutButtons();

        itemsShortcutButton = itemsButton;
        charactersShortcutButton = charactersButton;
        mapShortcutButton = mapButton;
        characterIconRoot = iconRoot;
        characterIconPrefab = iconPrefab;
        characterDB = database;
        sharedAdviceTrigger = adviceTrigger;

        BindShortcutButtons();
        if (Application.isPlaying)
            PopulateCharacterIcons();
    }

    public void SetNavigationActions(Action showItems, Action showCharacters, Action showMap)
    {
        UnbindShortcutButtons();

        openItems = showItems;
        openCharacters = showCharacters;
        openMap = showMap;

        BindShortcutButtons();
    }

    public void SetPortrait(Sprite sprite)
    {
        if (protagonistPortrait)
            protagonistPortrait.sprite = sprite;
    }

    private void UpdateUI()
    {
        if (StatusManager.Instance == null) return;

        int guts = StatusManager.Instance.Guts;
        int intelligence = StatusManager.Instance.Intelligence;
        int attention = StatusManager.Instance.Attention;
        int technique = StatusManager.Instance.Technique;
        int strength = StatusManager.Instance.Strength;

        if (radarChart)
        {
            radarChart.SetValues(guts, intelligence, attention, technique, strength);
            radarChart.GenerateMesh();
        }

        if (gutsText) gutsText.text = $"胆力\n{guts}";
        if (intelligenceText) intelligenceText.text = $"知力\n{intelligence}";
        if (attentionText) attentionText.text = $"注意力\n{attention}";
        if (techniqueText) techniqueText.text = $"技術力\n{technique}";
        if (strengthText) strengthText.text = $"筋力\n{strength}";
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateUI();
        PopulateCharacterIcons();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void BindShortcutButtons()
    {
        UnbindShortcutButtons();

        if (itemsShortcutButton && openItems != null)
            itemsShortcutButton.onClick.AddListener(OpenItems);
        if (charactersShortcutButton && openCharacters != null)
            charactersShortcutButton.onClick.AddListener(OpenCharacters);
        if (mapShortcutButton && openMap != null)
            mapShortcutButton.onClick.AddListener(OpenMap);
    }

    private void UnbindShortcutButtons()
    {
        if (itemsShortcutButton) itemsShortcutButton.onClick.RemoveListener(OpenItems);
        if (charactersShortcutButton) charactersShortcutButton.onClick.RemoveListener(OpenCharacters);
        if (mapShortcutButton) mapShortcutButton.onClick.RemoveListener(OpenMap);
    }

    private void OpenItems() => openItems?.Invoke();
    private void OpenCharacters() => openCharacters?.Invoke();
    private void OpenMap() => openMap?.Invoke();

    private void PopulateCharacterIcons()
    {
        if (!characterIconRoot || !characterIconPrefab) return;

        ClearChildren(characterIconRoot);

        var list = characterDB ? characterDB.GetAll() : null;
        if (list == null || list.Count == 0) return;

        foreach (var character in list)
        {
            var go = Instantiate(characterIconPrefab, characterIconRoot);
            go.SetActive(true);

            var image = go.GetComponentInChildren<Image>(true);
            if (image) image.sprite = character.icon ? character.icon : character.portrait;

            var label = go.GetComponentInChildren<TMP_Text>(true);
            if (label)
                label.text = string.IsNullOrWhiteSpace(character.displayName) ? character.id : character.displayName;

            var button = go.GetComponent<Button>();
            if (button)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OpenCharacters);
                button.onClick.AddListener(() => MenuReReAdvisor.Instance?.ShowCharacterHint(character));
            }

            var pointer = go.GetComponent<UIPointerEvents>();
            if (!pointer) pointer = go.AddComponent<UIPointerEvents>();

            pointer.onEnter = () =>
            {
                if (sharedAdviceTrigger)
                    sharedAdviceTrigger.ShowAdvice(character.summary, true);
            };
            pointer.onExit = () => sharedAdviceTrigger?.HideAdvice();
        }
    }

    private void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }
}
