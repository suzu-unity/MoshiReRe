using System;
using System.Collections.Generic;

/// <summary>
/// Runtime-only confidence state for CharacterInfo.nodes. It never writes to the
/// ScriptableObject definitions, so scenario updates cannot dirty authored data.
/// </summary>
public sealed class CharacterInformationNodeState
{
    public struct NodeView
    {
        public readonly string CharacterId;
        public readonly string NodeId;
        public readonly string Title;
        public readonly CharacterInformationNodeCategory Category;
        public readonly CharacterInformationConfidence Confidence;
        public readonly string Content;

        public bool IsHidden => Confidence == CharacterInformationConfidence.Unknown;

        internal NodeView(string characterId, CharacterInformationNodeDefinition definition, CharacterInformationConfidence confidence, string displayContent)
        {
            CharacterId = characterId;
            NodeId = definition.id;
            Title = definition.title;
            Category = definition.category;
            Confidence = confidence;
            Content = confidence == CharacterInformationConfidence.Unknown ? string.Empty : displayContent;
        }
    }

    private readonly CharacterDatabase database;
    private readonly Dictionary<string, CharacterInformationConfidence> confidences =
        new Dictionary<string, CharacterInformationConfidence>(StringComparer.Ordinal);
    private readonly Dictionary<string, string> displayContents =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public event Action<NodeView> NodeUpdated;

    public CharacterInformationNodeState(CharacterDatabase characterDatabase)
    {
        database = characterDatabase;
        Reset();
    }

    public void Reset()
    {
        confidences.Clear();
        displayContents.Clear();
        if (!database) return;

        foreach (var character in database.GetAll())
        {
            if (!character) continue;
            var characterId = GetCharacterId(character);
            if (string.IsNullOrWhiteSpace(characterId) || character.nodes == null) continue;

            foreach (var node in character.nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.id)) continue;
                confidences[MakeKey(characterId, node.id)] = node.initialConfidence;
            }
        }
    }

    public bool TryGetNode(string characterId, string nodeId, out NodeView node)
    {
        node = default;
        if (!TryGetDefinition(characterId, nodeId, out var definition)) return false;

        var confidence = confidences.TryGetValue(MakeKey(characterId, nodeId), out var stored)
            ? stored
            : definition.initialConfidence;
        var content = displayContents.TryGetValue(MakeKey(characterId, nodeId), out var storedContent)
            ? storedContent
            : definition.content;
        node = new NodeView(characterId, definition, confidence, content);
        return true;
    }

    public bool TrySetConfidence(string characterId, string nodeId, CharacterInformationConfidence confidence)
    {
        if (!TryGetDefinition(characterId, nodeId, out _)) return false;

        var key = MakeKey(characterId, nodeId);
        if (confidences.TryGetValue(key, out var previous) && previous == confidence) return true;

        confidences[key] = confidence;
        if (TryGetNode(characterId, nodeId, out var view)) NodeUpdated?.Invoke(view);
        return true;
    }

    public bool TrySetDisplayContent(string characterId, string nodeId, string displayContent)
    {
        if (!TryGetDefinition(characterId, nodeId, out _)) return false;

        var key = MakeKey(characterId, nodeId);
        var content = displayContent ?? string.Empty;
        if (displayContents.TryGetValue(key, out var previous) && previous == content) return true;

        displayContents[key] = content;
        if (TryGetNode(characterId, nodeId, out var view)) NodeUpdated?.Invoke(view);
        return true;
    }

    public IReadOnlyList<NodeView> GetNodes(string characterId)
    {
        var result = new List<NodeView>();
        var character = FindCharacter(characterId);
        if (!character || character.nodes == null) return result;

        var resolvedCharacterId = GetCharacterId(character);
        foreach (var definition in character.nodes)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.id)) continue;
            if (TryGetNode(resolvedCharacterId, definition.id, out var view)) result.Add(view);
        }

        return result;
    }

    public static string GetCharacterId(CharacterInfo character)
    {
        if (!character) return string.Empty;
        return string.IsNullOrWhiteSpace(character.id) ? character.name : character.id;
    }

    private bool TryGetDefinition(string characterId, string nodeId, out CharacterInformationNodeDefinition definition)
    {
        definition = null;
        if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(nodeId)) return false;

        var character = FindCharacter(characterId);
        if (!character || character.nodes == null) return false;

        foreach (var candidate in character.nodes)
        {
            if (candidate != null && string.Equals(candidate.id, nodeId, StringComparison.Ordinal))
            {
                definition = candidate;
                return true;
            }
        }

        return false;
    }

    private CharacterInfo FindCharacter(string characterId)
    {
        if (!database || string.IsNullOrWhiteSpace(characterId)) return null;
        foreach (var character in database.GetAll())
        {
            if (character && string.Equals(GetCharacterId(character), characterId, StringComparison.Ordinal)) return character;
        }

        return null;
    }

    private static string MakeKey(string characterId, string nodeId) => characterId + "\u001f" + nodeId;
}
