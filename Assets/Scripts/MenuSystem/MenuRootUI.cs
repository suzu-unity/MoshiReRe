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
    [SerializeField] private Button rerePortraitButton;   // ReRe 立ち絵のボタン
    // ★ 吹き出し表示を担当する共通トリガー（AdviceClickTrigger）
    [SerializeField] private AdviceClickTrigger sharedAdviceTrigger;

    [Header("Items Page")]
    [SerializeField] private Transform gridItemsRoot;
    [SerializeField] private GameObject itemCellPrefab;
    [SerializeField] private InventoryDatabase inventoryDB;
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private Image itemDetailImage;
    [SerializeField] private TMP_Text itemDetailTitle;
    [SerializeField] private TMP_Text itemDetailDescription;
    [SerializeField] private Button itemDetailCloseButton;

    [Header("Characters Page")]
    [SerializeField] private Transform gridCharactersRoot;
    [SerializeField] private GameObject characterCellPrefab;
    [SerializeField] private CharacterDatabase characterDB;
    [SerializeField] private GameObject characterDetailPanel;
    [SerializeField] private Image characterPortraitImage;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text characterDescriptionText;
    [SerializeField] private Button characterDetailCloseButton;

    [Header("Advice (demo messages)")]
    [TextArea] [SerializeField] private string[] adviceMessages;
    [SerializeField] private bool firstAdviceSticky = true; // 最初のアドバイスを自動で消さないかどうか

    private void Awake()
    {
        ShowPageTop();

        if (itemDetailPanel) itemDetailPanel.SetActive(false);
        if (characterDetailPanel) characterDetailPanel.SetActive(false);

        if (rerePortraitButton)
        {
            rerePortraitButton.onClick.RemoveAllListeners();
            rerePortraitButton.onClick.AddListener(NextAdvice);
        }

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

    private void OnEnable()
    {
        // 初回にアドバイスを表示
        if (sharedAdviceTrigger && adviceMessages != null && adviceMessages.Length > 0)
        {
            // firstAdviceSticky が true のときは autoHide を false にする
            bool autoHide = !firstAdviceSticky;
            sharedAdviceTrigger.ShowAdvice(adviceMessages[0], autoHide);
        }
    }

    private void OnDisable()
    {
        if (sharedAdviceTrigger) sharedAdviceTrigger.HideAdvice();
        if (rerePortraitButton) rerePortraitButton.onClick.RemoveListener(NextAdvice);
    }

    /// <summary>
    /// ReReの立ち絵をクリックしたときに呼ばれる。メッセージをランダムに表示する。
    /// </summary>
    public void NextAdvice()
    {
        if (adviceMessages == null || adviceMessages.Length == 0 || sharedAdviceTrigger == null) return;

        // ランダムなインデックスを取得して表示
        int index = Random.Range(0, adviceMessages.Length);
        sharedAdviceTrigger.ShowAdvice(adviceMessages[index], true);
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
            var go = Object.Instantiate(itemCellPrefab, gridItemsRoot);
            var img = go.GetComponentInChildren<Image>(true);
            if (img) img.sprite = item.icon;

            var btn = go.GetComponent<Button>();
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowItemDetail(item));
            }

            // マウスオーバー時のアドバイス表示
            var pointer = go.GetComponent<UIPointerEvents>();
            if (!pointer) pointer = go.AddComponent<UIPointerEvents>();

            pointer.onEnter = () =>
            {
                // Summaryを数秒表示したいので autoHide = true
                if (sharedAdviceTrigger) sharedAdviceTrigger.ShowAdvice(item.summary, true);
            };
            pointer.onExit = () =>
            {
                sharedAdviceTrigger?.HideAdvice();
            };
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
    private void PopulateCharacters()
    {
        if (!gridCharactersRoot || !characterCellPrefab) return;

        ClearChildren(gridCharactersRoot);

        var list = characterDB ? characterDB.GetAll() : null;
        if (list == null || list.Count == 0) return;

        foreach (var ch in list)
        {
            var go = Object.Instantiate(characterCellPrefab, gridCharactersRoot);
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

    public void OnOjToggleChanged(bool isOn)
    {
        if (!isOn) return;
        PopulateCharacters();
    }

    public void OnItadakiToggleChanged(bool isOn)
    {
        if (!isOn) return;
        PopulateCharacters();
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
/// 簡易マウスオーバー検知用コンポーネント。
/// IPointerEnterHandler / IPointerExitHandler を利用して
/// onEnter / onExit のコールバックを発火します。
/// </summary>
public class UIPointerEvents : MonoBehaviour,
    UnityEngine.EventSystems.IPointerEnterHandler,
    UnityEngine.EventSystems.IPointerExitHandler
{
    public System.Action onEnter;
    public System.Action onExit;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        => onEnter?.Invoke();

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        => onExit?.Invoke();
}
