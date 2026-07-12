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
    [SerializeField] private QuestDraft[] quests =
    {
        new QuestDraft { title = "Lost Potion", objective = "Find the lost potion.", progress = "0 / 1", hint = "ReRe knows a shortcut.", reward = "★ 200" },
        new QuestDraft { title = "Library Book", objective = "Return the library book.", progress = "1 / 3", hint = "Ask Yui about the reading room.", reward = "★ 150" },
        new QuestDraft { title = "Stray Kitten", objective = "Find a safe home for the kitten.", progress = "0 / 2", hint = "The kitten likes quiet places.", reward = "★ 250" },
        new QuestDraft { title = "Island Delivery", objective = "Deliver the package to the island.", progress = "2 / 6", hint = "Check the next ferry route.", reward = "★ 300" }
    };

    private int selectedQuestIndex;

    private void Awake()
    {
        BindButtons();
        BindQuestButtons();
        SelectQuest(0);
        ShowInbox();
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
        if (quests == null || quests.Length == 0) return;
        selectedQuestIndex = Mathf.Clamp(index, 0, quests.Length - 1);
        var quest = quests[selectedQuestIndex];

        SetText(activeQuestTitleText, quest.title);
        SetText(activeQuestObjectiveText, quest.objective);
        SetText(activeQuestProgressText, quest.progress);
        SetText(activeQuestHintText, quest.hint);
        SetText(activeQuestRewardText, quest.reward);

        if (inboxCardImages == null) return;
        for (var i = 0; i < inboxCardImages.Length; i++)
            if (inboxCardImages[i]) inboxCardImages[i].color = i == selectedQuestIndex
                ? new Color(1f, 0.78f, 0.78f, 1f)
                : new Color(1f, 0.96f, 0.84f, 1f);
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
