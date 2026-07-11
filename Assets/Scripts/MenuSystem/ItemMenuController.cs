using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemMenuController : MonoBehaviour
{
    [Serializable]
    private struct ItemDraft
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Color color;
    }

    [SerializeField] private Button[] itemButtons;
    [SerializeField] private RectTransform[] itemHighlights;
    [SerializeField] private Image[] itemIconImages;
    [SerializeField] private TMP_Text[] itemNameTexts;
    [SerializeField] private Image detailIconImage;
    [SerializeField] private TMP_Text detailTitleText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text rereCommentText;
    [SerializeField] private Image[] bagSlotImages;
    [SerializeField] private TMP_Text[] bagSlotTexts;
    [SerializeField] private Button addToBagButton;
    [SerializeField] private Button confirmBagButton;
    [SerializeField] private TMP_Text bagStatusText;
    [SerializeField] private GameObject bagZipOverlay;
    [SerializeField] private Image openBagImage;
    [SerializeField] private Image closedBagImage;
    [SerializeField] private Image zipperHookImage;
    [SerializeField] private Image zipperReReImage;
    [SerializeField] private Image[] packedOverlayItemImages;
    [SerializeField] private TMP_Text zipMessageText;
    [SerializeField] private float zipTravelSeconds = 1.8f;
    [SerializeField] private float zipResultHoldSeconds = 0.9f;
    [SerializeField] private int maxCarryItems = 4;
    [SerializeField] private ItemDraft[] items;

    private readonly int[] carryIndexes = new int[8];
    private int selectedIndex;
    private int carryCount;
    private Coroutine zipRoutine;

    private void Awake()
    {
        AutoWire();
        EnsureDefaultItems();
        BindButtons();
        ClearBag();
        SelectItem(0);
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void AutoWire()
    {
        if (itemButtons == null || itemButtons.Length == 0)
        {
            itemButtons = FindComponentsByName<Button>("ItemCard", 12);
        }

        if (itemHighlights == null || itemHighlights.Length == 0)
        {
            itemHighlights = new RectTransform[itemButtons.Length];
            for (var i = 0; i < itemButtons.Length; i++)
            {
                var highlight = itemButtons[i] ? itemButtons[i].transform.Find("SelectedFrame") : null;
                itemHighlights[i] = highlight as RectTransform;
            }
        }

        if (itemIconImages == null || itemIconImages.Length == 0)
            itemIconImages = FindComponentsByName<Image>("ItemIcon", 12);

        if (itemNameTexts == null || itemNameTexts.Length == 0)
            itemNameTexts = FindComponentsByName<TMP_Text>("ItemName", 12);

        if (!detailIconImage) detailIconImage = FindByName<Image>("DetailItemIcon");
        if (!detailTitleText) detailTitleText = FindByName<TMP_Text>("DetailTitle");
        if (!detailDescriptionText) detailDescriptionText = FindByName<TMP_Text>("DetailDescription");
        if (!rereCommentText) rereCommentText = FindByName<TMP_Text>("ReReCommentText");
        if (!addToBagButton) addToBagButton = FindByName<Button>("AddToBagButton");
        if (!confirmBagButton) confirmBagButton = FindByName<Button>("ConfirmBagButton");
        if (!bagStatusText) bagStatusText = FindByName<TMP_Text>("BagStatusText");
        if (!bagZipOverlay) bagZipOverlay = FindGameObject("BagZipOverlay");
        if (!openBagImage) openBagImage = FindByName<Image>("OpenBagImage");
        if (!closedBagImage) closedBagImage = FindByName<Image>("ClosedBagImage");
        if (!zipperHookImage) zipperHookImage = FindByName<Image>("ZipperHookImage");
        if (!zipperReReImage) zipperReReImage = FindByName<Image>("ZipperReReImage");
        if (!zipMessageText) zipMessageText = FindByName<TMP_Text>("ZipMessageText");

        if (bagSlotImages == null || bagSlotImages.Length == 0)
            bagSlotImages = FindComponentsByName<Image>("BagSlotIcon", 6);

        if (bagSlotTexts == null || bagSlotTexts.Length == 0)
            bagSlotTexts = FindComponentsByName<TMP_Text>("BagSlotText", 6);

        if (packedOverlayItemImages == null || packedOverlayItemImages.Length == 0)
            packedOverlayItemImages = FindComponentsByName<Image>("PackedOverlayItem", 6);

        if (bagZipOverlay)
            bagZipOverlay.SetActive(false);
    }

    private void EnsureDefaultItems()
    {
        if (items != null && items.Length > 0)
            return;

        items = new[]
        {
            Draft("charm", "Lucky Charm", "A small charm for uncertain conversations.", new Color(0.96f, 0.45f, 0.49f, 1f)),
            Draft("ticket", "Cafe Ticket", "Useful when you want an easy reason to meet.", new Color(0.98f, 0.81f, 0.31f, 1f)),
            Draft("drink", "Energy Drink", "A quick boost before a long outing.", new Color(0.43f, 0.86f, 0.74f, 1f)),
            Draft("note", "Secret Note", "A memo ReRe says not to lose.", new Color(0.67f, 0.58f, 0.91f, 1f)),
            Draft("key", "Tiny Key", "Looks connected to a locked route.", new Color(0.45f, 0.78f, 0.92f, 1f)),
            Draft("cosme", "Compact", "Good for making an impression.", new Color(0.98f, 0.68f, 0.52f, 1f)),
            Draft("book", "Guide Book", "Raises the odds of noticing details.", new Color(0.72f, 0.68f, 0.88f, 1f)),
            Draft("gift", "Wrapped Gift", "Someone may like this a lot.", new Color(1f, 0.72f, 0.82f, 1f))
        };
    }

    private static ItemDraft Draft(string id, string name, string description, Color color)
    {
        return new ItemDraft { id = id, displayName = name, description = description, color = color };
    }

    private void BindButtons()
    {
        UnbindButtons();

        for (var i = 0; i < itemButtons.Length; i++)
        {
            var index = i;
            if (itemButtons[i])
                itemButtons[i].onClick.AddListener(() => SelectItem(index));
        }

        if (addToBagButton) addToBagButton.onClick.AddListener(AddSelectedToBag);
        if (confirmBagButton) confirmBagButton.onClick.AddListener(ConfirmBag);
    }

    private void UnbindButtons()
    {
        if (itemButtons != null)
        {
            foreach (var button in itemButtons)
            {
                if (button)
                    button.onClick.RemoveAllListeners();
            }
        }

        if (addToBagButton) addToBagButton.onClick.RemoveListener(AddSelectedToBag);
        if (confirmBagButton) confirmBagButton.onClick.RemoveListener(ConfirmBag);
    }

    private void SelectItem(int index)
    {
        if (items == null || items.Length == 0)
            return;

        selectedIndex = Mathf.Clamp(index, 0, items.Length - 1);
        RefreshItems();
        RefreshDetail();
    }

    private void RefreshItems()
    {
        for (var i = 0; i < itemButtons.Length; i++)
        {
            var hasItem = i < items.Length;
            if (itemButtons[i])
                itemButtons[i].gameObject.SetActive(hasItem);

            if (!hasItem)
                continue;

            if (itemIconImages != null && i < itemIconImages.Length && itemIconImages[i])
                itemIconImages[i].color = items[i].color;

            if (itemNameTexts != null && i < itemNameTexts.Length && itemNameTexts[i])
                itemNameTexts[i].text = items[i].displayName;

            if (itemHighlights != null && i < itemHighlights.Length && itemHighlights[i])
                itemHighlights[i].gameObject.SetActive(i == selectedIndex);
        }
    }

    private void RefreshDetail()
    {
        var item = items[selectedIndex];

        if (detailIconImage) detailIconImage.color = item.color;
        if (detailTitleText) detailTitleText.text = item.displayName;
        if (detailDescriptionText) detailDescriptionText.text = item.description;
        if (rereCommentText)
            rereCommentText.text = "This might help when you need a gentle push. Want to pack it?";
    }

    private void AddSelectedToBag()
    {
        if (carryCount >= Mathf.Min(maxCarryItems, carryIndexes.Length))
        {
            if (bagStatusText) bagStatusText.text = "BAG is full.";
            return;
        }

        carryIndexes[carryCount] = selectedIndex;
        carryCount++;
        RefreshBag();
    }

    private void ConfirmBag()
    {
        if (zipRoutine != null)
            StopCoroutine(zipRoutine);

        zipRoutine = StartCoroutine(ZipBagRoutine());
    }

    private IEnumerator ZipBagRoutine()
    {
        if (carryCount == 0)
        {
            if (bagStatusText)
                bagStatusText.text = "No items packed yet.";
            yield break;
        }

        if (bagZipOverlay)
            bagZipOverlay.SetActive(true);

        if (openBagImage)
            openBagImage.gameObject.SetActive(true);

        if (closedBagImage)
            closedBagImage.gameObject.SetActive(false);

        if (zipMessageText)
            zipMessageText.text = "Packing...";

        RefreshPackedOverlayItems(true);
        SetZipperActorsVisible(true);

        var center = openBagImage ? openBagImage.rectTransform.anchoredPosition : Vector2.zero;
        var halfSize = openBagImage ? openBagImage.rectTransform.sizeDelta * 0.5f : new Vector2(250f, 150f);
        var inset = new Vector2(16f, 18f);
        var points = new[]
        {
            center + new Vector2(-halfSize.x + inset.x, halfSize.y - inset.y),
            center + new Vector2(halfSize.x - inset.x, halfSize.y - inset.y),
            center + new Vector2(halfSize.x - inset.x, -halfSize.y + inset.y),
            center + new Vector2(-halfSize.x + inset.x, -halfSize.y + inset.y),
            center + new Vector2(-halfSize.x + inset.x, halfSize.y - inset.y)
        };

        var elapsed = 0f;
        while (elapsed < zipTravelSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, zipTravelSeconds));
            var pos = PointOnPolyline(points, t);
            MoveZipperActors(pos, t);
            yield return null;
        }

        RefreshPackedOverlayItems(false);

        if (openBagImage)
            openBagImage.gameObject.SetActive(false);

        if (closedBagImage)
            closedBagImage.gameObject.SetActive(true);

        if (zipMessageText)
            zipMessageText.text = "BAG ready!";

        SetZipperActorsVisible(false);

        if (bagStatusText)
            bagStatusText.text = carryCount + "/" + maxCarryItems + " packed / ready";

        yield return new WaitForSecondsRealtime(zipResultHoldSeconds);

        if (bagZipOverlay)
            bagZipOverlay.SetActive(false);
    }

    private void RefreshPackedOverlayItems(bool visible)
    {
        if (packedOverlayItemImages == null)
            return;

        for (var i = 0; i < packedOverlayItemImages.Length; i++)
        {
            if (!packedOverlayItemImages[i])
                continue;

            var filled = visible && i < carryCount && carryIndexes[i] >= 0 && carryIndexes[i] < items.Length;
            packedOverlayItemImages[i].gameObject.SetActive(filled);
            if (filled)
                packedOverlayItemImages[i].color = items[carryIndexes[i]].color;
        }
    }

    private void SetZipperActorsVisible(bool visible)
    {
        if (zipperHookImage)
            zipperHookImage.gameObject.SetActive(visible);

        if (zipperReReImage)
            zipperReReImage.gameObject.SetActive(visible);
    }

    private void MoveZipperActors(Vector2 hookPosition, float progress)
    {
        if (zipperHookImage)
            zipperHookImage.rectTransform.anchoredPosition = hookPosition;

        if (zipperReReImage)
        {
            var swing = Mathf.Sin(progress * Mathf.PI * 8f) * 12f;
            zipperReReImage.rectTransform.anchoredPosition = hookPosition + new Vector2(0f, -86f + swing);
            zipperReReImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(progress * Mathf.PI * 6f) * 9f);
        }
    }

    private static Vector2 PointOnPolyline(Vector2[] points, float t)
    {
        if (points == null || points.Length == 0)
            return Vector2.zero;

        if (points.Length == 1)
            return points[0];

        var scaled = Mathf.Clamp01(t) * (points.Length - 1);
        var index = Mathf.Min(Mathf.FloorToInt(scaled), points.Length - 2);
        var localT = scaled - index;
        return Vector2.Lerp(points[index], points[index + 1], localT);
    }

    private void ConfirmBagLegacy()
    {
        if (bagStatusText)
            bagStatusText.text = carryCount == 0 ? "No items packed yet." : "BAG ready. ReRe will zip it up later.";
    }

    private void ClearBag()
    {
        carryCount = 0;
        for (var i = 0; i < carryIndexes.Length; i++)
            carryIndexes[i] = -1;
        RefreshBag();
    }

    private void RefreshBag()
    {
        var count = bagSlotImages != null ? bagSlotImages.Length : 0;
        for (var i = 0; i < count; i++)
        {
            var filled = i < carryCount && carryIndexes[i] >= 0 && carryIndexes[i] < items.Length;
            if (bagSlotImages[i])
                bagSlotImages[i].color = filled ? items[carryIndexes[i]].color : new Color(1f, 1f, 1f, 0.18f);

            if (bagSlotTexts != null && i < bagSlotTexts.Length && bagSlotTexts[i])
                bagSlotTexts[i].text = filled ? items[carryIndexes[i]].displayName : "EMPTY";
        }

        if (bagStatusText)
            bagStatusText.text = carryCount + "/" + maxCarryItems + " packed";
    }

    private T FindByName<T>(string childName) where T : Component
    {
        foreach (var component in GetComponentsInChildren<T>(true))
        {
            if (component.name == childName)
                return component;
        }

        return null;
    }

    private GameObject FindGameObject(string childName)
    {
        foreach (var rect in GetComponentsInChildren<RectTransform>(true))
        {
            if (rect.name == childName)
                return rect.gameObject;
        }

        return null;
    }

    private T[] FindComponentsByName<T>(string prefix, int max) where T : Component
    {
        var result = new T[max];
        for (var i = 0; i < max; i++)
            result[i] = FindByName<T>(prefix + i);
        return result;
    }
}
