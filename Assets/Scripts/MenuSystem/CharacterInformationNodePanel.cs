using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Renders the selected character's ReRe information nodes in MenuRootV2.</summary>
public class CharacterInformationNodePanel : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private Button[] characterRowButtons;
    [SerializeField] private int[] characterRowIndexes;
    [SerializeField] private TMP_Text selectedCharacterText;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private RectTransform nodeListRoot;
    [SerializeField] private GameObject nodeRowPrefab;

    private CharacterInformationNodeState state;
    private CharacterInfo selectedCharacter;
    private readonly List<GameObject> rows = new List<GameObject>();

    private void Awake()
    {
        state = new CharacterInformationNodeState(characterDatabase);
        state.NodeUpdated += HandleNodeUpdated;
        BindRows();
    }

    private void Start()
    {
        if (characterDatabase && characterDatabase.GetAll().Count > 0)
            SelectCharacterByIndex(GetInitialIndex());
    }

    private void OnDestroy()
    {
        if (state != null) state.NodeUpdated -= HandleNodeUpdated;
    }

    public bool SetNodeConfidence(string characterId, string nodeId, CharacterInformationConfidence confidence)
    {
        return state != null && state.TrySetConfidence(characterId, nodeId, confidence);
    }

    public bool SetNodeDisplayContent(string characterId, string nodeId, string displayContent)
    {
        return state != null && state.TrySetDisplayContent(characterId, nodeId, displayContent);
    }

    public bool TryGetNode(string characterId, string nodeId, out CharacterInformationNodeState.NodeView node)
    {
        node = default;
        return state != null && state.TryGetNode(characterId, nodeId, out node);
    }

    public void SelectCharacterByIndex(int index)
    {
        if (!characterDatabase) return;
        var characters = characterDatabase.GetAll();
        if (index < 0 || index >= characters.Count || !characters[index]) return;
        selectedCharacter = characters[index];
        Refresh();
    }

    private void BindRows()
    {
        if (characterRowButtons == null) return;
        for (var i = 0; i < characterRowButtons.Length; i++)
        {
            var button = characterRowButtons[i];
            if (!button) continue;
            var rowIndex = characterRowIndexes != null && i < characterRowIndexes.Length ? characterRowIndexes[i] : i;
            button.onClick.AddListener(() => SelectCharacterByIndex(rowIndex));
        }
    }

    private int GetInitialIndex()
    {
        return characterRowIndexes != null && characterRowIndexes.Length > 0 ? characterRowIndexes[0] : 0;
    }

    private void HandleNodeUpdated(CharacterInformationNodeState.NodeView node)
    {
        if (selectedCharacter && node.CharacterId == CharacterInformationNodeState.GetCharacterId(selectedCharacter)) Refresh();
    }

    private void Refresh()
    {
        ClearRows();
        if (!selectedCharacter || state == null) return;

        if (selectedCharacterText)
            selectedCharacterText.text = string.IsNullOrWhiteSpace(selectedCharacter.displayName)
                ? CharacterInformationNodeState.GetCharacterId(selectedCharacter)
                : selectedCharacter.displayName;

        var nodes = state.GetNodes(CharacterInformationNodeState.GetCharacterId(selectedCharacter));
        if (emptyText) emptyText.gameObject.SetActive(nodes.Count == 0);
        foreach (var node in nodes) CreateRow(node);
    }

    private void CreateRow(CharacterInformationNodeState.NodeView node)
    {
        if (!nodeListRoot || !nodeRowPrefab) return;
        var row = Instantiate(nodeRowPrefab, nodeListRoot);
        row.SetActive(true);
        rows.Add(row);

        var rowRect = row.GetComponent<RectTransform>();
        if (rowRect) rowRect.anchoredPosition = new Vector2(0f, -(rows.Count - 1) * 82f);

        var labels = row.GetComponentsInChildren<TMP_Text>(true);
        if (labels.Length > 0) labels[0].text = node.Title;
        if (labels.Length > 1) labels[1].text = GetCategoryLabel(node.Category) + "  " + GetConfidenceLabel(node.Confidence);
        if (labels.Length > 2) labels[2].text = node.IsHidden ? "?????" : node.Content;

        var image = row.GetComponent<Image>();
        if (image) image.color = GetConfidenceColor(node.Confidence);
    }

    private void ClearRows()
    {
        foreach (var row in rows)
            if (row) Destroy(row);
        rows.Clear();
    }

    public static string GetCategoryLabel(CharacterInformationNodeCategory category)
    {
        switch (category)
        {
            case CharacterInformationNodeCategory.BasicInformation: return "基本情報";
            case CharacterInformationNodeCategory.SelfImage: return "自己像";
            case CharacterInformationNodeCategory.Desire: return "欲望";
            case CharacterInformationNodeCategory.Fear: return "恐れ";
            case CharacterInformationNodeCategory.Resources: return "資源";
            case CharacterInformationNodeCategory.Risk: return "危険性";
            case CharacterInformationNodeCategory.CompanyConnection: return "会社接点";
            default: return category.ToString();
        }
    }

    public static string GetConfidenceLabel(CharacterInformationConfidence confidence)
    {
        switch (confidence)
        {
            case CharacterInformationConfidence.Unknown: return "未知";
            case CharacterInformationConfidence.Speculation: return "推測";
            case CharacterInformationConfidence.Confirmed: return "確認済み";
            case CharacterInformationConfidence.Misinformation: return "誤情報";
            default: return confidence.ToString();
        }
    }

    public static Color GetConfidenceColor(CharacterInformationConfidence confidence)
    {
        switch (confidence)
        {
            case CharacterInformationConfidence.Confirmed: return new Color(0.66f, 0.90f, 0.70f, 1f);
            case CharacterInformationConfidence.Speculation: return new Color(1f, 0.88f, 0.54f, 1f);
            case CharacterInformationConfidence.Misinformation: return new Color(1f, 0.64f, 0.68f, 1f);
            default: return new Color(0.68f, 0.70f, 0.76f, 1f);
        }
    }
}
