using System;
using System.Collections.Generic;
using Naninovel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Owns the manual save slots rendered inside MenuRootV2.</summary>
public sealed class MenuSaveLoadController : MonoBehaviour
{
    [Serializable]
    public sealed class SlotView
    {
        [SerializeField] private Button selectButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private TextMeshProUGUI detailLabel;
        [SerializeField] private RawImage preview;

        public SlotView(Button selectButton, Button deleteButton, TextMeshProUGUI detailLabel, RawImage preview = null)
        {
            this.selectButton = selectButton;
            this.deleteButton = deleteButton;
            this.detailLabel = detailLabel;
            this.preview = preview;
        }

        public Button SelectButton => selectButton;
        public Button DeleteButton => deleteButton;
        public TextMeshProUGUI DetailLabel => detailLabel;
        public RawImage Preview => preview;
    }

    [SerializeField] private SlotView[] slots = Array.Empty<SlotView>();
    [SerializeField] private Button saveModeButton;
    [SerializeField] private Button loadModeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationLabel;
    [SerializeField] private TextMeshProUGUI modeLabel;
    [SerializeField] private Toggle[] deleteSelections;
    [SerializeField] private GameObject deleteActions;
    [SerializeField] private Sprite saveNormalSprite, saveSelectedSprite, loadNormalSprite, loadSelectedSprite;
    private readonly HashSet<int> selectedForDeletion = new HashSet<int>();
    private bool deleteMode;
    private int[] pendingBulkDelete;
    public bool IsDeleteMode => deleteMode;
    public int SelectedDeleteCount => selectedForDeletion.Count;

    private MenuRootV2UI menuRoot;
    private IStateManager stateManager;
    private bool saveMode = true;
    private int pendingSlot = -1;
    private bool pendingDelete;

    public void Configure(SlotView[] slotViews, Button saveMode, Button loadMode, Button back,
        Button confirm, Button cancel, GameObject confirmation, TextMeshProUGUI confirmationText, TextMeshProUGUI modeText)
    {
        slots = slotViews;
        saveModeButton = saveMode;
        loadModeButton = loadMode;
        backButton = back;
        confirmButton = confirm;
        cancelButton = cancel;
        confirmationPanel = confirmation;
        confirmationLabel = confirmationText;
        modeLabel = modeText;
    }

    private void Awake()
    {
        menuRoot = GetComponentInParent<MenuRootV2UI>(true);
        BindButtons();
        HideConfirmation();
    }

    private void OnEnable()
    {
        ExitDeleteMode();
        HideConfirmation();
        Refresh().Forget();
    }
    private void OnDestroy() => UnbindButtons();

    private void BindButtons()
    {
        if (deleteSelections != null)
            for (var i = 0; i < deleteSelections.Length; i++)
            {
                var index = i;
                if (deleteSelections[i]) deleteSelections[i].onValueChanged.AddListener(value => SetDeleteSelected(index, value));
            }
        if (saveModeButton) saveModeButton.onClick.AddListener(ShowSaveMode);
        if (loadModeButton) loadModeButton.onClick.AddListener(ShowLoadMode);
        if (backButton) backButton.onClick.AddListener(Back);
        if (confirmButton) confirmButton.onClick.AddListener(Confirm);
        if (cancelButton) cancelButton.onClick.AddListener(HideConfirmation);
        for (var i = 0; i < slots.Length; i++)
        {
            var index = i;
            if (slots[i].SelectButton) slots[i].SelectButton.onClick.AddListener(() => Select(index));
            if (slots[i].DeleteButton) slots[i].DeleteButton.onClick.AddListener(() => RequestDelete(index));
        }
    }

    private void UnbindButtons()
    {
        if (saveModeButton) saveModeButton.onClick.RemoveListener(ShowSaveMode);
        if (loadModeButton) loadModeButton.onClick.RemoveListener(ShowLoadMode);
        if (backButton) backButton.onClick.RemoveListener(Back);
        if (confirmButton) confirmButton.onClick.RemoveListener(Confirm);
        if (cancelButton) cancelButton.onClick.RemoveListener(HideConfirmation);
    }

    public void ShowSaveMode() { saveMode = true; ExitDeleteMode(); HideConfirmation(); Refresh().Forget(); }
    public void ShowLoadMode() { saveMode = false; ExitDeleteMode(); HideConfirmation(); Refresh().Forget(); }
    public void Back() { HideConfirmation(); menuRoot?.ShowTop(); }

    private async UniTask Refresh()
    {
        if (saveModeButton && saveNormalSprite) saveModeButton.image.sprite = saveMode ? saveSelectedSprite : saveNormalSprite;
        if (loadModeButton && loadNormalSprite) loadModeButton.image.sprite = saveMode ? loadNormalSprite : loadSelectedSprite;
        if (modeLabel) modeLabel.text = saveMode ? "セーブ / 記録する" : "ロード / 記録から再開";
        if (!TryGetStateManager()) return;
        for (var i = 0; i < slots.Length; i++)
        {
            var slotId = stateManager.Configuration.IndexToSaveSlotId(i + 1);
            var exists = stateManager.GameSlotManager.SaveSlotExists(slotId);
            var label = slots[i].DetailLabel;
            if (slots[i].Preview) { slots[i].Preview.texture = null; slots[i].Preview.gameObject.SetActive(false); }
            if (label)
            {
                if (!exists) label.text = $"SLOT {i + 1:00}\n空きスロット";
                else
                {
                    var state = await stateManager.GameSlotManager.Load(slotId);
                    if (slots[i].Preview && state?.Thumbnail)
                    {
                        slots[i].Preview.texture = state.Thumbnail;
                        slots[i].Preview.gameObject.SetActive(true);
                        var aspect = slots[i].Preview.GetComponent<AspectRatioFitter>();
                        if (aspect) aspect.aspectRatio = (float)state.Thumbnail.width / state.Thumbnail.height;
                    }
                    var progress = state == null ? null : state.PlaybackSpot.ScriptPath;
                    label.text = $"SLOT {i + 1:00}  {MenuDaySaveBridge.FormatDay(state)}\n{state?.SaveDateTime:yyyy/MM/dd HH:mm}\n{(string.IsNullOrEmpty(progress) ? "進行状況の記録" : progress)}";
                }
            }
            if (slots[i].DeleteButton) slots[i].DeleteButton.gameObject.SetActive(exists);
        }
    }

    private void Select(int index)
    {
        if (deleteMode) { SetDeleteSelected(index, !selectedForDeletion.Contains(index)); return; }
        if (!TryGetStateManager()) return;
        var slotId = stateManager.Configuration.IndexToSaveSlotId(index + 1);
        if (!saveMode && !stateManager.GameSlotManager.SaveSlotExists(slotId)) return;
        if (saveMode && stateManager.GameSlotManager.SaveSlotExists(slotId))
            ShowConfirmation(index, false, $"スロット {index + 1:00} に上書きしますか？\n元の記録は戻せません。");
        else Execute(index, false).Forget();
    }

    private void RequestDelete(int index)
    {
        if (!TryGetStateManager()) return;
        var slotId = stateManager.Configuration.IndexToSaveSlotId(index + 1);
        if (stateManager.GameSlotManager.SaveSlotExists(slotId)) ShowConfirmation(index, true, $"スロット {index + 1:00} を削除しますか？\n削除した記録は戻せません。");
    }

    private void ShowConfirmation(int index, bool delete, string message)
    {
        pendingSlot = index;
        pendingDelete = delete;
        if (confirmationLabel) confirmationLabel.text = message;
        if (confirmationPanel) confirmationPanel.SetActive(true);
        SetBackgroundInteractable(false);
    }

    private void HideConfirmation()
    {
        pendingBulkDelete = null;
        pendingSlot = -1;
        if (confirmationPanel) confirmationPanel.SetActive(false);
        SetBackgroundInteractable(true);
    }

    private void Confirm()
    {
        if (pendingBulkDelete != null)
        {
            var targets = pendingBulkDelete;
            HideConfirmation();
            if (TryGetStateManager())
                foreach (var index in targets)
                    stateManager.GameSlotManager.DeleteSaveSlot(stateManager.Configuration.IndexToSaveSlotId(index + 1));
            ExitDeleteMode();
            Refresh().Forget();
            return;
        }
        if (pendingSlot < 0) return;
        var slot = pendingSlot;
        var delete = pendingDelete;
        HideConfirmation();
        Execute(slot, delete).Forget();
    }

    private async UniTask Execute(int index, bool delete)
    {
        if (!TryGetStateManager()) return;
        var slotId = stateManager.Configuration.IndexToSaveSlotId(index + 1);
        if (delete) stateManager.GameSlotManager.DeleteSaveSlot(slotId);
        else if (saveMode)
        {
            using (new InteractionBlocker())
                await stateManager.SaveGame(slotId);
        }
        else
        {
            menuRoot?.Hide();
            using (await LoadingScreen.Show())
                await stateManager.LoadGame(slotId);
            menuRoot?.Hide();
            return;
        }
        await Refresh();
    }

    private void SetBackgroundInteractable(bool interactable)
    {
        if (deleteSelections != null) foreach (var selection in deleteSelections) if (selection) selection.interactable = interactable;
        if (saveModeButton) saveModeButton.interactable = interactable;
        if (loadModeButton) loadModeButton.interactable = interactable;
        if (backButton) backButton.interactable = interactable;
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i].SelectButton) slots[i].SelectButton.interactable = interactable;
            if (slots[i].DeleteButton) slots[i].DeleteButton.interactable = interactable;
        }
    }

    private bool TryGetStateManager()
    {
        return Engine.Initialized && Engine.TryGetService<IStateManager>(out stateManager);
    }

    public void EnterDeleteMode()
    {
        if (confirmationPanel && confirmationPanel.activeSelf) return;
        deleteMode = true; selectedForDeletion.Clear();
        if (deleteActions) deleteActions.SetActive(true);
        if (deleteSelections != null) foreach (var selection in deleteSelections)
            if (selection) { selection.SetIsOnWithoutNotify(false); selection.gameObject.SetActive(true); }
    }

    public void ExitDeleteMode()
    {
        deleteMode = false; selectedForDeletion.Clear(); pendingBulkDelete = null;
        if (deleteActions) deleteActions.SetActive(false);
        if (deleteSelections != null) foreach (var selection in deleteSelections)
            if (selection) { selection.SetIsOnWithoutNotify(false); selection.gameObject.SetActive(false); }
    }

    public void SetDeleteSelected(int index, bool value)
    {
        if (!deleteMode || index < 0 || index >= slots.Length || (confirmationPanel && confirmationPanel.activeSelf)) return;
        if (value) selectedForDeletion.Add(index); else selectedForDeletion.Remove(index);
        if (deleteSelections != null && index < deleteSelections.Length && deleteSelections[index]) deleteSelections[index].SetIsOnWithoutNotify(value);
    }

    public void RequestBulkDelete()
    {
        if (!deleteMode || selectedForDeletion.Count == 0 || !TryGetStateManager()) return;
        var targets = new List<int>();
        foreach (var index in selectedForDeletion)
            if (stateManager.GameSlotManager.SaveSlotExists(stateManager.Configuration.IndexToSaveSlotId(index + 1))) targets.Add(index);
        if (targets.Count == 0) return;
        ShowConfirmation(-1, true, $"選択した {targets.Count} 件のデータを削除しますか？\n削除した記録は戻せません。");
        pendingBulkDelete = targets.ToArray();
    }
}
