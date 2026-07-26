using UnityEngine;
using UnityEngine.UI;

public class MenuPageNavigation : MonoBehaviour
{
    [SerializeField] private Button home;
    [SerializeField] private Button dress;
    [SerializeField] private Button item;
    [SerializeField] private Button characters;
    [SerializeField] private Button quest;
    [SerializeField] private Button map;
    [SerializeField] private Button save;
    [SerializeField] private Button settings;

    private MenuRootV2UI menuRoot;

    private void Awake()
    {
        menuRoot = GetComponentInParent<MenuRootV2UI>(true);
        BindButtons();
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void BindButtons()
    {
        UnbindButtons();

        if (home) home.onClick.AddListener(ShowHome);
        if (dress) dress.onClick.AddListener(ShowDress);
        if (item) item.onClick.AddListener(ShowItem);
        if (characters) characters.onClick.AddListener(ShowCharacters);
        if (quest) quest.onClick.AddListener(ShowQuest);
        if (map) map.onClick.AddListener(ShowMap);
        if (save) save.onClick.AddListener(ShowSave);
        if (settings) settings.onClick.AddListener(ShowSettings);
    }

    private void UnbindButtons()
    {
        if (home) home.onClick.RemoveListener(ShowHome);
        if (dress) dress.onClick.RemoveListener(ShowDress);
        if (item) item.onClick.RemoveListener(ShowItem);
        if (characters) characters.onClick.RemoveListener(ShowCharacters);
        if (quest) quest.onClick.RemoveListener(ShowQuest);
        if (map) map.onClick.RemoveListener(ShowMap);
        if (save) save.onClick.RemoveListener(ShowSave);
        if (settings) settings.onClick.RemoveListener(ShowSettings);
    }

    private void ShowHome()
    {
        menuRoot?.ShowTop();
    }

    private void ShowDress()
    {
        menuRoot?.ShowStatus();
    }

    private void ShowItem()
    {
        menuRoot?.ShowItems();
    }

    private void ShowCharacters()
    {
        menuRoot?.ShowCharacters();
    }

    private void ShowQuest()
    {
        menuRoot?.ShowQuest();
    }

    private void ShowMap()
    {
        menuRoot?.ShowMap();
    }

    private void ShowSave()
    {
        menuRoot?.ShowSave();
    }

    private void ShowSettings()
    {
        menuRoot?.ShowSettings();
    }
}
