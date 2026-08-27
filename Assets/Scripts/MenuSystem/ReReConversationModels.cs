using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The small set of expressions understood by the ReRe conversation view.
/// A view is free to map these values to sprites, animators, or other visuals.
/// </summary>
public enum ReReExpression
{
    Neutral,
    Listening,
    Thinking,
    Encouraging,
    Concerned,
    Delighted,
    Surprised
}

/// <summary>Lifecycle state for one free-text conversation turn.</summary>
public enum ReReConversationState
{
    Idle,
    Listening,
    Responding,
    Error
}

/// <summary>
/// Runtime facts used to choose a local ReRe response.  The collections are
/// deliberately strings so the conversation system does not depend on a
/// particular quest, clue, or inventory implementation.
/// </summary>
public sealed class ReReConversationContext
{
    public static readonly ReReConversationContext Empty = new ReReConversationContext();

    public int StoryProgress { get; }
    public string ActiveQuestId { get; }
    public string ActiveQuestText { get; }
    public string ActiveContextId { get; }
    public IReadOnlyCollection<string> ContextTags { get; }
    public IReadOnlyCollection<string> KnownClues { get; }
    public IReadOnlyCollection<string> InventoryItems { get; }

    public ReReConversationContext(
        int storyProgress = 0,
        string activeQuestId = null,
        string activeQuestText = null,
        string activeContextId = null,
        IEnumerable<string> contextTags = null,
        IEnumerable<string> knownClues = null,
        IEnumerable<string> inventoryItems = null)
    {
        StoryProgress = storyProgress;
        ActiveQuestId = activeQuestId ?? string.Empty;
        ActiveQuestText = activeQuestText ?? string.Empty;
        ActiveContextId = activeContextId ?? string.Empty;
        ContextTags = Copy(contextTags);
        KnownClues = Copy(knownClues);
        InventoryItems = Copy(inventoryItems);
    }

    private static IReadOnlyCollection<string> Copy(IEnumerable<string> values)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (values == null)
            return result;

        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                result.Add(value.Trim());
        }

        return result;
    }
}

/// <summary>
/// A response authored by designers.  Responses are retrieved by id and then
/// selected from this bank; semantic providers never generate dialogue text.
/// </summary>
[Serializable]
public sealed class ReReResponseEntry
{
    public string id;

    [TextArea(2, 5)]
    public string[] responses = new string[0];

    [Tooltip("Japanese words or phrases. Matching is punctuation/space tolerant.")]
    public string[] keywords = new string[0];

    [Header("Story gates")]
    public int minimumStoryProgress;
    public int maximumStoryProgress = int.MaxValue;
    public string[] requiredQuestIds = new string[0];
    public string[] requiredContextTags = new string[0];
    public string[] requiredClueIds = new string[0];
    public string[] requiredInventoryIds = new string[0];

    [Header("Selection")]
    public int priority;
    public bool fallback;
    public ReReExpression expression = ReReExpression.Neutral;

    public bool IsUsable => !string.IsNullOrWhiteSpace(id) && HasResponses;
    public bool HasResponses => responses != null && responses.Length > 0;
}

/// <summary>Result of one deterministic engine selection.</summary>
public readonly struct ReReResponseResult
{
    public readonly bool HasResponse;
    public readonly string ResponseId;
    public readonly string Text;
    public readonly ReReExpression Expression;
    public readonly float Score;
    public readonly bool UsedSemanticRetrieval;

    public ReReResponseResult(
        bool hasResponse,
        string responseId,
        string text,
        ReReExpression expression,
        float score,
        bool usedSemanticRetrieval)
    {
        HasResponse = hasResponse;
        ResponseId = responseId ?? string.Empty;
        Text = text ?? string.Empty;
        Expression = expression;
        Score = score;
        UsedSemanticRetrieval = usedSemanticRetrieval;
    }

    public static ReReResponseResult Empty => new ReReResponseResult(false, string.Empty, string.Empty,
        ReReExpression.Neutral, 0f, false);
}

/// <summary>
/// Optional bridge for game systems that can expose current narrative facts.
/// Implementations may be MonoBehaviours on the same object, parent, or child.
/// </summary>
public interface IReReConversationContextProvider
{
    ReReConversationContext GetConversationContext();
}

/// <summary>
/// Optional semantic retrieval bridge.  Implementations can call an external
/// embedding service and return an id from the supplied local response bank.
/// The provider must not return generated dialogue; the engine always renders
/// text authored in <see cref="ReReResponseEntry.responses"/>.  A missing or
/// failing provider is safe because local keyword retrieval remains available.
/// </summary>
public interface IReReSemanticRetrievalProvider
{
    bool TryRetrieve(
        string normalizedInput,
        IReadOnlyList<ReReResponseEntry> candidates,
        out ReReSemanticMatch match);
}

public readonly struct ReReSemanticMatch
{
    public readonly string ResponseId;
    public readonly float Score;

    public ReReSemanticMatch(string responseId, float score)
    {
        ResponseId = responseId ?? string.Empty;
        Score = score;
    }
}

/// <summary>UnityEvent payload for expression hooks.</summary>
[Serializable]
public sealed class ReReExpressionEvent : UnityEngine.Events.UnityEvent<ReReExpression>
{
}

/// <summary>UnityEvent payload for state hooks.</summary>
[Serializable]
public sealed class ReReConversationStateEvent : UnityEngine.Events.UnityEvent<ReReConversationState>
{
}
