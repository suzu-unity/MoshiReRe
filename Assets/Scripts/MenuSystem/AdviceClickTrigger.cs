using UnityEngine;

/// <summary>
/// クリックやホバーで AdviceBubble を表示するトリガー。
/// </summary>
public class AdviceClickTrigger : MonoBehaviour
{
    [SerializeField] private AdviceBubble adviceBubble; // 吹き出しの表示アニメーションを持つコンポーネント
    [SerializeField] private float autoHideDelay = 3f; // 自動非表示までの秒数

    /// <summary>
    /// メッセージを吹き出しに表示する。
    /// autoHide が true の場合は autoHideDelay 秒後に非表示。
    /// </summary>
    public void ShowAdvice(string message, bool autoHide = true)
    {
        if (adviceBubble != null)
        {
            adviceBubble.Show(message, autoHide, autoHideDelay);
        }
    }

    /// <summary>
    /// 吹き出しを即座に隠す。
    /// </summary>
    public void HideAdvice()
    {
        adviceBubble?.Hide();
    }
}
