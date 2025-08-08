using UnityEngine;
using TMPro;
using Naninovel;
using System.Collections;
using Febucci.UI; // ★ これを忘れずに

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;     // 本体表示（動かさない）
    [SerializeField] private AudioSource moneySE;           // 効果音
    [SerializeField] private TextAnimator_TMP floatingText; // 増減表示用（TextAnimator）
    [SerializeField] private CanvasGroup floatingGroup;     // フェードアウト用

    private bool initialized = false;
    private int lastAmount = 0;

    private void Start()
    {
        // 初期クリア（New Text対策）
        if (moneyText) moneyText.text = "";
        if (floatingText) floatingText.SetText("");
        if (floatingGroup) floatingGroup.alpha = 0f;

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateMoneyText;
            lastAmount = MoneyManager.Instance.CurrentMoney;
            UpdateMoneyText(lastAmount, playSound: false);
            initialized = true;
        }
    }

    public void UpdateMoneyText(int newAmount)
    {
        UpdateMoneyText(newAmount, playSound: true);
    }

    private void UpdateMoneyText(int newAmount, bool playSound)
    {
        // 本体は常に静止表示（<mspace>でラベルと金額の間に余裕）
        if (moneyText != null)
            moneyText.text = $"所持金:<mspace=0.6em></mspace>¥{newAmount:N0}";

        // 差分があるときだけポップアップ
        int diff = newAmount - lastAmount;
        if (diff != 0 && floatingText != null && floatingGroup != null)
        {
            string sign = diff > 0 ? "+" : "-";
            string color = diff > 0 ? "red" : "blue";

            // TextAnimatorのビルトイン：増＝<bounce>、減＝<slide>（下へ沈むようにyを負に）
            string animTag = diff > 0 ? "<bounce>" : "<slide y=-20>";

            // 例: <bounce><color=red>+¥1,000</color></bounce>
            string popup = $"{animTag}<color={color}>{sign}¥{Mathf.Abs(diff):N0}</color>";

            // 表示＆フェード開始
            floatingGroup.alpha = 1f;
            floatingText.SetText(popup); // ← TextAnimator経由でタグが効く
            StopAllCoroutines();
            StartCoroutine(FadeOutFloatingText());
        }

        if (playSound && moneySE != null && initialized)
            moneySE.Play();

        lastAmount = newAmount;
    }

    private IEnumerator FadeOutFloatingText()
    {
        // 少し見せてからフェード
        yield return new WaitForSeconds(1.0f);

        float duration = 0.5f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            floatingGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }
        floatingGroup.alpha = 0f;

        // 消した後はテキストもクリア（任意）
        floatingText.SetText("");
    }

    private void OnDestroy()
    {
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyText;
    }
}