using TMPro;
using UnityEngine;

/// <summary>
/// Displays a notification badge and optionally its current item count.
/// </summary>
public class MenuNotificationBadge : MonoBehaviour
{
    [SerializeField] private GameObject badgeTarget;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private bool initialVisible = true;
    [SerializeField] private int initialCount;

    public bool IsVisible { get; private set; }
    public int Count { get; private set; }

    private void Awake()
    {
        SetVisible(initialVisible);
        SetCount(initialCount);
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;

        if (badgeTarget != null)
            badgeTarget.SetActive(visible);

        if (countText != null)
        {
            countText.text = Count > 0 ? Count.ToString() : "!";
            countText.gameObject.SetActive(visible);
        }
    }

    public void SetCount(int count)
    {
        Count = Mathf.Max(0, count);

        SetVisible(Count > 0);

        if (countText == null) return;

        countText.text = Count > 0 ? Count.ToString() : "!";
        countText.gameObject.SetActive(IsVisible);
    }
}
