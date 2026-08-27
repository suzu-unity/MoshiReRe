using System;
using System.Collections.Generic;
using Naninovel;
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
    [SerializeField] private InventoryDatabase inventoryDatabase;

    public event Action ContextChanged;

    public ReReConversationContext GetConversationContext()
    {
        SyncRuntimeContext(out var runtimeStoryProgress, out var runtimeContextId,
            out var runtimeTags, out var runtimeClues, out var runtimeItems);

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
            Mathf.Max(storyProgress, runtimeStoryProgress),
            questId,
            questText,
            string.IsNullOrWhiteSpace(runtimeContextId) ? activeContextId : runtimeContextId,
            MergeTags(SplitTags(contextTags), runtimeTags),
            MergeTags(SplitTags(knownClues), runtimeClues),
            MergeTags(SplitTags(inventoryItems), runtimeItems));
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

    private void SyncRuntimeContext(
        out int runtimeStoryProgress,
        out string runtimeContextId,
        out string[] runtimeTags,
        out string[] runtimeClues,
        out string[] runtimeItems)
    {
        runtimeStoryProgress = 0;
        runtimeContextId = string.Empty;
        var tags = new List<string>();
        var clues = new List<string>();
        var items = new List<string>();

        if (Engine.Initialized)
        {
            var variables = Engine.GetService<ICustomVariableManager>();
            if (variables != null)
            {
                runtimeContextId = ReadVariable(variables, "rereContext");
                if (int.TryParse(ReadVariable(variables, "storyProgress"), out var parsedProgress))
                    runtimeStoryProgress = parsedProgress;
                if (ReadBoolean(variables, "papaCafeKeyFound"))
                {
                    clues.Add("confirmed");
                    clues.Add("papa_cafe_key");
                }
            }
        }

        if (runtimeContextId.IndexOf("papa_cafe", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            tags.Add("papa_cafe");
            tags.Add("night_target");
        }
        else if (runtimeContextId.IndexOf("office", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            tags.Add("office");
        }
        if (!string.IsNullOrWhiteSpace(runtimeContextId))
            tags.Add(runtimeContextId);

        if (inventoryDatabase != null)
        {
            foreach (var item in inventoryDatabase.GetAcquired())
            {
                if (item == null) continue;
                if (!string.IsNullOrWhiteSpace(item.id)) items.Add(item.id);
                var displayName = item.GetDisplayName();
                if (!string.IsNullOrWhiteSpace(displayName)) items.Add(displayName);
            }
        }

        runtimeTags = tags.ToArray();
        runtimeClues = clues.ToArray();
        runtimeItems = items.ToArray();
    }

    private static string ReadVariable(ICustomVariableManager variables, string name)
    {
        return variables != null && variables.VariableExists(name)
            ? variables.GetVariableValue(name).ToString()
            : string.Empty;
    }

    private static bool ReadBoolean(ICustomVariableManager variables, string name)
    {
        var raw = ReadVariable(variables, name);
        return (bool.TryParse(raw, out var value) && value)
               || (float.TryParse(raw, out var number) && number > 0f);
    }

    private static string[] MergeTags(IEnumerable<string> first, IEnumerable<string> second)
    {
        var merged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (first != null)
            foreach (var value in first)
                if (!string.IsNullOrWhiteSpace(value)) merged.Add(value.Trim());
        if (second != null)
            foreach (var value in second)
                if (!string.IsNullOrWhiteSpace(value)) merged.Add(value.Trim());
        var result = new string[merged.Count];
        merged.CopyTo(result);
        return result;
    }
}
