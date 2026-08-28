using System;
using System.Collections.Generic;
using Naninovel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoshiReRe.ItemPresentation
{
    /// <summary>
    /// Runtime-only inventory presentation modal used by Naninovel conversation beats.
    /// It builds a small uGUI view under the active Naninovel canvas, so no existing menu
    /// prefab or generated image asset is required.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ItemPresentationUI : MonoBehaviour
    {
        private const string SelectionSfxPath = "SFX/Title/title_cursor_move";
        private const string ConfirmSfxPath = "SFX/Title/title_confirm";

        [Header("Optional scene wiring")]
        [SerializeField] private InventoryDatabase inventoryDatabase;
        [SerializeField] private Canvas hostCanvas;

        [Header("Presentation")]
        [SerializeField] private string titleText = "提示するアイテムを選ぶ";
        [SerializeField] private string instructionText = "↑↓ / W S で選択　Enter で提示　Esc で戻る";
        [SerializeField] private Color overlayColor = new Color(0.02f, 0.04f, 0.09f, 0.78f);
        [SerializeField] private Color panelColor = new Color(0.07f, 0.11f, 0.2f, 0.98f);
        [SerializeField] private Color cardColor = new Color(0.12f, 0.18f, 0.29f, 1f);
        [SerializeField] private Color selectedCardColor = new Color(0.14f, 0.47f, 0.66f, 1f);
        [SerializeField] private Color accentColor = new Color(0.52f, 0.9f, 1f, 1f);

        private static ItemPresentationUI active;

        private readonly List<Button> itemButtons = new List<Button>();
        private readonly List<Image> itemCardImages = new List<Image>();

        private GameObject overlayRoot;
        private Canvas overlayCanvas;
        private RectTransform cardsRoot;
        private Image detailIcon;
        private TMP_Text detailIconLabel;
        private TMP_Text detailName;
        private TMP_Text detailSummary;
        private TMP_Text emptyText;
        private Button presentButton;
        private Button backButton;
        private TMP_Text statusText;
        private List<InventoryItem> candidates = new List<InventoryItem>();
        private readonly List<MenuEsc> suspendedMenuEsc = new List<MenuEsc>();
        private int selectedIndex = -1;
        private bool showing;
        private bool hasResult;
        private ItemPresentationOutcome result;

        public static ItemPresentationUI Active => active;
        public bool IsShowing => showing;
        public int SelectedIndex => selectedIndex;

        private void Awake()
        {
            if (active != null && active != this)
            {
                Destroy(gameObject);
                return;
            }

            active = this;
        }

        private void Update()
        {
            if (!showing || candidates.Count == 0)
                return;

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                SelectIndex(selectedIndex - 1, true);
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                SelectIndex(selectedIndex + 1, true);
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
                PresentSelected();
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
                CancelPresentation();
        }

        private void OnDestroy()
        {
            ResumeMenuEsc();
            if (active == this)
                active = null;
        }

        /// <summary>Finds the loaded inventory asset without adding an asset-path dependency.</summary>
        public static InventoryDatabase ResolveLoadedDatabase(InventoryDatabase preferred = null)
        {
            if (preferred != null)
                return preferred;

            var databases = Resources.FindObjectsOfTypeAll<InventoryDatabase>();
            InventoryDatabase first = null;
            for (var i = 0; i < databases.Length; i++)
            {
                var candidate = databases[i];
                if (candidate == null)
                    continue;

                first ??= candidate;
                if (candidate.GetAcquired().Count > 0)
                    return candidate;
            }

            return first;
        }

        public static async UniTask<ItemPresentationOutcome> PresentAsync(
            InventoryDatabase database,
            AsyncToken asyncToken = default)
        {
            var presenter = active;
            if (presenter == null)
            {
                presenter = FindFirstObjectByType<ItemPresentationUI>(FindObjectsInactive.Include);
                if (presenter == null)
                    presenter = new GameObject("ItemPresentationPresenter").AddComponent<ItemPresentationUI>();
            }

            return await presenter.BeginAndWaitAsync(database, asyncToken);
        }

        private async UniTask<ItemPresentationOutcome> BeginAndWaitAsync(
            InventoryDatabase database,
            AsyncToken asyncToken)
        {
            database = ResolveLoadedDatabase(database != null ? database : inventoryDatabase);
            candidates = new List<InventoryItem>(ItemPresentationFlow.GetCandidates(database));
            if (candidates.Count == 0)
                return ItemPresentationOutcome.NoItems();

            EnsureUi();
            PopulateCandidates();
            showing = true;
            hasResult = false;
            selectedIndex = 0;
            result = ItemPresentationOutcome.Cancelled();
            overlayRoot.SetActive(true);
            SuspendMenuEsc();
            RefreshSelection(false);

            while (!hasResult)
            {
                if (asyncToken.Canceled)
                {
                    Complete(ItemPresentationOutcome.Cancelled(), true);
                    asyncToken.ThrowIfCanceled();
                }

                await AsyncUtils.WaitEndOfFrame();
            }

            showing = false;
            ResumeMenuEsc();
            if (overlayRoot != null)
                overlayRoot.SetActive(false);
            return result;
        }

        public void SelectIndex(int index)
        {
            SelectIndex(index, true);
        }

        private void SelectIndex(int index, bool playSfx)
        {
            if (!showing || candidates.Count == 0)
                return;

            if (index < 0)
                index = candidates.Count - 1;
            if (index >= candidates.Count)
                index = 0;

            selectedIndex = index;
            RefreshSelection(playSfx);
        }

        public void PresentSelected()
        {
            if (!showing || selectedIndex < 0 || selectedIndex >= candidates.Count)
                return;

            PlaySfx(ConfirmSfxPath);
            Complete(ItemPresentationOutcome.Presented(candidates[selectedIndex]), true);
        }

        public void CancelPresentation()
        {
            if (!showing)
                return;

            PlaySfx(ConfirmSfxPath);
            Complete(ItemPresentationOutcome.Cancelled(), true);
        }

        private void Complete(ItemPresentationOutcome value, bool hideImmediately)
        {
            if (hasResult)
                return;

            result = value;
            hasResult = true;
            if (hideImmediately)
            {
                showing = false;
                ResumeMenuEsc();
                if (overlayRoot != null)
                    overlayRoot.SetActive(false);
            }
        }

        private void SuspendMenuEsc()
        {
            suspendedMenuEsc.Clear();
            var menus = FindObjectsByType<MenuEsc>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < menus.Length; i++)
            {
                var menu = menus[i];
                if (menu == null || !menu.enabled)
                    continue;
                menu.enabled = false;
                suspendedMenuEsc.Add(menu);
            }
        }

        private void ResumeMenuEsc()
        {
            for (var i = 0; i < suspendedMenuEsc.Count; i++)
                if (suspendedMenuEsc[i] != null)
                    suspendedMenuEsc[i].enabled = true;
            suspendedMenuEsc.Clear();
        }

        private void EnsureUi()
        {
            if (overlayRoot != null)
                return;

            hostCanvas = ResolveHostCanvas(hostCanvas);
            if (hostCanvas == null)
                hostCanvas = CreateFallbackCanvas();

            EnsureEventSystem();

            overlayRoot = new GameObject("ItemPresentationOverlay", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(Image));
            overlayRoot.transform.SetParent(hostCanvas.transform, false);
            overlayRoot.transform.SetAsLastSibling();

            var rootRect = overlayRoot.GetComponent<RectTransform>();
            SetStretch(rootRect);

            overlayCanvas = overlayRoot.GetComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = hostCanvas.sortingOrder + 20;

            var group = overlayRoot.GetComponent<CanvasGroup>();
            group.interactable = true;
            group.blocksRaycasts = true;

            var blocker = overlayRoot.GetComponent<Image>();
            blocker.color = overlayColor;
            blocker.raycastTarget = true;

            var panel = CreateImage("Panel", overlayRoot.transform, panelColor);
            SetCentered(panel.rectTransform, new Vector2(1240f, 760f));

            var header = CreateEmpty("Header", panel.transform);
            SetRect(header.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(36f, -126f), new Vector2(-36f, -30f));
            var headerTitle = CreateText(header.transform, "Title", titleText, 34f, accentColor, TextAlignmentOptions.Left);
            SetRect(headerTitle.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var hint = CreateText(header.transform, "Hint", instructionText, 18f, new Color(0.76f, 0.84f, 0.92f, 1f), TextAlignmentOptions.Right);
            SetRect(hint.rectTransform, new Vector2(0.35f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            var body = CreateEmpty("Body", panel.transform);
            SetRect(body.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(36f, 124f), new Vector2(-36f, -144f));

            var listPanel = CreateImage("ItemListPanel", body.transform, new Color(0.04f, 0.07f, 0.13f, 0.9f));
            SetRect(listPanel.rectTransform, new Vector2(0f, 0f), new Vector2(0.48f, 1f), Vector2.zero, new Vector2(-12f, 0f));
            var listLabel = CreateText(listPanel.rectTransform, "ListLabel", "所持品", 22f, Color.white, TextAlignmentOptions.Left);
            SetRect(listLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(22f, -58f), new Vector2(-22f, -18f));

            var cardContainer = CreateEmpty("Cards", listPanel.transform);
            SetRect(cardContainer.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(18f, 18f), new Vector2(-18f, -72f));
            cardsRoot = cardContainer.GetComponent<RectTransform>();
            var layout = cardContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            emptyText = CreateText(listPanel.rectTransform, "Empty", "提示できる所持品がありません。", 20f, new Color(0.78f, 0.84f, 0.9f, 1f), TextAlignmentOptions.Center);
            SetRect(emptyText.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(16f, -30f), new Vector2(-16f, 30f));

            var detailPanel = CreateImage("DetailPanel", body.transform, new Color(0.08f, 0.12f, 0.21f, 0.96f));
            SetRect(detailPanel.rectTransform, new Vector2(0.48f, 0f), new Vector2(1f, 1f), new Vector2(12f, 0f), Vector2.zero);
            detailIcon = CreateImage("DetailIcon", detailPanel.transform, Color.white);
            SetRect(detailIcon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-90f, -230f), new Vector2(90f, -50f));
            detailIconLabel = CreateText(detailIcon.rectTransform, "Placeholder", "?", 60f, Color.white, TextAlignmentOptions.Center);
            SetStretch(detailIconLabel.rectTransform);
            detailName = CreateText(detailPanel.rectTransform, "DetailName", string.Empty, 28f, accentColor, TextAlignmentOptions.Center);
            SetRect(detailName.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -286f), new Vector2(-18f, -242f));
            detailSummary = CreateText(detailPanel.rectTransform, "DetailSummary", string.Empty, 20f, Color.white, TextAlignmentOptions.TopLeft);
            detailSummary.enableWordWrapping = true;
            SetRect(detailSummary.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(26f, 34f), new Vector2(-26f, -304f));

            var footer = CreateEmpty("Footer", panel.transform);
            SetRect(footer.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(36f, 28f), new Vector2(-36f, 112f));
            statusText = CreateText(footer.transform, "Status", string.Empty, 18f, new Color(0.75f, 0.84f, 0.92f, 1f), TextAlignmentOptions.Left);
            SetRect(statusText.rectTransform, new Vector2(0f, 0f), new Vector2(0.55f, 1f), Vector2.zero, Vector2.zero);
            presentButton = CreateButton(footer.transform, "Present", "このアイテムを提示する", accentColor, PresentSelected);
            SetRect(presentButton.GetComponent<RectTransform>(), new Vector2(0.58f, 0f), new Vector2(0.8f, 1f), Vector2.zero, Vector2.zero);
            backButton = CreateButton(footer.transform, "Back", "戻る", Color.white, CancelPresentation);
            SetRect(backButton.GetComponent<RectTransform>(), new Vector2(0.82f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            overlayRoot.SetActive(false);
        }

        private void PopulateCandidates()
        {
            ClearChildren(cardsRoot);
            itemButtons.Clear();
            itemCardImages.Clear();

            for (var i = 0; i < candidates.Count; i++)
            {
                var item = candidates[i];
                var index = i;
                var card = CreateButton(cardsRoot, "ItemCard_" + i, item.GetDisplayName(), Color.white, () => SelectIndex(index, true));
                var cardRect = card.GetComponent<RectTransform>();
                var element = card.gameObject.AddComponent<LayoutElement>();
                element.minHeight = 88f;
                element.preferredHeight = 88f;

                var cardImage = card.GetComponent<Image>();
                itemCardImages.Add(cardImage);
                itemButtons.Add(card);

                var icon = CreateImage("Icon", card.transform, Color.white);
                SetRect(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, -30f), new Vector2(76f, 30f));
                var placeholder = CreateText(icon.rectTransform, "Placeholder", "?", 30f, Color.white, TextAlignmentOptions.Center);
                SetStretch(placeholder.rectTransform);
                ApplyItemImage(icon, placeholder, item);

                var label = card.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = item.GetDisplayName();
                    label.fontSize = 22f;
                    label.alignment = TextAlignmentOptions.Left;
                    SetRect(label.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(104f, 0f), new Vector2(-18f, 0f));
                }
                card.navigation = new Navigation { mode = Navigation.Mode.None };
            }

            if (emptyText != null)
                emptyText.gameObject.SetActive(candidates.Count == 0);
        }

        private void RefreshSelection(bool playSfx)
        {
            for (var i = 0; i < itemCardImages.Count; i++)
                if (itemCardImages[i] != null)
                    itemCardImages[i].color = i == selectedIndex ? selectedCardColor : cardColor;

            if (selectedIndex >= 0 && selectedIndex < candidates.Count)
            {
                var item = candidates[selectedIndex];
                ApplyItemImage(detailIcon, detailIconLabel, item);
                if (detailName != null)
                    detailName.text = item.GetDisplayName();
                if (detailSummary != null)
                    detailSummary.text = string.IsNullOrWhiteSpace(item.summary) ? "このアイテムを相手に見せる。" : item.summary;
                if (statusText != null)
                    statusText.text = (selectedIndex + 1) + " / " + candidates.Count;
                if (presentButton != null)
                    presentButton.interactable = true;
                if (EventSystem.current != null && selectedIndex < itemButtons.Count)
                    EventSystem.current.SetSelectedGameObject(itemButtons[selectedIndex].gameObject);
            }

            if (playSfx)
                PlaySfx(SelectionSfxPath);
        }

        private static Canvas ResolveHostCanvas(Canvas preferred)
        {
            if (preferred != null && preferred.isActiveAndEnabled)
                return preferred;

            var activeScene = SceneManager.GetActiveScene();
            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Canvas best = null;
            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (canvas == null || canvas.gameObject.scene != activeScene || !canvas.gameObject.activeInHierarchy)
                    continue;
                if (best == null || canvas.sortingOrder > best.sortingOrder || canvas.isRootCanvas && !best.isRootCanvas)
                    best = canvas;
            }

            return best;
        }

        private static Canvas CreateFallbackCanvas()
        {
            var canvasObject = new GameObject("ItemPresentationCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < systems.Length; i++)
                if (systems[i] != null && systems[i].gameObject.activeInHierarchy)
                    return;

            var eventObject = new GameObject("ItemPresentationEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventObject.hideFlags = HideFlags.DontSave;
        }

        private void PlaySfx(string path)
        {
            if (!Engine.Initialized || string.IsNullOrWhiteSpace(path))
                return;
            if (Engine.TryGetService<IAudioManager>(out var audio) && audio != null)
                audio.PlaySfxFast(path, 1f, null, true, true);
        }

        private static GameObject CreateEmpty(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(
            Transform parent,
            string name,
            string value,
            float size,
            Color color,
            TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = value ?? string.Empty;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.enableWordWrapping = true;
            return text;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Color textColor,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.12f, 0.18f, 0.29f, 1f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            var text = CreateText(go.transform, "Label", label, 20f, textColor, TextAlignmentOptions.Center);
            SetStretch(text.rectTransform);
            return button;
        }

        private static void ApplyItemImage(Image image, TMP_Text placeholder, InventoryItem item)
        {
            if (image == null || item == null)
                return;

            if (item.icon != null)
            {
                image.sprite = item.icon;
                image.color = Color.white;
                image.preserveAspect = true;
                if (placeholder != null)
                    placeholder.gameObject.SetActive(false);
            }
            else
            {
                image.sprite = null;
                image.color = ItemPresentationFlow.GetPlaceholderColor(item.id);
                image.preserveAspect = false;
                if (placeholder != null)
                {
                    placeholder.text = GetPlaceholderLabel(item);
                    placeholder.gameObject.SetActive(true);
                }
            }
        }

        private static string GetPlaceholderLabel(InventoryItem item)
        {
            var name = item != null ? item.GetDisplayName() : string.Empty;
            return string.IsNullOrWhiteSpace(name) ? "?" : name.Substring(0, 1);
        }

        private static void ClearChildren(Transform root)
        {
            if (root == null)
                return;

            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }

        private static void SetStretch(RectTransform rect)
        {
            SetRect(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private static void SetCentered(RectTransform rect, Vector2 size)
        {
            SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), -size * 0.5f, size * 0.5f);
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            if (rect == null)
                return;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
        }
    }
}
