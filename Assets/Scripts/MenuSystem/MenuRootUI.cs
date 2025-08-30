// Assets/Scripts/MenuSystem/MenuRootUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuRootUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform frame;
    [SerializeField] private Transform gridItemsRoot;
    [SerializeField] private GameObject itemCellPrefab;
    [SerializeField] private AdviceBubble adviceBubble;
    [SerializeField] private Button rerePortraitButton; // ← 立ち絵の Button

    [Header("Advice List")]
    [TextArea(2,4)]
    [SerializeField] private string[] adviceMessages = new[]
    {
        "次は『駅前』に行くといいかも！",
        "夜は危ないよ、気をつけてね。",
        "財布の中身…確認しておく？",
    };
    private int adviceIndex = 0;

    [Header("Dummy")]
    [SerializeField] private Sprite[] dummyIcons;

    void OnEnable()
    {
        PopulateDummyItems();

        // 立ち絵クリックで切替
        if (rerePortraitButton)
        {
            rerePortraitButton.onClick.RemoveListener(NextAdvice);
            rerePortraitButton.onClick.AddListener(NextAdvice);
        }

        // 最初のアドバイスを出しっぱに
        if (adviceBubble && adviceMessages.Length > 0)
            adviceBubble.Show(adviceMessages[adviceIndex], autoHideOverride:false);
    }

    void OnDisable()
    {
        if (adviceBubble) adviceBubble.Hide();
        if (rerePortraitButton) rerePortraitButton.onClick.RemoveListener(NextAdvice);
    }

    public void NextAdvice()
    {
        if (adviceMessages == null || adviceMessages.Length == 0 || adviceBubble == null) return;
        adviceIndex = (adviceIndex + 1) % adviceMessages.Length;
        adviceBubble.UpdateText(adviceMessages[adviceIndex]);
    }

    private void PopulateDummyItems()
    {
        if (!gridItemsRoot || !itemCellPrefab) return;

        for (int i = gridItemsRoot.childCount - 1; i >= 0; i--)
            Destroy(gridItemsRoot.GetChild(i).gameObject);

        int count = Mathf.Max(1, dummyIcons != null ? Mathf.Min(4, dummyIcons.Length) : 0);
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(itemCellPrefab, gridItemsRoot);
            var img = go.GetComponentInChildren<Image>();
            var label = go.GetComponentInChildren<TMP_Text>();
            if (img) img.sprite = dummyIcons[i];
            if (label) label.text = i == 0 ? "鍵" : $"アイテム{i+1}";
        }
    }
}