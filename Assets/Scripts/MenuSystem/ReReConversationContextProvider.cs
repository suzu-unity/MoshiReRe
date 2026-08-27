using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inspector-friendly context bridge for the free-text ReRe demo.  Projects
/// with richer quest/inventory systems can replace this component with another
/// implementation of <see cref="IReReConversationContextProvider"/>.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReReConversationContextProvider : MonoBehaviour, IReReConversationContextProvider
{
    [Header("Story")]
    [SerializeField] private bool useMainQuestState = true;
    [SerializeField] private int storyProgress;
    [SerializeField] private string activeQuestId;
    [TextArea]
    [SerializeField] private string activeQuestText;
    [SerializeField] private string activeContextId;

    [Header("Tags (comma, semicolon, or newline separated)")]
    [SerializeField] private string contextTags;
    [SerializeField] private string knownClues;
    [SerializeField] private string inventoryItems;

    public event Action ContextChanged;

    public ReReConversationContext GetConversationContext()
    {
        var questId = activeQuestId;
        var questText = activeQuestText;
        if (useMainQuestState && MainQuestState.Current.IsAssigned)
        {
            if (string.IsNullOrWhiteSpace(questId))
                questId = MainQuestState.Current.Title;
            if (string.IsNullOrWhiteSpace(questText))
                questText = string.IsNullOrWhiteSpace(MainQuestState.Current.Objective)
                    ? MainQuestState.Current.Title
                    : MainQuestState.Current.Title + " / " + MainQuestState.Current.Objective;
        }

        return new ReReConversationContext(
            storyProgress,
            questId,
            questText,
            activeContextId,
            SplitTags(contextTags),
            SplitTags(knownClues),
            SplitTags(inventoryItems));
    }

    public void SetStoryProgress(int value)
    {
        storyProgress = value;
        ContextChanged?.Invoke();
    }

    public void SetActiveQuest(string id, string text = null)
    {
        activeQuestId = id ?? string.Empty;
        activeQuestText = text ?? string.Empty;
        ContextChanged?.Invoke();
    }

    public void SetActiveContext(string id)
    {
        activeContextId = id ?? string.Empty;
        ContextChanged?.Invoke();
    }

    public void SetKnownClues(IEnumerable<string> values)
    {
        knownClues = JoinTags(values);
        ContextChanged?.Invoke();
    }

    public void SetInventoryItems(IEnumerable<string> values)
    {
        inventoryItems = JoinTags(values);
        ContextChanged?.Invoke();
    }

    private static string[] SplitTags(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new string[0];

        return value.Split(new[] { ',', ';', '\n', '\r', '|' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private static string JoinTags(IEnumerable<string> values)
    {
        if (values == null)
            return string.Empty;

        var result = new List<string>();
        foreach (var value in values)
            if (!string.IsNullOrWhiteSpace(value)) result.Add(value.Trim());
        return string.Join(",", result);
    }
}
