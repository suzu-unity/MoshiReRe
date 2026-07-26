using UnityEngine;
using UnityEngine.UI;

public enum MenuStatusAdviceType
{
    Guts,
    Intelligence,
    Attention,
    Technique,
    Strength,
    Portrait
}

public class MenuReReAdvisor : MonoBehaviour
{
    public static MenuReReAdvisor Instance { get; private set; }

    [SerializeField] private AdviceClickTrigger adviceTrigger;
    [SerializeField] private Button reReButton;
    [TextArea] [SerializeField] private string defaultMessage = "気になるところをタップしてみて。ReReが補足するよ。";

    private string lastMessage;

    private void Awake()
    {
        Instance = this;

        if (!reReButton)
            reReButton = GetComponentInChildren<Button>(true);

        if (reReButton)
        {
            reReButton.onClick.RemoveListener(RepeatLastMessage);
            reReButton.onClick.AddListener(RepeatLastMessage);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (reReButton)
            reReButton.onClick.RemoveListener(RepeatLastMessage);
    }

    public void ShowMessage(string message, bool autoHide = false)
    {
        lastMessage = string.IsNullOrWhiteSpace(message) ? defaultMessage : message;
        adviceTrigger?.ShowAdvice(lastMessage, autoHide);
    }

    public void ShowStatusHint(MenuStatusAdviceType type)
    {
        ShowMessage(BuildStatusMessage(type), false);
    }

    public void ShowCharacterHint(CharacterInfo character)
    {
        if (!character)
        {
            ShowMessage(defaultMessage, true);
            return;
        }

        var label = string.IsNullOrWhiteSpace(character.displayName) ? character.id : character.displayName;
        if (!string.IsNullOrWhiteSpace(character.summary))
            ShowMessage($"{label}\n{character.summary}", false);
        else
            ShowMessage($"{label}\nこの人は会話の選び方で印象が変わりそう。", false);
    }

    public void ShowItemHint(InventoryItem item)
    {
        if (!item)
        {
            ShowMessage(defaultMessage, true);
            return;
        }

        var label = item.GetDisplayName();
        if (!string.IsNullOrWhiteSpace(item.summary))
            ShowMessage($"{label}\n{item.summary}", false);
        else
            ShowMessage($"{label}\n今は使いどころを見極めたいね。", false);
    }

    public void RepeatLastMessage()
    {
        ShowMessage(lastMessage, false);
    }

    private string BuildStatusMessage(MenuStatusAdviceType type)
    {
        int value = GetStatusValue(type);
        string name = GetStatusName(type);

        if (type == MenuStatusAdviceType.Portrait)
            return "主人公の今の調子がここに集約されているよ。気になる能力から確認してみよう。";

        if (value <= 2)
            return $"{name}はかなり低め。今はそこを補う行動を意識した方がよさそう。";
        if (value <= 4)
            return $"{name}が少し足りていないかも。次の選択で補強できると安心。";
        if (value <= 7)
            return $"{name}は平均的。困りはしないけど、伸ばす余地はまだあるね。";
        return $"{name}はかなり頼もしい数字。この調子なら強みとして使っていけそう。";
    }

    private int GetStatusValue(MenuStatusAdviceType type)
    {
        if (StatusManager.Instance == null) return 0;

        switch (type)
        {
            case MenuStatusAdviceType.Guts: return StatusManager.Instance.Guts;
            case MenuStatusAdviceType.Intelligence: return StatusManager.Instance.Intelligence;
            case MenuStatusAdviceType.Attention: return StatusManager.Instance.Attention;
            case MenuStatusAdviceType.Technique: return StatusManager.Instance.Technique;
            case MenuStatusAdviceType.Strength: return StatusManager.Instance.Strength;
            default: return 0;
        }
    }

    private static string GetStatusName(MenuStatusAdviceType type)
    {
        switch (type)
        {
            case MenuStatusAdviceType.Guts: return "胆力";
            case MenuStatusAdviceType.Intelligence: return "知力";
            case MenuStatusAdviceType.Attention: return "注意力";
            case MenuStatusAdviceType.Technique: return "技術力";
            case MenuStatusAdviceType.Strength: return "筋力";
            default: return "ステータス";
        }
    }
}
