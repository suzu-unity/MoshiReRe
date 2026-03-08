using UnityEngine;
using TMPro;
using Naninovel;
using System.Collections;
using Febucci.UI;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private AudioSource moneySE;
    [SerializeField] private TextAnimator_TMP floatingText;
    [SerializeField] private CanvasGroup floatingGroup;

    private bool initialized = false;
    private bool subscribed = false;
    private int lastAmount = 0;

    private void Start()
    {
        if (moneyText) moneyText.text = "";
        if (floatingText) floatingText.SetText("");
        if (floatingGroup) floatingGroup.alpha = 0f;

        StartCoroutine(BindToMoneyManagerWhenReady());
    }

    private IEnumerator BindToMoneyManagerWhenReady()
    {
        while (MoneyManager.Instance == null)
            yield return null;

        MoneyManager.Instance.OnMoneyChanged += UpdateMoneyText;
        subscribed = true;

        lastAmount = MoneyManager.Instance.CurrentMoney;
        UpdateMoneyText(lastAmount, playSound: false);
        initialized = true;
    }

    public void UpdateMoneyText(int newAmount)
    {
        UpdateMoneyText(newAmount, playSound: true);
    }

    private void UpdateMoneyText(int newAmount, bool playSound)
    {
        if (moneyText != null)
            moneyText.text = $"所持金: <mspace=0.6em></mspace>¥{newAmount:N0}";

        int diff = newAmount - lastAmount;

        if (diff != 0 && floatingText != null && floatingGroup != null)
        {
            string sign = diff > 0 ? "+" : "-";
            string color = diff > 0 ? "red" : "blue";
            string animTag = diff > 0 ? "<bounce>" : "<slide y=-20>";
            string popup = $"{animTag}<color={color}>{sign}¥{Mathf.Abs(diff):N0}</color>";

            floatingGroup.alpha = 1f;
            floatingText.SetText(popup);
            StopAllCoroutines();
            StartCoroutine(FadeOutFloatingText());
        }

        if (diff != 0 && playSound && moneySE != null && initialized)
            moneySE.Play();

        lastAmount = newAmount;
    }

    private IEnumerator FadeOutFloatingText()
    {
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

        if (floatingText != null)
            floatingText.SetText("");
    }

    private void OnDestroy()
    {
        if (subscribed && MoneyManager.Instance != null)
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyText;
    }
}
