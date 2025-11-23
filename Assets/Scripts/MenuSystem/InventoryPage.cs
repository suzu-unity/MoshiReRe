using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryPage : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform gridItemsRoot;
    [SerializeField] private GameObject itemCellPrefab;
    [SerializeField] private GameObject itemDetailPanel;
    [SerializeField] private Image itemDetailImage;
    [SerializeField] private TMP_Text itemDetailTitle;
    [SerializeField] private TMP_Text itemDetailDescription;
    [SerializeField] private Button itemDetailCloseButton;

    [Header("Data")]
    [SerializeField] private InventoryDatabase inventoryDB;

    [Header("External Dependencies")]
    [SerializeField] private AdviceClickTrigger sharedAdviceTrigger;

    private void Awake()
    {
        if (itemDetailCloseButton)
        {
            itemDetailCloseButton.onClick.RemoveAllListeners();
            itemDetailCloseButton.onClick.AddListener(() => itemDetailPanel.SetActive(false));
        }
        if (itemDetailPanel) itemDetailPanel.SetActive(false);
    }

    private void Start()
    {
        if (!gridItemsRoot) Debug.LogError("[InventoryPage] Grid Items Root is not assigned!");
        if (!itemCellPrefab) Debug.LogError("[InventoryPage] Item Cell Prefab is not assigned!");
        if (!inventoryDB) Debug.LogError("[InventoryPage] Inventory Database is not assigned!");
    }

    public void Show()
    {
        gameObject.SetActive(true);
        PopulateItems();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (itemDetailPanel) itemDetailPanel.SetActive(false);
    }

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
