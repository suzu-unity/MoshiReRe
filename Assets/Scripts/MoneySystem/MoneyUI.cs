using System.Collections;
using System.Text;
using Febucci.UI;
using Naninovel;
using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private AudioSource moneySE;
    [SerializeField] private TextAnimator_TMP floatingText;
    [SerializeField] private CanvasGroup floatingGroup;

    private bool initialized;
    private bool subscribed;
    private int lastAmount;
    private Coroutine bindingRoutine;
    private Coroutine floatingRoutine;
    private RectTransform floatingRect;
    private Vector2 floatingRestPosition;

    private void Start()
    {
        if (moneyText) moneyText.text = "¥ 000,000";
        if (floatingText) floatingText.SetText(string.Empty);
        if (floatingGroup) floatingGroup.alpha = 0f;

        if (floatingText) floatingRect = floatingText.GetComponent<RectTransform>();
        if (floatingRect) floatingRestPosition = floatingRect.anchoredPosition;
        bindingRoutine = StartCoroutine(BindToMoneyManagerWhenReady());
    }

    private IEnumerator BindToMoneyManagerWhenReady()
    {
        while (MoneyManager.Instance == null)
            yield return null;

        MoneyManager.Instance.OnMoneyChanged += UpdateMoneyText;
        subscribed = true;

        lastAmount = MoneyManager.Instance.CurrentMoney;
        UpdateMoneyText(lastAmount, false);
        initialized = true;
        bindingRoutine = null;
    }

    public void UpdateMoneyText(int newAmount)
    {
        UpdateMoneyText(newAmount, true);
    }

    private void UpdateMoneyText(int newAmount, bool playSound)
    {
        if (moneyText != null)
            moneyText.text = $"<mspace=0.62em>¥ {FormatMoney(newAmount)}</mspace>";

        int diff = newAmount - lastAmount;
        if (diff != 0 && floatingText != null && floatingGroup != null)
        {
            string sign = diff > 0 ? "+" : "-";
            string color = diff > 0 ? "#42E8FF" : "#FF8A5B";
            string popup = $"<mspace=0.62em><color={color}>{sign}¥{FormatMoney(Mathf.Abs(diff))}</color></mspace>";

            floatingGroup.alpha = 1f;
            floatingText.SetText(popup);
            if (floatingRoutine != null)
                StopCoroutine(floatingRoutine);
            floatingRoutine = StartCoroutine(FadeOutFloatingText());
        }

        if (diff != 0 && playSound && moneySE != null && initialized)
            moneySE.Play();

        lastAmount = newAmount;
    }

    private IEnumerator FadeOutFloatingText()
    {
        if (floatingRect)
            floatingRect.anchoredPosition = floatingRestPosition + new Vector2(0f, -10f);

        const float settleDuration = 0.18f;
        float t = 0f;
        while (t < settleDuration)
        {
            t += Time.unscaledDeltaTime;
            if (floatingRect)
                floatingRect.anchoredPosition = Vector2.Lerp(floatingRestPosition + new Vector2(0f, -10f), floatingRestPosition, t / settleDuration);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.55f);

        const float fadeDuration = 0.32f;
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            floatingGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        floatingGroup.alpha = 0f;
        if (floatingText != null)
            floatingText.SetText(string.Empty);
        floatingRoutine = null;
    }

    private static string FormatMoney(int amount)
    {
        string digits = Mathf.Max(0, amount).ToString().PadLeft(6, '0');
        var formatted = new StringBuilder(digits.Length + digits.Length / 3);
        for (int i = 0; i < digits.Length; i++)
        {
            if (i > 0 && (digits.Length - i) % 3 == 0)
                formatted.Append(',');
            formatted.Append(digits[i]);
        }

        return formatted.ToString();
    }

    private void OnDestroy()
    {
        if (bindingRoutine != null) StopCoroutine(bindingRoutine);
        if (floatingRoutine != null) StopCoroutine(floatingRoutine);
        if (subscribed && MoneyManager.Instance != null)
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyText;
    }
}
