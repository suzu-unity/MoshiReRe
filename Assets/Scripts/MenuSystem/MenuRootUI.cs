using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuRootUI : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private GameObject pageTop;
    [SerializeField] private GameObject pageItems;
    [SerializeField] private GameObject pageCharacters;

    [Header("Common UI")]
    [SerializeField] private Button rerePortraitButton;   // ReRe立ち絵のボタン
    [SerializeField] private AdviceBubble adviceBubble;   // 吹き出し

    [Header("Items Page")]
    [SerializeField] private Transform gridItemsRoot;     // GridLayoutの親
    [SerializeField] private GameObject itemCellPrefab;   // 小さなセル（Image+Button想定）
    [SerializeField] private InventoryDatabase inventoryDB;
    [SerializeField] private GameObject itemDetailPanel;  // 詳細パネル（非表示スタート）
    [SerializeField] private Image itemDetailImage;
    [SerializeField] private TMP_Text itemDetailTitle;
    [SerializeField] private TMP_Text itemDetailDescription;
    [SerializeField] private Button itemDetailCloseButton;

    [Header("Characters Page")]
    [SerializeField] private Transform gridCharactersRoot;
    [SerializeField] private GameObject characterCellPrefab; // 小さなセル（Image+Button想定）
    [SerializeField] private CharacterDatabase characterDB;
    [SerializeField] private GameObject characterDetailPanel;
    [SerializeField] private Image characterPortraitImage;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text characterDescriptionText;
    [SerializeField] private Button characterDetailCloseButton;

    [Header("Advice (optional demo)")]
    [TextArea] [SerializeField] private string[] adviceMessages;
    [SerializeField] private bool firstAdviceSticky = true; // 最初のアドバイス出しっぱにするか
    private int adviceIndex = 0;

    void Awake()
    {
        // 初期はTopページ
        ShowPageTop();

        // 詳細パネルは隠す
        if (itemDetailPanel) itemDetailPanel.SetActive(false);
        if (characterDetailPanel) characterDetailPanel.SetActive(false);

        // 立ち絵クリックで次のアドバイス
        if (rerePortraitButton)
        {
            rerePortraitButton.onClick.RemoveListener(NextAdvice);
            rerePortraitButton.onClick.AddListener(NextAdvice);
        }

        // 詳細パネルの戻る
        if (itemDetailCloseButton)
        {
            itemDetailCloseButton.onClick.RemoveAllListeners();
            itemDetailCloseButton.onClick.AddListener(() => itemDetailPanel.SetActive(false));
        }
        if (characterDetailCloseButton)
        {
            characterDetailCloseButton.onClick.RemoveAllListeners();
            characterDetailCloseButton.onClick.AddListener(() => characterDetailPanel.SetActive(false));
        }
    }

    void OnEnable()
    {
        // 最初のアドバイスを出す（任意）
        if (adviceBubble && adviceMessages != null && adviceMessages.Length > 0)
            adviceBubble.Show(adviceMessages[adviceIndex], autoHideOverride: !firstAdviceSticky);
    }

    void OnDisable()
    {
        if (adviceBubble) adviceBubble.Hide();
        if (rerePortraitButton) rerePortraitButton.onClick.RemoveListener(NextAdvice);
    }

    public void NextAdvice()
    {
        if (adviceMessages == null || adviceMessages.Length == 0 || adviceBubble == null) return;
        adviceIndex = (adviceIndex + 1) % adviceMessages.Length;
        adviceBubble.UpdateText(adviceMessages[adviceIndex]);
    }

    // ===== Top / Items / Characters ページ切替 =====
    public void ShowPageTop()
    {
        if (pageTop) pageTop.SetActive(true);
        if (pageItems) pageItems.SetActive(false);
        if (pageCharacters) pageCharacters.SetActive(false);
    }

    public void ShowPageItems()
    {
        if (pageTop) pageTop.SetActive(false);
        if (pageItems) pageItems.SetActive(true);
        if (pageCharacters) pageCharacters.SetActive(false);

        PopulateItems();
    }

    public void ShowPageCharacters()
    {
        if (pageTop) pageTop.SetActive(false);
        if (pageItems) pageItems.SetActive(false);
        if (pageCharacters) pageCharacters.SetActive(true);

        // ひとまず全件表示（後でカテゴリフィルタに対応）
        PopulateCharacters();
    }

    // ====== Items ======
    private void PopulateItems()
    {
        if (!gridItemsRoot || !itemCellPrefab) return;

        ClearChildren(gridItemsRoot);

        var items = inventoryDB ? inventoryDB.GetAll() : null;
        if (items == null || items.Count == 0) return;

        foreach (var item in items)
        {
            var go = Instantiate(itemCellPrefab, gridItemsRoot);
            var img = go.GetComponentInChildren<Image>(true);
            if (img) img.sprite = item.icon;

            // Hoverで吹き出し、Clickで詳細
            var btn = go.GetComponent<Button>();
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowItemDetail(item));
            }

            // マウスオーバー時の説明（EventTriggerでも可）
            var pointer = go.AddComponent<UIPointerEvents>(); // 下に定義
            pointer.onEnter = () => { if (adviceBubble) adviceBubble.UpdateText(item.summary); };
            pointer.onExit  = () => { /*必要なら何もしない or 最初のアドバイスに戻す*/ };
        }
    }

    private void ShowItemDetail(InventoryItem item)
    {
        if (!itemDetailPanel) return;
        if (itemDetailImage) itemDetailImage.sprite = item.detailImage ? item.detailImage : item.icon;
        if (itemDetailTitle) itemDetailTitle.text = string.IsNullOrEmpty(item.id) ? "Item" : item.id;
        if (itemDetailDescription) itemDetailDescription.text = item.description;
        itemDetailPanel.SetActive(true);
    }

    // ====== Characters ======
    private void PopulateCharacters(CharacterCategory? filter = null)
    {
        if (!gridCharactersRoot || !characterCellPrefab) return;

        ClearChildren(gridCharactersRoot);

        var list = characterDB ? characterDB.GetAll() : null;
        if (list == null || list.Count == 0) return;

        var view = (filter.HasValue) ? list.Where(c => c.category == filter.Value) : list;

        foreach (var ch in view)
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

            var pointer = go.AddComponent<UIPointerEvents>();
            pointer.onEnter = () => { if (adviceBubble) adviceBubble.UpdateText(ch.summary); };
            pointer.onExit  = () => { /*必要なら何もしない*/ };
        }
    }

    public void OnOjToggleChanged(bool isOn)
    {
        if (!isOn) return;
        PopulateCharacters(CharacterCategory.Oj);
    }

    public void OnItadakiToggleChanged(bool isOn)
    {
        if (!isOn) return;
        PopulateCharacters(CharacterCategory.Itadaki);
    }
// ====== Character Detail ======
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

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Object.Destroy(root.GetChild(i).gameObject);
    }
}

/// <summary>
/// 簡易マウスオーバー検知（EventTrigger代替の最小実装）
/// </summary>
public class UIPointerEvents : MonoBehaviour,
    UnityEngine.EventSystems.IPointerEnterHandler,
    UnityEngine.EventSystems.IPointerExitHandler
{
    public System.Action onEnter;
    public System.Action onExit;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData) => onEnter?.Invoke();
    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData) => onExit?.Invoke();
}
