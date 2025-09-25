using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// アイコンや立ち絵がクリックされたときに説明テキストを表示するトリガー。
/// 表示ロジックを一本化するため、MenuRootUIから移動してきたロジックも含まれます。
/// </summary>
public class AdviceClickTrigger : MonoBehaviour, IPointerClickHandler
{
    // 説明用の吹き出しパネル（TextMeshProUGUI を子に持つ GameObject）。
    [SerializeField]
    private GameObject advicePanel;

    // 説明文を表示するためのテキストコンポーネント（TextMeshProUGUI）。
    [SerializeField]
    private TMPro.TextMeshProUGUI adviceText;

    // このアイコン/立ち絵用の説明文。
    [TextArea]
    public string message;

    // 自動的に非表示にするまでの時間（秒）。
    [SerializeField]
    private float autoHideDelay = 3f;

    // 現在実行中の非表示用コルーチン。
    private Coroutine hideCoroutine;

    /// <summary>
    /// IPointerClickHandler の実装。クリック時に呼び出される。
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        ShowAdvice();
    }

    /// <summary>
    /// 吹き出しを表示し、必要であれば既存のコルーチンを停止して新しいタイマーを開始します。
    /// </summary>
    public void ShowAdvice()
    {
        if (advicePanel == null || adviceText == null) return;

        adviceText.text = message;
        advicePanel.SetActive(true);

        // 既存のタイマーがあれば停止してリセットする
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    /// <summary>
    /// 吹き出しを即座に非表示にします（タイマーをリセットします）。
    /// </summary>
    public void HideAdvice()
    {
        if (advicePanel != null)
        {
            advicePanel.SetActive(false);
        }
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    /// <summary>
    /// 指定秒数待ってから吹き出しを非表示にするコルーチン。
    /// </summary>
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);
        HideAdvice();
    }
}
