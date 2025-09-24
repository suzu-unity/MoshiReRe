using UnityEngine;

public class AdvicePop : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AdviceBubble adviceBubble;

    [Header("Messages")]
    [TextArea]
    [SerializeField] private string[] messages;

    // 「最初のメッセージを出しっぱなし」にするか？
    // Nullable (bool?) だと Show(bool) に渡す時に CS1503 が出るので bool に統一
    [SerializeField] private bool firstAdviceSticky = true;

    private int index = 0;

    private void OnEnable()
    {
        if (!adviceBubble) return;
        if (messages == null || messages.Length == 0) return;

        // Show(message, autoHide)
        // firstAdviceSticky が true なら出しっぱなしにしたいので autoHide=false
        // false の時は自動で消したければ true に（必要なければ false のままでもOK）
        bool autoHide = !firstAdviceSticky;
        adviceBubble.Show(messages[index], autoHide);
    }

    public void NextAdvice()
    {
        if (!adviceBubble) return;
        if (messages == null || messages.Length == 0) return;

        index = (index + 1) % messages.Length;
        // 次のメッセージは基本出しっぱなし（必要あれば第2引数を true に）
        adviceBubble.Show(messages[index], false);
    }

    public void HideAdvice()
    {
        if (adviceBubble) adviceBubble.Hide();
    }
}
