using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemMenuController : MonoBehaviour
{
    [Serializable]
    private struct ItemDraft
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        [TextArea] public string summary;
        public Sprite icon;
        public Sprite detailImage;
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
    [SerializeField] private RectTransform bagDropArea;
    [SerializeField] private Button addToBagButton;
    [SerializeField] private Button confirmBagButton;
    [SerializeField] private TMP_Text bagStatusText;
    [SerializeField] private GameObject bagZipOverlay;
    [SerializeField] private Image openBagImage;
    [SerializeField] private Image closedBagImage;
    [SerializeField] private Image zipperHookImage;
    [SerializeField] private Image zipperReReImage;
    [SerializeField] private Sprite openBagSprite;
    [SerializeField] private Sprite closedBagSprite;
    [SerializeField] private Sprite[] bagZipStages;
    [SerializeField] private Sprite zipperHookSprite;
    [SerializeField] private Sprite[] zipperReReFrames;
    [SerializeField] private Image[] packedOverlayItemImages;
    [SerializeField] private TMP_Text zipMessageText;
    [SerializeField] private float zipTravelSeconds = 1.8f;
    [SerializeField] private float zipResultHoldSeconds = 0.9f;
    [SerializeField] private float zipReReFrameRate = 10f;
    [SerializeField] private Vector2 zipReReSize = new Vector2(210f, 210f);
    [SerializeField] private float bagStageDisplayWidth = 540f;
    [SerializeField] private float bagStageBottomY = -312f;
    [SerializeField] private Vector2 zipperPathCenter = new Vector2(-10f, 24f);
    [SerializeField] private Vector2 zipperPathHalfSize = new Vector2(232f, 160f);
    [SerializeField] private int maxCarryItems = 4;
    [SerializeField] private InventoryDatabase inventoryDatabase;
    [SerializeField] private ItemDraft[] items;

    private readonly int[] carryIndexes = new int[8];
    private int selectedIndex;
    private int carryCount;
    private Coroutine zipRoutine;
    private RectTransform pageRect;
    private Image dragGhostImage;

    private const float ZipPathEnd = 0.88f;

    private void Awake()
    {
        AutoWire();
        LoadInventoryDatabase();
        EnsureDefaultItems();
        BindButtons();
        ConfigureDragSources();
        ConfigureBagDragSources();
        ClearBag();
        ApplyZipSprites();
        SelectItem(0);
    }

    private void OnEnable()
    {
        InventoryDatabase.ItemAcquired += HandleItemAcquired;
        RefreshFromInventoryDatabase();
    }

    private void OnDestroy()
    {
        InventoryDatabase.ItemAcquired -= HandleItemAcquired;
        UnbindButtons();
    }

    private void OnDisable()
    {
        InventoryDatabase.ItemAcquired -= HandleItemAcquired;
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
        if (!bagDropArea)
        {
            var dropObject = FindGameObject("CarryBagPanel");
            if (dropObject)
                bagDropArea = dropObject.transform as RectTransform;
        }

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

        pageRect = transform as RectTransform;
    }

    private void ApplyZipSprites()
    {
        if (openBagImage && openBagSprite)
        {
            openBagImage.sprite = openBagSprite;
            openBagImage.color = Color.white;
            openBagImage.preserveAspect = true;
            FitBagStageImage(openBagImage, openBagSprite);
        }

        if (closedBagImage && closedBagSprite)
        {
            closedBagImage.sprite = closedBagSprite;
            closedBagImage.color = Color.white;
            closedBagImage.preserveAspect = true;
            FitBagStageImage(closedBagImage, closedBagSprite);
        }

        if (zipperHookImage && zipperHookSprite)
        {
            zipperHookImage.sprite = zipperHookSprite;
            zipperHookImage.color = Color.white;
            zipperHookImage.preserveAspect = true;
        }

        if (zipperReReImage)
        {
            zipperReReImage.preserveAspect = true;
            zipperReReImage.rectTransform.sizeDelta = zipReReSize;
            if (zipperReReFrames != null && zipperReReFrames.Length > 0)
                zipperReReImage.sprite = zipperReReFrames[0];
        }
    }

    private void EnsureDefaultItems()
    {
        if (inventoryDatabase)
            return;

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

    private void LoadInventoryDatabase()
    {
        if (!inventoryDatabase)
            return;

        var databaseItems = inventoryDatabase.GetAcquired();
        var loadedItems = new System.Collections.Generic.List<ItemDraft>(databaseItems.Count);
        for (var i = 0; i < databaseItems.Count; i++)
        {
            var item = databaseItems[i];
            if (!item)
                continue;

            var displayName = item.GetDisplayName();
            var id = string.IsNullOrWhiteSpace(item.id) ? item.name : item.id;
            loadedItems.Add(new ItemDraft
            {
                id = id,
                displayName = displayName,
                summary = item.summary,
                description = item.description,
                icon = item.icon,
                detailImage = item.detailImage,
                color = DefaultItemColor(loadedItems.Count)
            });
        }

        items = loadedItems.ToArray();
    }

    private void RefreshFromInventoryDatabase()
    {
        if (!inventoryDatabase) return;
        LoadInventoryDatabase();
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, items.Length - 1));
        RefreshItems();
        if (items.Length > 0) RefreshDetail();
    }

    private void HandleItemAcquired(InventoryItem item) => RefreshFromInventoryDatabase();

    private static ItemDraft Draft(string id, string name, string description, Color color)
    {
        return new ItemDraft { id = id, displayName = name, description = description, color = color };
    }

    private static Color DefaultItemColor(int index)
    {
        var colors = new[]
        {
            new Color(0.96f, 0.45f, 0.49f, 1f), new Color(0.98f, 0.81f, 0.31f, 1f),
            new Color(0.43f, 0.86f, 0.74f, 1f), new Color(0.67f, 0.58f, 0.91f, 1f),
            new Color(0.45f, 0.78f, 0.92f, 1f), new Color(0.98f, 0.68f, 0.52f, 1f),
            new Color(0.72f, 0.68f, 0.88f, 1f), new Color(1f, 0.72f, 0.82f, 1f)
        };
        return colors[index % colors.Length];
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

        if (addToBagButton)
            addToBagButton.gameObject.SetActive(false);
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

    private void ConfigureDragSources()
    {
        if (itemButtons == null)
            return;

        for (var i = 0; i < itemButtons.Length; i++)
        {
            if (!itemButtons[i])
                continue;

            var source = itemButtons[i].GetComponent<ItemMenuDragSource>();
            if (!source)
                source = itemButtons[i].gameObject.AddComponent<ItemMenuDragSource>();

            source.Initialize(this, i);
        }
    }

    private void ConfigureBagDragSources()
    {
        if (bagSlotImages == null)
            return;

        for (var i = 0; i < bagSlotImages.Length; i++)
        {
            var slot = bagSlotImages[i] ? bagSlotImages[i].transform.parent : null;
            if (!slot)
                continue;

            var slotImage = slot.GetComponent<Image>();
            if (slotImage)
                slotImage.raycastTarget = true;

            var source = slot.GetComponent<ItemMenuBagDragSource>();
            if (!source)
                source = slot.gameObject.AddComponent<ItemMenuBagDragSource>();

            source.Initialize(this, i);
        }
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
                ApplyItemImage(itemIconImages[i], items[i].icon ? items[i].icon : items[i].detailImage, items[i].color);

            if (itemNameTexts != null && i < itemNameTexts.Length && itemNameTexts[i])
                itemNameTexts[i].text = items[i].displayName;

            if (itemHighlights != null && i < itemHighlights.Length && itemHighlights[i])
                itemHighlights[i].gameObject.SetActive(i == selectedIndex);
        }
    }

    private void RefreshDetail()
    {
        var item = items[selectedIndex];

        if (detailIconImage) ApplyItemImage(detailIconImage, item.detailImage ? item.detailImage : item.icon, item.color);
        if (detailTitleText) detailTitleText.text = item.displayName;
        if (detailDescriptionText)
            detailDescriptionText.text = string.IsNullOrWhiteSpace(item.description) ? item.summary : item.description;
        if (rereCommentText)
            rereCommentText.text = string.IsNullOrWhiteSpace(item.summary)
                ? "Drag this item into the bag if you want to carry it."
                : item.summary;
    }

    private void AddSelectedToBag()
    {
        AddItemToBag(selectedIndex);
    }

    public void BeginItemDrag(int index, PointerEventData eventData)
    {
        if (items == null || index < 0 || index >= items.Length)
            return;

        SelectItem(index);
        EnsureDragGhost();

        if (!dragGhostImage)
            return;

        ApplyItemImage(dragGhostImage, items[index].icon ? items[index].icon : items[index].detailImage, items[index].color);
        dragGhostImage.gameObject.SetActive(true);
        MoveDragGhost(eventData);
    }

    public void MoveItemDrag(PointerEventData eventData)
    {
        MoveDragGhost(eventData);
    }

    public void EndItemDrag(int index, PointerEventData eventData)
    {
        if (dragGhostImage)
            dragGhostImage.gameObject.SetActive(false);

        if (IsPointerOverBag(eventData))
            AddItemToBag(index);
        else if (bagStatusText)
            bagStatusText.text = carryCount + "/" + maxCarryItems + " packed";
    }

    public void BeginBagDrag(int slotIndex, PointerEventData eventData)
    {
        if (slotIndex < 0 || slotIndex >= carryCount || items == null)
            return;

        var itemIndex = carryIndexes[slotIndex];
        if (itemIndex < 0 || itemIndex >= items.Length)
            return;

        SelectItem(itemIndex);
        EnsureDragGhost();
        if (!dragGhostImage)
            return;

        ApplyItemImage(dragGhostImage, items[itemIndex].icon ? items[itemIndex].icon : items[itemIndex].detailImage, items[itemIndex].color);
        dragGhostImage.gameObject.SetActive(true);
        MoveDragGhost(eventData);
    }

    public void MoveBagDrag(PointerEventData eventData)
    {
        MoveDragGhost(eventData);
    }

    public void EndBagDrag(int slotIndex, PointerEventData eventData)
    {
        if (dragGhostImage)
            dragGhostImage.gameObject.SetActive(false);

        if (slotIndex < 0 || slotIndex >= carryCount)
            return;

        if (!IsPointerOverBag(eventData))
            RemoveItemFromBag(slotIndex);
        else if (bagStatusText)
            bagStatusText.text = carryCount + "/" + maxCarryItems + " packed";
    }

    private void AddItemToBag(int index)
    {
        if (carryCount >= Mathf.Min(maxCarryItems, carryIndexes.Length))
        {
            if (bagStatusText) bagStatusText.text = "BAG is full.";
            return;
        }

        if (items == null || index < 0 || index >= items.Length)
            return;

        carryIndexes[carryCount] = index;
        carryCount++;
        RefreshBag();

        if (rereCommentText)
            rereCommentText.text = items[index].displayName + "をバッグに入れたよ。";
    }

    private void RemoveItemFromBag(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= carryCount)
            return;

        for (var i = slotIndex; i < carryCount - 1; i++)
            carryIndexes[i] = carryIndexes[i + 1];

        carryCount--;
        carryIndexes[carryCount] = -1;
        RefreshBag();

        if (rereCommentText)
            rereCommentText.text = "Item returned to the list.";
    }

    private void EnsureDragGhost()
    {
        if (dragGhostImage)
            return;

        var root = pageRect ? pageRect : transform as RectTransform;
        var ghostObject = new GameObject("ItemDragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        ghostObject.transform.SetParent(root, false);
        ghostObject.transform.SetAsLastSibling();

        dragGhostImage = ghostObject.GetComponent<Image>();
        dragGhostImage.raycastTarget = false;
        dragGhostImage.color = Color.white;
        dragGhostImage.rectTransform.sizeDelta = new Vector2(76f, 76f);

        var canvasGroup = ghostObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        ghostObject.SetActive(false);
    }

    private void MoveDragGhost(PointerEventData eventData)
    {
        if (!dragGhostImage || !pageRect || eventData == null)
            return;

        var camera = eventData.pressEventCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(pageRect, eventData.position, camera, out var local))
            dragGhostImage.rectTransform.anchoredPosition = local;
    }

    private bool IsPointerOverBag(PointerEventData eventData)
    {
        if (!bagDropArea || eventData == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(bagDropArea, eventData.position, eventData.pressEventCamera);
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

        ApplyZipSprites();

        if (openBagImage)
            openBagImage.gameObject.SetActive(true);

        if (closedBagImage)
            closedBagImage.gameObject.SetActive(false);

        if (zipMessageText)
            zipMessageText.text = "Packing...";

        RefreshPackedOverlayItems(true);
        SetZipperActorsVisible(true);
        UpdateBagZipStage(0f);

        var zipPoints = new[]
        {
            zipperPathCenter + new Vector2(zipperPathHalfSize.x, zipperPathHalfSize.y),
            zipperPathCenter + new Vector2(zipperPathHalfSize.x, -zipperPathHalfSize.y),
            zipperPathCenter + new Vector2(-zipperPathHalfSize.x, -zipperPathHalfSize.y),
            zipperPathCenter + new Vector2(-zipperPathHalfSize.x, zipperPathHalfSize.y)
        };

        var elapsed = 0f;
        while (elapsed < zipTravelSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, zipTravelSeconds));
            MoveZipperActors(zipPoints, t);
            UpdateBagZipStage(t);
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
            {
                var item = items[carryIndexes[i]];
                ApplyItemImage(packedOverlayItemImages[i], item.icon ? item.icon : item.detailImage, item.color);
            }
        }
    }

    private void SetZipperActorsVisible(bool visible)
    {
        if (zipperHookImage)
            zipperHookImage.gameObject.SetActive(visible && (zipperReReFrames == null || zipperReReFrames.Length == 0));

        if (zipperReReImage)
            zipperReReImage.gameObject.SetActive(visible);
    }

    private void MoveZipperActors(Vector2[] zipPoints, float progress)
    {
        var hasReReFrames = zipperReReFrames != null && zipperReReFrames.Length > 0;
        var hookStart = zipPoints != null && zipPoints.Length > 0 ? zipPoints[0] : Vector2.zero;
        const float jumpEnd = 0.26f;

        var hookPosition = hookStart;
        if (progress > jumpEnd)
        {
            var zipT = Mathf.InverseLerp(jumpEnd, ZipPathEnd, Mathf.Min(progress, ZipPathEnd));
            hookPosition = PointOnPolyline(zipPoints, zipT);
        }

        if (zipperHookImage)
            zipperHookImage.rectTransform.anchoredPosition = hookPosition;

        if (zipperReReImage)
        {
            if (hasReReFrames)
            {
                var frame = Mathf.Clamp(Mathf.FloorToInt(progress * zipTravelSeconds * zipReReFrameRate), 0, zipperReReFrames.Length - 1);
                zipperReReImage.sprite = zipperReReFrames[frame];
                zipperReReImage.rectTransform.sizeDelta = zipReReSize;
            }

            var hangOffset = new Vector2(0f, -88f + Mathf.Sin(progress * Mathf.PI * 6f) * 8f);
            var actorPosition = hookPosition + hangOffset;

            if (hasReReFrames)
            {
                if (progress < jumpEnd)
                {
                    var start = hookStart + new Vector2(110f, -232f);
                    var apex = hookStart + new Vector2(46f, -40f);
                    var land = hookStart + hangOffset;
                    var jumpT = EaseOut(progress / jumpEnd);
                    actorPosition = QuadraticBezier(start, apex, land, jumpT);
                }
                else if (progress > ZipPathEnd)
                {
                    var lastPoint = zipPoints != null && zipPoints.Length > 0 ? zipPoints[zipPoints.Length - 1] : hookStart;
                    var end = lastPoint + new Vector2(-140f, 178f);
                    actorPosition = Vector2.Lerp(lastPoint + hangOffset, end, EaseOut((progress - ZipPathEnd) / (1f - ZipPathEnd)));
                }
            }

            zipperReReImage.rectTransform.anchoredPosition = actorPosition;
            zipperReReImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, progress < jumpEnd ? 0f : Mathf.Sin(progress * Mathf.PI * 4f) * 7f);
        }
    }

    private void UpdateBagZipStage(float progress)
    {
        if (!openBagImage || bagZipStages == null || bagZipStages.Length < 2)
            return;

        var stageCount = Mathf.Min(bagZipStages.Length - 1, 5);
        var stageProgress = Mathf.Clamp01(progress / ZipPathEnd);
        var stageIndex = Mathf.Clamp(Mathf.FloorToInt(stageProgress * stageCount), 0, stageCount - 1);
        var stage = bagZipStages[stageIndex];
        if (!stage)
            return;

        openBagImage.sprite = stage;
        openBagImage.preserveAspect = true;
        FitBagStageImage(openBagImage, stage);
    }

    private void FitBagStageImage(Image image, Sprite sprite)
    {
        if (!image || !sprite || sprite.rect.width <= 0f)
            return;

        var width = Mathf.Max(1f, bagStageDisplayWidth);
        var height = width * sprite.rect.height / sprite.rect.width;
        image.rectTransform.sizeDelta = new Vector2(width, height);
        image.rectTransform.anchoredPosition = new Vector2(
            image.rectTransform.anchoredPosition.x,
            bagStageBottomY + height * 0.5f);
    }

    private static float EaseOut(float value)
    {
        value = Mathf.Clamp01(value);
        return 1f - (1f - value) * (1f - value);
    }

    private static Vector2 QuadraticBezier(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        t = Mathf.Clamp01(t);
        var a = Vector2.Lerp(start, control, t);
        var b = Vector2.Lerp(control, end, t);
        return Vector2.Lerp(a, b, t);
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
            bagStatusText.text = carryCount == 0 ? "アイテムが入っていません。" : "バッグの準備ができました。";
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
            {
                if (filled)
                {
                    var item = items[carryIndexes[i]];
                    ApplyItemImage(bagSlotImages[i], item.icon ? item.icon : item.detailImage, item.color);
                }
                else
                {
                    bagSlotImages[i].sprite = null;
                    bagSlotImages[i].color = new Color(1f, 1f, 1f, 0.18f);
                    bagSlotImages[i].preserveAspect = false;
                }
            }

            if (bagSlotTexts != null && i < bagSlotTexts.Length && bagSlotTexts[i])
                bagSlotTexts[i].text = filled ? items[carryIndexes[i]].displayName : "EMPTY";
        }

        if (bagStatusText)
            bagStatusText.text = carryCount + "/" + maxCarryItems + " packed";
    }

    private static void ApplyItemImage(Image image, Sprite sprite, Color placeholderColor)
    {
        if (!image)
            return;

        if (sprite)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            return;
        }

        image.color = placeholderColor;
        image.preserveAspect = false;
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
