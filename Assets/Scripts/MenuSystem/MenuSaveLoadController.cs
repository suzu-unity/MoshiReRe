using System;
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

        public SlotView(Button selectButton, Button deleteButton, TextMeshProUGUI detailLabel)
        {
            this.selectButton = selectButton;
            this.deleteButton = deleteButton;
            this.detailLabel = detailLabel;
        }

        public Button SelectButton => selectButton;
        public Button DeleteButton => deleteButton;
        public TextMeshProUGUI DetailLabel => detailLabel;
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
        HideConfirmation();
        Refresh().Forget();
    }
    private void OnDestroy() => UnbindButtons();

    private void BindButtons()
    {
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

    public void ShowSaveMode() { saveMode = true; HideConfirmation(); Refresh().Forget(); }
    public void ShowLoadMode() { saveMode = false; HideConfirmation(); Refresh().Forget(); }
    public void Back() { HideConfirmation(); menuRoot?.ShowTop(); }

    private async UniTask Refresh()
    {
        if (modeLabel) modeLabel.text = saveMode ? "セーブ / 記録する" : "ロード / 記録から再開";
        if (!TryGetStateManager()) return;
        for (var i = 0; i < slots.Length; i++)
        {
            var slotId = stateManager.Configuration.IndexToSaveSlotId(i + 1);
            var exists = stateManager.GameSlotManager.SaveSlotExists(slotId);
            var label = slots[i].DetailLabel;
            if (label)
            {
                if (!exists) label.text = $"SLOT {i + 1:00}\n空きスロット";
                else
                {
                    var state = await stateManager.GameSlotManager.Load(slotId);
                    var progress = state == null ? null : state.PlaybackSpot.ScriptPath;
                    label.text = $"SLOT {i + 1:00}\n{state?.SaveDateTime:yyyy/MM/dd HH:mm}\n{(string.IsNullOrEmpty(progress) ? "進行状況の記録" : progress)}";
                }
            }
            if (slots[i].DeleteButton) slots[i].DeleteButton.gameObject.SetActive(exists);
        }
    }

    private void Select(int index)
    {
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
        pendingSlot = -1;
        if (confirmationPanel) confirmationPanel.SetActive(false);
        SetBackgroundInteractable(true);
    }

    private void Confirm()
    {
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
}
