using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class MainQuestPopup : MonoBehaviour
{
    private const string ResourcePath = "UI/MainQuestPopup";
    private static MainQuestPopup instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text deadlineText;
    [SerializeField] private Vector2 shownPosition = new Vector2(30f, -30f);
    [SerializeField] private Vector2 hiddenPosition = new Vector2(-650f, -30f);
    [SerializeField, Min(0.05f)] private float slideSeconds = 0.22f;
    [SerializeField, Min(0f)] private float holdSeconds = 3f;

    private float hideAt;
    private bool isShowing;

    public static MainQuestPopup EnsureInstance()
    {
        if (instance) return instance;

        var prefab = Resources.Load<MainQuestPopup>(ResourcePath);
        if (!prefab)
        {
            Debug.LogError($"[MainQuestPopup] Resources prefab was not found: {ResourcePath}");
            return null;
        }

        return Instantiate(prefab);
    }

    private void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        if (!popupRect) popupRect = transform as RectTransform;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        SetVisible(false, true);
    }

    private void OnEnable() => MainQuestState.OnChanged += OnQuestChanged;

    private void OnDisable() => MainQuestState.OnChanged -= OnQuestChanged;

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Update()
    {
        if (!popupRect) return;

        var target = isShowing ? shownPosition : hiddenPosition;
        popupRect.anchoredPosition = Vector2.MoveTowards(popupRect.anchoredPosition, target,
            Vector2.Distance(shownPosition, hiddenPosition) / slideSeconds * Time.unscaledDeltaTime);

        if (isShowing && Time.unscaledTime >= hideAt)
            SetVisible(false, false);
    }

    public void Show(MainQuestState.Data quest)
    {
        if (!quest.IsAssigned) return;

        if (titleText) titleText.text = quest.Title;
        if (objectiveText) objectiveText.text = quest.Objective;
        if (deadlineText) deadlineText.text = MainQuestState.FormatDeadline(quest.DeadlineDays);
        SetVisible(true, false);
        hideAt = Time.unscaledTime + holdSeconds;
    }

    private void OnQuestChanged(MainQuestState.Data quest) => Show(quest);

    private void SetVisible(bool visible, bool immediately)
    {
        isShowing = visible;
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (immediately && popupRect)
            popupRect.anchoredPosition = visible ? shownPosition : hiddenPosition;
    }
}
