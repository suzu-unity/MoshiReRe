using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestMenuController : MonoBehaviour
{
    [System.Serializable]
    private struct QuestDraft
    {
        public string title;
        public string objective;
        public string progress;
        public string hint;
        public string reward;
    }

    [SerializeField] private GameObject inboxRoot;
    [SerializeField] private GameObject caseBoardRoot;
    [SerializeField] private Button inboxTabButton;
    [SerializeField] private Button caseBoardTabButton;
    [SerializeField] private Button[] inboxQuestButtons;
    [SerializeField] private Image[] inboxCardImages;
    [SerializeField] private TMP_Text activeQuestTitleText;
    [SerializeField] private TMP_Text activeQuestObjectiveText;
    [SerializeField] private TMP_Text activeQuestProgressText;
    [SerializeField] private TMP_Text activeQuestHintText;
    [SerializeField] private TMP_Text activeQuestRewardText;
    // 旧prefabとの互換性のためフィールドは残す。表示はMainQuestStateのみを使用する。
    [SerializeField] private QuestDraft[] quests = new QuestDraft[0];

    private int selectedQuestIndex;

    private void Awake()
    {
        BindButtons();
        BindQuestButtons();
        Refresh(MainQuestState.Current);
        ShowInbox();
    }

    private void OnEnable()
    {
        MainQuestState.OnChanged += Refresh;
        MainQuestState.SyncFromNaninovel();
        Refresh(MainQuestState.Current);
    }

    private void OnDisable()
    {
        MainQuestState.OnChanged -= Refresh;
    }

    private void OnDestroy()
    {
        if (inboxTabButton) inboxTabButton.onClick.RemoveListener(ShowInbox);
        if (caseBoardTabButton) caseBoardTabButton.onClick.RemoveListener(ShowCaseBoard);
    }

    public void ShowInbox()
    {
        if (inboxRoot) inboxRoot.SetActive(true);
        if (caseBoardRoot) caseBoardRoot.SetActive(false);
        SetTabState(inboxTabButton, true);
        SetTabState(caseBoardTabButton, false);
    }

    public void ShowCaseBoard()
    {
        if (inboxRoot) inboxRoot.SetActive(false);
        if (caseBoardRoot) caseBoardRoot.SetActive(true);
        SetTabState(inboxTabButton, false);
        SetTabState(caseBoardTabButton, true);
    }

    private void BindButtons()
    {
        if (inboxTabButton) inboxTabButton.onClick.AddListener(ShowInbox);
        if (caseBoardTabButton) caseBoardTabButton.onClick.AddListener(ShowCaseBoard);
    }

    private void BindQuestButtons()
    {
        if (inboxQuestButtons == null) return;
        for (var i = 0; i < inboxQuestButtons.Length; i++)
        {
            var index = i;
            if (inboxQuestButtons[i]) inboxQuestButtons[i].onClick.AddListener(() => SelectQuest(index));
        }
    }

    private void SelectQuest(int index)
    {
        selectedQuestIndex = Mathf.Max(0, index);
        Refresh(MainQuestState.Current);

        if (inboxCardImages == null) return;
        for (var i = 0; i < inboxCardImages.Length; i++)
            if (inboxCardImages[i]) inboxCardImages[i].color = i == selectedQuestIndex
                ? new Color(1f, 0.78f, 0.78f, 1f)
                : new Color(1f, 0.96f, 0.84f, 1f);
    }

    private void Refresh(MainQuestState.Data quest)
    {
        var assigned = quest.IsAssigned;
        SetText(activeQuestTitleText, assigned ? quest.Title : "メインクエストはありません");
        SetText(activeQuestObjectiveText, assigned ? quest.Objective : "シナリオ中にクエストが設定されます");
        SetText(activeQuestProgressText, assigned ? MainQuestState.FormatDeadline(quest.DeadlineDays) : "期限: --");
        SetText(activeQuestHintText, assigned ? "期限までに進めよう" : "現在のクエストを確認しよう");
        SetText(activeQuestRewardText, assigned ? "メインクエスト" : string.Empty);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text) text.text = value;
    }

    private static void SetTabState(Button button, bool selected)
    {
        if (!button) return;

        var colors = button.colors;
        colors.normalColor = selected ? Color.white : new Color(0.82f, 0.78f, 0.88f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.86f, 0.84f, 0.92f, 1f);
        button.colors = colors;
    }
}
