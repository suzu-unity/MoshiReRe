using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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

    [Test]
    public void MenuRootV2CharacterRows_AssignOnlyMatchingCategoriesWithoutDuplicates()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NaninovelData/Resources/UI/MenuRootV2.prefab");
        Assert.That(prefab, Is.Not.Null);
        var panel = prefab.GetComponentInChildren<CharacterInformationNodePanel>(true);
        Assert.That(panel, Is.Not.Null);

        const System.Reflection.BindingFlags Flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var rowButtons = (Button[])typeof(CharacterInformationNodePanel).GetField("characterRowButtons", Flags).GetValue(panel);
        var rowIndexes = (int[])typeof(CharacterInformationNodePanel).GetField("characterRowIndexes", Flags).GetValue(panel);
        var menuDatabase = (CharacterDatabase)typeof(CharacterInformationNodePanel).GetField("characterDatabase", Flags).GetValue(panel);
        Assert.That(rowButtons, Has.Length.EqualTo(12));
        Assert.That(rowIndexes, Has.Length.EqualTo(rowButtons.Length));
        Assert.That(menuDatabase, Is.Not.Null);

        var assigned = new HashSet<CharacterInfo>();
        for (var i = 0; i < rowButtons.Length; i++)
        {
            var expectedCategory = i < 6 ? CharacterCategory.Oj : CharacterCategory.Itadaki;
            if (rowIndexes[i] < 0)
            {
                Assert.That(rowButtons[i].interactable, Is.False, "Empty character rows must be disabled.");
                continue;
            }

            var character = menuDatabase.GetAll()[rowIndexes[i]];
            Assert.That(character.category, Is.EqualTo(expectedCategory));
            Assert.That(assigned.Add(character), Is.True, "A character must not be assigned to multiple fixed rows.");
            Assert.That(rowButtons[i].interactable, Is.True);

            var labels = rowButtons[i].GetComponentsInChildren<TMP_Text>(true);
            Assert.That(labels, Is.Not.Empty);
            var expectedName = CharacterInformationNodePanel.GetDisplayName(character);
            Assert.That(labels[0].text, Is.EqualTo(expectedName));
        }
    }
}
