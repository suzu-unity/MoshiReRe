using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps the portrait home HUD readable while allowing live game state and
/// individual notification badges to be updated without rebuilding the prefab.
/// </summary>
public sealed class MenuTopHudState : MonoBehaviour
{
    public enum HudAction
    {
        Dress,
        Characters,
        Home,
        Items,
        Save,
        Map,
        Quest,
        Settings
    }

    [System.Serializable]
    private struct BadgeBinding
    {
        public HudAction action;
        public MenuNotificationBadge badge;
    }

    [Header("Display")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI debtDaysText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GameObject debtUrgencyMark;
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private Image staminaFill;
    [SerializeField] private int initialStamina = 85;
    [SerializeField] private int maximumStamina = 100;

    [Header("Fallback Values")]
    [SerializeField, Min(0)] private int initialDay = 3;
    [SerializeField, Min(0)] private int initialDebtDays = 7;
    [SerializeField] private int initialMoney = 145000;
    [SerializeField, Min(0)] private int urgentDebtDays = 7;

    [Header("Badge API")]
    [SerializeField] private BadgeBinding[] badgeBindings;

    private readonly Dictionary<HudAction, MenuNotificationBadge> badges = new Dictionary<HudAction, MenuNotificationBadge>();
    private bool subscribedToQuest;
    public int CurrentDay => initialDay;

    private void Awake()
    {
        CacheBadges();
        SetDay(initialDay);
        SetDebtDays(initialDebtDays);
        SetMoney(initialMoney);
        SetStamina(initialStamina, maximumStamina);
    }

    private void OnEnable()
    {
        SubscribeQuestState();
    }

    private void OnDisable()
    {
        UnsubscribeQuestState();
    }

    /// <summary>Updates the day label. A day service can call this directly when introduced.</summary>
    public void SetDay(int day)
    {
        initialDay = Mathf.Max(0, day);
        if (dayText)
            dayText.text = $"DAY\n{initialDay:00}";
    }

    /// <summary>Updates the remaining repayment days and its small urgency indicator.</summary>
    public void SetDebtDays(int days)
    {
        initialDebtDays = Mathf.Max(0, days);
        if (debtDaysText)
            debtDaysText.text = $"返済期限\nあと{initialDebtDays}日";
        if (debtUrgencyMark)
            debtUrgencyMark.SetActive(initialDebtDays <= urgentDebtDays);
    }

    /// <summary>Updates the money label. Money systems can call this without taking an assembly dependency.</summary>
    public void SetMoney(int money)
    {
        initialMoney = money;
        if (moneyText)
            moneyText.text = $"¥ {FormatMoney(money)}";
    }

    public void SetStamina(int current, int maximum)
    {
        maximumStamina = Mathf.Max(1, maximum);
        initialStamina = Mathf.Clamp(current, 0, maximumStamina);
        if (staminaText) staminaText.text = $"{initialStamina}/{maximumStamina}";
        if (staminaFill) staminaFill.fillAmount = (float)initialStamina / maximumStamina;
    }

    /// <summary>Sets a notification count for one independently addressable HUD action.</summary>
    public void SetBadgeCount(HudAction action, int count)
    {
        if (TryGetBadge(action, out var badge))
            badge.SetCount(count);
    }

    /// <summary>Shows or hides one independently addressable HUD action badge.</summary>
    public void SetBadgeVisible(HudAction action, bool visible)
    {
        if (TryGetBadge(action, out var badge))
            badge.SetVisible(visible);
    }

    public int GetBadgeCount(HudAction action)
    {
        return TryGetBadge(action, out var badge) ? badge.Count : 0;
    }

    private void SubscribeQuestState()
    {
        if (subscribedToQuest)
            return;

        MainQuestState.OnChanged += HandleQuestChanged;
        subscribedToQuest = true;
        if (MainQuestState.Current.IsAssigned)
            SetDebtDays(MainQuestState.Current.DeadlineDays);
    }

    private void UnsubscribeQuestState()
    {
        if (!subscribedToQuest)
            return;

        MainQuestState.OnChanged -= HandleQuestChanged;
        subscribedToQuest = false;
    }

    private void HandleQuestChanged(MainQuestState.Data data)
    {
        if (data.IsAssigned)
            SetDebtDays(data.DeadlineDays);
    }

    private void CacheBadges()
    {
        badges.Clear();
        if (badgeBindings == null)
            return;

        foreach (var binding in badgeBindings)
        {
            if (binding.badge)
                badges[binding.action] = binding.badge;
        }
    }

    private bool TryGetBadge(HudAction action, out MenuNotificationBadge badge)
    {
        if (badges.Count == 0)
            CacheBadges();
        return badges.TryGetValue(action, out badge) && badge;
    }

    private static string FormatMoney(int amount)
    {
        var absolute = amount < 0 ? -(long)amount : amount;
        var formatted = absolute.ToString("N0");
        return amount < 0 ? "-" + formatted : formatted;
    }
}
