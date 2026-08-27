using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Deterministic, offline-first response selection for ReRe conversations.
/// This class has no Unity scene dependencies and is intentionally easy to
/// exercise from EditMode tests.
/// </summary>
public sealed class ReReConversationEngine
{
    private const float SemanticConfidenceThreshold = 0.05f;

    private readonly List<ReReResponseEntry> entries;
    private readonly IReReSemanticRetrievalProvider semanticProvider;

    public IReadOnlyList<ReReResponseEntry> Entries => entries;

    public ReReConversationEngine(
        IEnumerable<ReReResponseEntry> responseEntries = null,
        IReReSemanticRetrievalProvider semanticProvider = null)
    {
        entries = responseEntries == null
            ? ReReOfflineResponseBank.CreateEntries()
            : responseEntries.Where(entry => entry != null && entry.IsUsable).ToList();

        if (entries.Count == 0)
            entries = ReReOfflineResponseBank.CreateEntries();
        else if (!entries.Any(entry => entry.fallback))
            entries.AddRange(ReReOfflineResponseBank.CreateEntries().Where(entry => entry.fallback));

        this.semanticProvider = semanticProvider;
    }

    public ReReResponseResult Respond(string input, ReReConversationContext context = null, int turnIndex = 0)
    {
        var normalizedInput = NormalizeInput(input);
        if (normalizedInput.Length == 0)
            return ReReResponseResult.Empty;

        context = context ?? ReReConversationContext.Empty;
        var candidates = entries.Where(entry => IsEligible(entry, context)).ToList();
        if (candidates.Count == 0)
            return ReReResponseResult.Empty;

        if (semanticProvider != null && TrySemanticResponse(normalizedInput, candidates, context, turnIndex, out var semanticResult))
            return semanticResult;

        var selected = SelectLocalEntry(normalizedInput, candidates, context);
        if (selected.Entry == null)
            return ReReResponseResult.Empty;

        return CreateResult(selected.Entry, selected.Score, normalizedInput, context, turnIndex, false);
    }

    public static string NormalizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormKC).ToLower(CultureInfo.InvariantCulture);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (char.IsWhiteSpace(character) || char.IsPunctuation(character) || char.IsSymbol(character))
                continue;

            builder.Append(character);
        }

        return builder.ToString();
    }

    private bool TrySemanticResponse(
        string normalizedInput,
        IReadOnlyList<ReReResponseEntry> candidates,
        ReReConversationContext context,
        int turnIndex,
        out ReReResponseResult result)
    {
        result = ReReResponseResult.Empty;
        ReReSemanticMatch match;
        try
        {
            if (!semanticProvider.TryRetrieve(normalizedInput, candidates, out match) ||
                string.IsNullOrWhiteSpace(match.ResponseId) ||
                match.Score < SemanticConfidenceThreshold)
                return false;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[ReReConversation] Semantic retrieval failed; using offline matching. {exception.Message}");
            return false;
        }

        var entry = candidates.FirstOrDefault(candidate =>
            string.Equals(candidate.id, match.ResponseId, StringComparison.OrdinalIgnoreCase));
        if (entry == null)
            return false;

        result = CreateResult(entry, match.Score, normalizedInput, context, turnIndex, true);
        return result.HasResponse;
    }

    private static (ReReResponseEntry Entry, float Score) SelectLocalEntry(
        string normalizedInput,
        IReadOnlyList<ReReResponseEntry> candidates,
        ReReConversationContext context)
    {
        ReReResponseEntry bestEntry = null;
        var bestScore = float.MinValue;
        foreach (var entry in candidates)
        {
            var score = ScoreEntry(entry, normalizedInput, context);
            if (score <= 0f && !entry.fallback)
                continue;

            if (bestEntry == null || score > bestScore ||
                (Mathf.Approximately(score, bestScore) && CompareEntries(entry, bestEntry) < 0))
            {
                bestEntry = entry;
                bestScore = score;
            }
        }

        return (bestEntry, bestEntry == null ? 0f : bestScore);
    }

    private static int CompareEntries(ReReResponseEntry left, ReReResponseEntry right)
    {
        var priority = right.priority.CompareTo(left.priority);
        if (priority != 0)
            return priority;

        return string.Compare(left.id, right.id, StringComparison.OrdinalIgnoreCase);
    }

    private static float ScoreEntry(ReReResponseEntry entry, string normalizedInput, ReReConversationContext context)
    {
        var score = entry.fallback ? 0.5f : 0f;
        if (entry.keywords != null)
        {
            foreach (var keyword in entry.keywords)
            {
                var normalizedKeyword = NormalizeInput(keyword);
                if (normalizedKeyword.Length == 0)
                    continue;

                if (string.Equals(normalizedInput, normalizedKeyword, StringComparison.Ordinal))
                    score += 80f + normalizedKeyword.Length;
                else if (normalizedInput.Contains(normalizedKeyword, StringComparison.Ordinal))
                    score += 30f + normalizedKeyword.Length;
                else if (normalizedKeyword.Contains(normalizedInput, StringComparison.Ordinal) && normalizedInput.Length >= 2)
                    score += 5f + normalizedInput.Length;
            }
        }

        score += ContextBonus(entry.requiredQuestIds, context.ActiveQuestId, 18f);
        score += ContextBonus(entry.requiredContextTags, context.ContextTags, 12f);
        score += ContextBonus(entry.requiredClueIds, context.KnownClues, 10f);
        score += ContextBonus(entry.requiredInventoryIds, context.InventoryItems, 10f);
        score += Mathf.Clamp(entry.priority, -100, 100) * 0.01f;
        return score;
    }

    private static float ContextBonus(string[] requirements, string value, float amount)
    {
        if (requirements == null || requirements.Length == 0 || string.IsNullOrWhiteSpace(value))
            return 0f;

        foreach (var requirement in requirements)
            if (TagMatches(requirement, value))
                return amount;

        return 0f;
    }

    private static float ContextBonus(string[] requirements, IReadOnlyCollection<string> values, float amount)
    {
        if (requirements == null || requirements.Length == 0 || values == null || values.Count == 0)
            return 0f;

        var matches = 0;
        foreach (var requirement in requirements)
        {
            foreach (var value in values)
            {
                if (TagMatches(requirement, value))
                {
                    matches++;
                    break;
                }
            }
        }

        return matches * amount;
    }

    private static bool IsEligible(ReReResponseEntry entry, ReReConversationContext context)
    {
        if (entry == null || !entry.IsUsable)
            return false;

        if (context.StoryProgress < entry.minimumStoryProgress || context.StoryProgress > entry.maximumStoryProgress)
            return false;

        return HasRequired(entry.requiredQuestIds, context.ActiveQuestId) &&
               HasRequired(entry.requiredContextTags, context.ContextTags) &&
               HasRequired(entry.requiredClueIds, context.KnownClues) &&
               HasRequired(entry.requiredInventoryIds, context.InventoryItems);
    }

    private static bool HasRequired(string[] requirements, string value)
    {
        if (requirements == null || requirements.Length == 0)
            return true;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        foreach (var requirement in requirements)
            if (!TagMatches(requirement, value))
                return false;

        return true;
    }

    private static bool HasRequired(string[] requirements, IReadOnlyCollection<string> values)
    {
        if (requirements == null || requirements.Length == 0)
            return true;

        if (values == null || values.Count == 0)
            return false;

        foreach (var requirement in requirements)
        {
            var found = false;
            foreach (var value in values)
            {
                if (TagMatches(requirement, value))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                return false;
        }

        return true;
    }

    private static bool TagMatches(string requirement, string value)
    {
        var normalizedRequirement = NormalizeInput(requirement);
        var normalizedValue = NormalizeInput(value);
        return normalizedRequirement.Length > 0 &&
               (string.Equals(normalizedRequirement, normalizedValue, StringComparison.Ordinal) ||
                normalizedValue.Contains(normalizedRequirement, StringComparison.Ordinal));
    }

    private static ReReResponseResult CreateResult(
        ReReResponseEntry entry,
        float score,
        string normalizedInput,
        ReReConversationContext context,
        int turnIndex,
        bool usedSemanticRetrieval)
    {
        if (entry == null || !entry.HasResponses)
            return ReReResponseResult.Empty;

        var variantIndex = StableVariantIndex(entry, normalizedInput, context, turnIndex);
        var response = entry.responses[Mathf.Clamp(variantIndex, 0, entry.responses.Length - 1)] ?? string.Empty;
        response = FormatResponse(response, context);
        return new ReReResponseResult(true, entry.id, response, entry.expression, score, usedSemanticRetrieval);
    }

    private static int StableVariantIndex(
        ReReResponseEntry entry,
        string normalizedInput,
        ReReConversationContext context,
        int turnIndex)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + StableHash(entry.id);
            hash = hash * 31 + StableHash(normalizedInput);
            hash = hash * 31 + context.StoryProgress;
            hash = hash * 31 + StableHash(context.ActiveQuestId);
            hash = hash * 31 + turnIndex;
            return Mathf.Abs(hash == int.MinValue ? int.MaxValue : hash) % entry.responses.Length;
        }
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            var hash = 23;
            if (value != null)
                foreach (var character in value)
                    hash = hash * 31 + character;
            return hash;
        }
    }

    private static string FormatResponse(string response, ReReConversationContext context)
    {
        if (string.IsNullOrEmpty(response))
            return string.Empty;

        return response
            .Replace("{quest}", string.IsNullOrWhiteSpace(context.ActiveQuestText) ? "いまの目的" : context.ActiveQuestText)
            .Replace("{context}", string.IsNullOrWhiteSpace(context.ActiveContextId) ? "現在の状況" : context.ActiveContextId)
            .Replace("{story}", context.StoryProgress.ToString(CultureInfo.InvariantCulture));
    }
}
