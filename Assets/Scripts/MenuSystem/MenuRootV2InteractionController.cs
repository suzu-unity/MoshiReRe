using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuRootV2InteractionController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speechText;
    [SerializeField] private GameObject confirmButtons;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private const string DefaultMessage = "気になるアイコンを選んでみて。今必要そうなことをReReが一緒に整理するよ。";

    private void Awake()
    {
        AutoWire();
        BindCardButtons();
        BindConfirmButtons();
        ShowMessage(DefaultMessage, false);
    }

    private void OnDestroy()
    {
        if (yesButton) yesButton.onClick.RemoveListener(HandleYes);
        if (noButton) noButton.onClick.RemoveListener(HandleNo);
    }

    private void AutoWire()
    {
        if (!speechText)
        {
            var speech = transform.Find("ReReSpeechBubble/Message");
            if (speech) speechText = speech.GetComponent<TextMeshProUGUI>();
        }

        if (!confirmButtons)
        {
            var confirm = transform.Find("ReReSpeechBubble/ConfirmButtons");
            if (confirm) confirmButtons = confirm.gameObject;
        }

        if (!yesButton)
        {
            var yes = transform.Find("ReReSpeechBubble/ConfirmButtons/YesButton");
            if (yes) yesButton = yes.GetComponent<Button>();
        }

        if (!noButton)
        {
            var no = transform.Find("ReReSpeechBubble/ConfirmButtons/NoButton");
            if (no) noButton = no.GetComponent<Button>();
        }
    }

    private void BindCardButtons()
    {
        foreach (var button in GetComponentsInChildren<Button>(true))
        {
            var name = button.name;
            if (name.StartsWith("ItemCard"))
                button.onClick.AddListener(() => ShowMessage("このアイテムは会話の流れを変えたい時に使うと良いかも。今使う？", true));
            else if (name.StartsWith("ContactCard"))
                button.onClick.AddListener(() => ShowMessage("この人は今のクエストに関係しているかも。会いに行ってみる？", true));
            else if (name.StartsWith("QuestCard"))
                button.onClick.AddListener(() => ShowMessage("このクエストを追いかけるなら、関連エリアへ移動できるよ。行ってみる？", true));
            else if (name.EndsWith("Area") || name == "VisitAreaButton" || name == "GoToAreaButton")
                button.onClick.AddListener(() => ShowMessage("この場所へ移動する？ 関連キャラや進行中の用事も一緒に確認しておくね。", true));
        }
    }

    private void BindConfirmButtons()
    {
        if (yesButton) yesButton.onClick.AddListener(HandleYes);
        if (noButton) noButton.onClick.AddListener(HandleNo);
    }

    private void HandleYes()
    {
        ShowMessage("OK。選んだ内容で進めるね。必要な情報も一緒に確認しておこう。", false);
    }

    private void HandleNo()
    {
        ShowMessage(DefaultMessage, false);
    }

    private void ShowMessage(string message, bool showConfirm)
    {
        if (speechText) speechText.text = message;
        if (confirmButtons) confirmButtons.SetActive(showConfirm);
    }
}
