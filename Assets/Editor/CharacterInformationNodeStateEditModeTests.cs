using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class CharacterInformationNodeStateEditModeTests
{
    private CharacterDatabase database;
    private CharacterInfo character;

    [SetUp]
    public void SetUp()
    {
        database = ScriptableObject.CreateInstance<CharacterDatabase>();
        character = ScriptableObject.CreateInstance<CharacterInfo>();
        character.id = "yui";
        character.nodes = new List<CharacterInformationNodeDefinition>
        {
            new CharacterInformationNodeDefinition
            {
                id = "desire",
                title = "欲しいもの",
                category = CharacterInformationNodeCategory.Desire,
                content = "安定した生活",
                initialConfidence = CharacterInformationConfidence.Unknown
            }
        };
        database.characters.Add(character);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(character);
        Object.DestroyImmediate(database);
    }

    [Test]
    public void Reset_RestoresDefinitionInitialConfidenceWithoutMutatingDefinition()
    {
        var state = new CharacterInformationNodeState(database);
        state.TrySetConfidence("yui", "desire", CharacterInformationConfidence.Confirmed);
        state.Reset();

        Assert.That(state.TryGetNode("yui", "desire", out var node), Is.True);
        Assert.That(node.Confidence, Is.EqualTo(CharacterInformationConfidence.Unknown));
        Assert.That(character.nodes[0].initialConfidence, Is.EqualTo(CharacterInformationConfidence.Unknown));
    }

    [Test]
    public void Update_ChangesRuntimeStateAndRaisesEvent()
    {
        var state = new CharacterInformationNodeState(database);
        CharacterInformationNodeState.NodeView updated = default;
        var eventCount = 0;
        state.NodeUpdated += node => { updated = node; eventCount++; };

        Assert.That(state.TrySetConfidence("yui", "desire", CharacterInformationConfidence.Speculation), Is.True);
        Assert.That(eventCount, Is.EqualTo(1));
        Assert.That(updated.Confidence, Is.EqualTo(CharacterInformationConfidence.Speculation));
        Assert.That(state.TryGetNode("yui", "desire", out var node), Is.True);
        Assert.That(node.Confidence, Is.EqualTo(CharacterInformationConfidence.Speculation));

        Assert.That(state.TrySetDisplayContent("yui", "desire", "自由"), Is.True);
        Assert.That(eventCount, Is.EqualTo(2));
        Assert.That(state.TryGetNode("yui", "desire", out node), Is.True);
        Assert.That(node.Content, Is.EqualTo("自由"));
        Assert.That(character.nodes[0].content, Is.EqualTo("安定した生活"));
    }

    [Test]
    public void UnknownNode_HidesContentUntilConfidenceChanges()
    {
        var state = new CharacterInformationNodeState(database);

        Assert.That(state.TryGetNode("yui", "desire", out var unknown), Is.True);
        Assert.That(unknown.IsHidden, Is.True);
        Assert.That(unknown.Content, Is.Empty);

        state.TrySetConfidence("yui", "desire", CharacterInformationConfidence.Confirmed);
        Assert.That(state.TryGetNode("yui", "desire", out var confirmed), Is.True);
        Assert.That(confirmed.IsHidden, Is.False);
        Assert.That(confirmed.Content, Is.EqualTo("安定した生活"));
    }
}
