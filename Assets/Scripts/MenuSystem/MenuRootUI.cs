using Naninovel.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuRootUI : CustomUI
{
    [Header("Pages")]
    [SerializeField] private GameObject pageTop;
    [SerializeField] private InventoryPage pageItems;
    [SerializeField] private CharacterPage pageCharacters;
    [SerializeField] private StatusPage pageStatus;
    [SerializeField] private MapPage pageMap;

    [Header("Tab Buttons")]
    [SerializeField] private Button statusTabButton;
    [SerializeField] private Button itemsTabButton;
    [SerializeField] private Button charactersTabButton;
    [SerializeField] private Button mapTabButton;

    [Header("Status Visuals")]
    [SerializeField] private Sprite protagonistPortrait;

    [Header("Common UI")]
    [SerializeField] private AdviceClickTrigger sharedAdviceTrigger;

    [Header("Advice (demo messages)")]
    [TextArea] [SerializeField] private string[] adviceMessages;
    [SerializeField] private bool firstAdviceSticky = true;

    protected override void Awake()
    {
        base.Awake();

        ApplyPortraitLayout();
        AutoWirePages();
        AutoBuildTopTabs();

        if (pageItems) pageItems.SetAdviceTrigger(sharedAdviceTrigger);
        if (pageCharacters) pageCharacters.SetAdviceTrigger(sharedAdviceTrigger);
        if (pageStatus) pageStatus.SetPortrait(protagonistPortrait);

        BindTabButtons();
        ShowPageStatus();
        Hide();
    }

    protected override void OnEnable()
    {
        ShowPageStatus();
        base.OnEnable();

        if (sharedAdviceTrigger && adviceMessages != null && adviceMessages.Length > 0)
        {
            bool autoHide = !firstAdviceSticky;
            sharedAdviceTrigger.ShowAdvice(adviceMessages[0], autoHide);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (sharedAdviceTrigger) sharedAdviceTrigger.HideAdvice();
        UnbindTabButtons();
    }

    public override void Show()
    {
        base.Show();
        ShowPageStatus();
    }

    public void OpenMenu()
    {
        ShowPageStatus();
    }

    public void ShowPageTop()
    {
        if (pageTop) pageTop.SetActive(true);
        if (pageItems) pageItems.Hide();
        if (pageCharacters) pageCharacters.Hide();
        if (pageStatus) pageStatus.Hide();
        if (pageMap) pageMap.Hide();
    }

    public void ShowPageStatus()
    {
        if (pageTop) pageTop.SetActive(false);
        if (pageItems) pageItems.Hide();
        if (pageCharacters) pageCharacters.Hide();
        if (pageMap) pageMap.Hide();
        if (pageStatus) pageStatus.Show();
    }

    public void ShowPageItems()
    {
        if (pageTop) pageTop.SetActive(false);
        if (pageStatus) pageStatus.Hide();
        if (pageCharacters) pageCharacters.Hide();
        if (pageMap) pageMap.Hide();
        if (pageItems) pageItems.Show();
    }

    public void ShowPageCharacters()
    {
        if (pageTop) pageTop.SetActive(false);
        if (pageItems) pageItems.Hide();
        if (pageStatus) pageStatus.Hide();
        if (pageMap) pageMap.Hide();
        if (pageCharacters) pageCharacters.Show();
    }

    public void ShowPageMap()
    {
        if (pageTop) pageTop.SetActive(false);
        if (pageItems) pageItems.Hide();
        if (pageCharacters) pageCharacters.Hide();
        if (pageStatus) pageStatus.Hide();
        if (pageMap) pageMap.Show();
    }

    public void OnOjToggleChanged(bool isOn)
    {
        if (!isOn) return;
        if (pageCharacters) pageCharacters.Show();
    }

    public void OnItadakiToggleChanged(bool isOn)
    {
        if (!isOn) return;
        if (pageCharacters) pageCharacters.Show();
    }

    private void ApplyPortraitLayout()
    {
        var root = transform as RectTransform;
        if (!root) return;

        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(900f, 1600f);
        root.anchoredPosition = Vector2.zero;
    }

    private void AutoWirePages()
    {
        if (!pageItems) pageItems = GetComponentInChildren<InventoryPage>(true);
        if (!pageCharacters) pageCharacters = GetComponentInChildren<CharacterPage>(true);
        if (!pageStatus) pageStatus = GetComponentInChildren<StatusPage>(true);
        if (!pageMap) pageMap = GetComponentInChildren<MapPage>(true);

        if (!pageStatus)
        {
            var statusGo = CreateBasicPage("PageStatus", new Vector2(0f, -120f));
            pageStatus = statusGo.AddComponent<StatusPage>();
            BuildStatusPage(statusGo.transform as RectTransform, pageStatus);
        }

        if (!pageMap)
        {
            var mapGo = CreateBasicPage("PageMap", new Vector2(0f, -120f));
            pageMap = mapGo.AddComponent<MapPage>();
            BuildMapPage(mapGo.transform as RectTransform);
        }

        if (!pageTop)
            pageTop = transform.Find("PageTop")?.gameObject;
    }

    private void AutoBuildTopTabs()
    {
        if (statusTabButton && itemsTabButton && charactersTabButton && mapTabButton)
            return;

        var tabRoot = transform.Find("TopTabs") as RectTransform;
        if (!tabRoot)
        {
            var go = new GameObject("TopTabs", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(transform, false);
            tabRoot = go.GetComponent<RectTransform>();

            tabRoot.anchorMin = new Vector2(0.5f, 1f);
            tabRoot.anchorMax = new Vector2(0.5f, 1f);
            tabRoot.pivot = new Vector2(0.5f, 1f);
            tabRoot.anchoredPosition = new Vector2(0f, -24f);
            tabRoot.sizeDelta = new Vector2(820f, 96f);

            var h = go.GetComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleCenter;
            h.spacing = 12f;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = true;
        }

        statusTabButton = statusTabButton ? statusTabButton : CreateTabButton(tabRoot, "Status", "TabStatus");
        itemsTabButton = itemsTabButton ? itemsTabButton : CreateTabButton(tabRoot, "Items", "TabItems");
        charactersTabButton = charactersTabButton ? charactersTabButton : CreateTabButton(tabRoot, "Characters", "TabCharacters");
        mapTabButton = mapTabButton ? mapTabButton : CreateTabButton(tabRoot, "Map", "TabMap");
    }

    private Button CreateTabButton(RectTransform parent, string label, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        button.colors = colors;

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.transform as RectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 30;
        tmp.color = Color.white;

        return button;
    }

    private GameObject CreateBasicPage(string name, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var rt = go.transform as RectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(860f, 1440f);
        return go;
    }

    private void BuildStatusPage(RectTransform parent, StatusPage statusPage)
    {
        var portraitGo = new GameObject("ProtagonistPortrait", typeof(RectTransform), typeof(Image));
        portraitGo.transform.SetParent(parent, false);
        var portraitRt = portraitGo.transform as RectTransform;
        portraitRt.anchorMin = new Vector2(0f, 1f);
        portraitRt.anchorMax = new Vector2(0f, 1f);
        portraitRt.pivot = new Vector2(0f, 1f);
        portraitRt.anchoredPosition = new Vector2(24f, -24f);
        portraitRt.sizeDelta = new Vector2(280f, 430f);
        var portrait = portraitGo.GetComponent<Image>();

        var radarGo = new GameObject("StatusRadar", typeof(RectTransform), typeof(RadarChart));
        radarGo.transform.SetParent(parent, false);
        var radarRt = radarGo.transform as RectTransform;
        radarRt.anchorMin = new Vector2(1f, 1f);
        radarRt.anchorMax = new Vector2(1f, 1f);
        radarRt.pivot = new Vector2(1f, 1f);
        radarRt.anchoredPosition = new Vector2(-40f, -40f);
        radarRt.sizeDelta = new Vector2(420f, 420f);
        var radar = radarGo.GetComponent<RadarChart>();

        TMP_Text CreateStatText(string n, Vector2 p)
        {
            var tgo = new GameObject(n, typeof(RectTransform), typeof(TextMeshProUGUI));
            tgo.transform.SetParent(parent, false);
            var trt = tgo.transform as RectTransform;
            trt.anchorMin = new Vector2(0f, 1f);
            trt.anchorMax = new Vector2(0f, 1f);
            trt.pivot = new Vector2(0f, 1f);
            trt.anchoredPosition = p;
            trt.sizeDelta = new Vector2(420f, 52f);
            var t = tgo.GetComponent<TextMeshProUGUI>();
            t.fontSize = 32f;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Left;
            return t;
        }

        var guts = CreateStatText("GutsText", new Vector2(24f, -500f));
        var intel = CreateStatText("IntelligenceText", new Vector2(24f, -560f));
        var attention = CreateStatText("AttentionText", new Vector2(24f, -620f));
        var tech = CreateStatText("TechniqueText", new Vector2(24f, -680f));
        var strength = CreateStatText("StrengthText", new Vector2(24f, -740f));

        statusPage.Configure(radar, portrait, guts, intel, attention, tech, strength);
        statusPage.SetPortrait(protagonistPortrait);
    }

    private void BuildMapPage(RectTransform parent)
    {
        var labelGo = new GameObject("MapPlaceholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(parent, false);
        var rt = labelGo.transform as RectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(700f, 120f);

        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "MAP PAGE";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 56;
        tmp.color = Color.white;
    }

    private void BindTabButtons()
    {
        UnbindTabButtons();

        if (statusTabButton) statusTabButton.onClick.AddListener(ShowPageStatus);
        if (itemsTabButton) itemsTabButton.onClick.AddListener(ShowPageItems);
        if (charactersTabButton) charactersTabButton.onClick.AddListener(ShowPageCharacters);
        if (mapTabButton) mapTabButton.onClick.AddListener(ShowPageMap);
    }

    private void UnbindTabButtons()
    {
        if (statusTabButton) statusTabButton.onClick.RemoveListener(ShowPageStatus);
        if (itemsTabButton) itemsTabButton.onClick.RemoveListener(ShowPageItems);
        if (charactersTabButton) charactersTabButton.onClick.RemoveListener(ShowPageCharacters);
        if (mapTabButton) mapTabButton.onClick.RemoveListener(ShowPageMap);
    }
}
