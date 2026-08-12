using Naninovel.UI;
using UnityEngine;
using UnityEngine.UI;

public class MenuRootV2UI : CustomUI
{
    [Header("Pages")]
    [SerializeField] private GameObject standardPhoneLayer;
    [SerializeField] private GameObject pageTop;
    [SerializeField] private GameObject pageStatus;
    [SerializeField] private GameObject pageItems;
    [SerializeField] private GameObject pageCharacters;
    [SerializeField] private GameObject pageQuest;
    [SerializeField] private GameObject pageMap;
    [SerializeField] private GameObject pageSave;
    [SerializeField] private GameObject pageSettings;

    [Header("Presentation")]
    [SerializeField] private MenuRootV2OrientationTransition orientationTransition;

    [Header("Navigation")]
    [SerializeField] private Button topButton;
    [SerializeField] private Button statusButton;
    [SerializeField] private Button itemsButton;
    [SerializeField] private Button charactersButton;
    [SerializeField] private Button questButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button dressHomeButton;
    [SerializeField] private Button dressDressButton;
    [SerializeField] private Button dressStatusButton;
    [SerializeField] private Button dressItemsButton;
    [SerializeField] private Button dressMapButton;
    [SerializeField] private Button charactersHomeButton;
    [SerializeField] private Button charactersDressButton;
    [SerializeField] private Button charactersItemsButton;
    [SerializeField] private Button charactersCharactersButton;
    [SerializeField] private Button charactersQuestButton;
    [SerializeField] private Button charactersMapButton;
    [SerializeField] private Button itemsHomeButton;
    [SerializeField] private Button itemsDressButton;
    [SerializeField] private Button itemsItemsButton;
    [SerializeField] private Button itemsCharactersButton;
    [SerializeField] private Button itemsQuestButton;
    [SerializeField] private Button itemsMapButton;

    [Header("Top Tiles")]
    [SerializeField] private MenuTopReReMascot topMascot;
    [SerializeField] private Button dressTileButton;
    [SerializeField] private Button statusTileButton;
    [SerializeField] private Button itemsTileButton;
    [SerializeField] private Button charactersTileButton;
    [SerializeField] private Button questTileButton;
    [SerializeField] private Button mapTileButton;

    private bool pageStateInitialized;

    public override bool BlockInputWhenVisible => true;

    protected override void Awake()
    {
        base.Awake();
        BindButtons();
        ShowPageImmediate(pageTop);
    }

    protected override void OnDestroy()
    {
        UnbindButtons();
        base.OnDestroy();
    }

    public override void Show()
    {
        base.Show();
        ResetToTop();
    }

    public void ResetToTop()
    {
        ShowPageImmediate(pageTop);
        if (topMascot)
            topMascot.PlaceForMenuOpen();
    }

    public void ShowTop() => ShowPage(pageTop);
    public void ShowStatus() => ShowPage(pageStatus);
    public void ShowItems() => ShowPage(pageItems);
    public void ShowCharacters() => ShowPage(pageCharacters);
    public void ShowQuest() => ShowPage(pageQuest);
    public void ShowMap() => ShowPage(pageMap);
    public void ShowSave() => ShowPage(pageSave);
    public void ShowSettings() => ShowPage(pageSettings);

    private void ShowPage(GameObject target)
    {
        if (!target)
            return;

        var targetIsPortrait = target == pageTop;
        if (pageStateInitialized && orientationTransition)
        {
            if (orientationTransition.RequestPage(target, targetIsPortrait, () => ApplyPage(target)))
                return;

            if (orientationTransition.IsTransitioning)
                return;
        }

        ShowPageImmediate(target);
    }

    private void ShowPageImmediate(GameObject target)
    {
        if (!target)
            return;

        ApplyPage(target);
        orientationTransition?.SetInitialPage(target, target == pageTop);
        pageStateInitialized = true;
    }

    private void ApplyPage(GameObject target)
    {
        if (standardPhoneLayer)
            standardPhoneLayer.SetActive(target != pageTop && target != pageStatus && target != pageItems && target != pageCharacters);

        SetActive(pageTop, target);
        SetActive(pageStatus, target);
        SetActive(pageItems, target);
        SetActive(pageCharacters, target);
        SetActive(pageQuest, target);
        SetActive(pageMap, target);
        SetActive(pageSave, target);
        SetActive(pageSettings, target);
    }

    private static void SetActive(GameObject page, GameObject target)
    {
        if (page)
            page.SetActive(page == target);
    }

    private void BindButtons()
    {
        UnbindButtons();
        if (topButton) topButton.onClick.AddListener(ShowTop);
        if (statusButton) statusButton.onClick.AddListener(ShowStatus);
        if (itemsButton) itemsButton.onClick.AddListener(ShowItems);
        if (charactersButton) charactersButton.onClick.AddListener(ShowCharacters);
        if (questButton) questButton.onClick.AddListener(ShowQuest);
        if (mapButton) mapButton.onClick.AddListener(ShowMap);
        if (saveButton) saveButton.onClick.AddListener(ShowSave);
        if (settingsButton) settingsButton.onClick.AddListener(ShowSettings);
        if (dressHomeButton) dressHomeButton.onClick.AddListener(ShowTop);
        if (dressDressButton) dressDressButton.onClick.AddListener(ShowStatus);
        if (dressStatusButton) dressStatusButton.onClick.AddListener(ShowStatus);
        if (dressItemsButton) dressItemsButton.onClick.AddListener(ShowItems);
        if (dressMapButton) dressMapButton.onClick.AddListener(ShowMap);
        if (charactersHomeButton) charactersHomeButton.onClick.AddListener(ShowTop);
        if (charactersDressButton) charactersDressButton.onClick.AddListener(ShowStatus);
        if (charactersItemsButton) charactersItemsButton.onClick.AddListener(ShowItems);
        if (charactersCharactersButton) charactersCharactersButton.onClick.AddListener(ShowCharacters);
        if (charactersQuestButton) charactersQuestButton.onClick.AddListener(ShowQuest);
        if (charactersMapButton) charactersMapButton.onClick.AddListener(ShowMap);
        if (itemsHomeButton) itemsHomeButton.onClick.AddListener(ShowTop);
        if (itemsDressButton) itemsDressButton.onClick.AddListener(ShowStatus);
        if (itemsItemsButton) itemsItemsButton.onClick.AddListener(ShowItems);
        if (itemsCharactersButton) itemsCharactersButton.onClick.AddListener(ShowCharacters);
        if (itemsQuestButton) itemsQuestButton.onClick.AddListener(ShowQuest);
        if (itemsMapButton) itemsMapButton.onClick.AddListener(ShowMap);
        if (dressTileButton) dressTileButton.onClick.AddListener(ShowStatus);
        if (statusTileButton) statusTileButton.onClick.AddListener(ShowStatus);
        if (itemsTileButton) itemsTileButton.onClick.AddListener(ShowItems);
        if (charactersTileButton) charactersTileButton.onClick.AddListener(ShowCharacters);
        if (questTileButton) questTileButton.onClick.AddListener(ShowQuest);
        if (mapTileButton) mapTileButton.onClick.AddListener(ShowMap);
    }

    private void UnbindButtons()
    {
        if (topButton) topButton.onClick.RemoveListener(ShowTop);
        if (statusButton) statusButton.onClick.RemoveListener(ShowStatus);
        if (itemsButton) itemsButton.onClick.RemoveListener(ShowItems);
        if (charactersButton) charactersButton.onClick.RemoveListener(ShowCharacters);
        if (questButton) questButton.onClick.RemoveListener(ShowQuest);
        if (mapButton) mapButton.onClick.RemoveListener(ShowMap);
        if (saveButton) saveButton.onClick.RemoveListener(ShowSave);
        if (settingsButton) settingsButton.onClick.RemoveListener(ShowSettings);
        if (dressHomeButton) dressHomeButton.onClick.RemoveListener(ShowTop);
        if (dressDressButton) dressDressButton.onClick.RemoveListener(ShowStatus);
        if (dressStatusButton) dressStatusButton.onClick.RemoveListener(ShowStatus);
        if (dressItemsButton) dressItemsButton.onClick.RemoveListener(ShowItems);
        if (dressMapButton) dressMapButton.onClick.RemoveListener(ShowMap);
        if (charactersHomeButton) charactersHomeButton.onClick.RemoveListener(ShowTop);
        if (charactersDressButton) charactersDressButton.onClick.RemoveListener(ShowStatus);
        if (charactersItemsButton) charactersItemsButton.onClick.RemoveListener(ShowItems);
        if (charactersCharactersButton) charactersCharactersButton.onClick.RemoveListener(ShowCharacters);
        if (charactersQuestButton) charactersQuestButton.onClick.RemoveListener(ShowQuest);
        if (charactersMapButton) charactersMapButton.onClick.RemoveListener(ShowMap);
        if (itemsHomeButton) itemsHomeButton.onClick.RemoveListener(ShowTop);
        if (itemsDressButton) itemsDressButton.onClick.RemoveListener(ShowStatus);
        if (itemsItemsButton) itemsItemsButton.onClick.RemoveListener(ShowItems);
        if (itemsCharactersButton) itemsCharactersButton.onClick.RemoveListener(ShowCharacters);
        if (itemsQuestButton) itemsQuestButton.onClick.RemoveListener(ShowQuest);
        if (itemsMapButton) itemsMapButton.onClick.RemoveListener(ShowMap);
        if (dressTileButton) dressTileButton.onClick.RemoveListener(ShowStatus);
        if (statusTileButton) statusTileButton.onClick.RemoveListener(ShowStatus);
        if (itemsTileButton) itemsTileButton.onClick.RemoveListener(ShowItems);
        if (charactersTileButton) charactersTileButton.onClick.RemoveListener(ShowCharacters);
        if (questTileButton) questTileButton.onClick.RemoveListener(ShowQuest);
        if (mapTileButton) mapTileButton.onClick.RemoveListener(ShowMap);
    }
}
