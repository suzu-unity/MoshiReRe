using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class AdviceClickTrigger : MonoBehaviour
{
    [SerializeField] private GameObject advicePanel;           // 吹き出しの箱（Panel等）
    [SerializeField] private TextMeshProUGUI adviceText;       // 吹き出しの本文
    [SerializeField] private float autoHideDelay = 3f;         // 自動消去までの秒数
    private Coroutine hideCoroutine;

    public void ShowAdvice(string message, bool autoHide = true)
    {
        if (!advicePanel || !adviceText) return;

        adviceText.text = message;
        advicePanel.SetActive(true);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        if (autoHide) hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    public void HideAdvice()
    {
        if (advicePanel) advicePanel.SetActive(false);
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);
        HideAdvice();
    }
}
