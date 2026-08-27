using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ReReConversationEngineTests
{
    [Test]
    public void NormalizeInput_RemovesJapaneseWhitespaceAndPunctuation()
    {
        Assert.That(ReReConversationEngine.NormalizeInput("  返済？\n"), Is.EqualTo("返済"));
    }

    [Test]
    public void ResponseSelection_ChangesWithStoryProgress()
    {
        var engine = new ReReConversationEngine();

        var opening = engine.Respond("返済", new ReReConversationContext(storyProgress: 0), 0);
        var later = engine.Respond("返済", new ReReConversationContext(storyProgress: 2), 0);

        Assert.That(opening.HasResponse, Is.True);
        Assert.That(later.HasResponse, Is.True);
        Assert.That(opening.ResponseId, Is.Not.EqualTo(later.ResponseId));
        Assert.That(later.ResponseId, Is.EqualTo("debt_later"));
    }

    [Test]
    public void ResponseSelection_UsesQuestAndContextWhenAvailable()
    {
        var engine = new ReReConversationEngine();

        var chapter = engine.Respond("事件の資料", new ReReConversationContext(
            activeQuestId: "chapter_1",
            activeQuestText: "企画の名義を書き戻す"), 0);
        var office = engine.Respond("会社の証拠", new ReReConversationContext(
            activeContextId: "オフィス街",
            contextTags: new[] { "office" }), 0);

        Assert.That(chapter.ResponseId, Is.EqualTo("chapter_one_case"));
        Assert.That(office.ResponseId, Is.EqualTo("office_context"));
        Assert.That(chapter.Text, Does.Contain("手掛かり").Or.Contain("元担当者"));
    }

    [Test]
    public void ResponseSelection_UsesKnownClueAndInventoryGates()
    {
        var engine = new ReReConversationEngine();

        var clue = engine.Respond("情報を確認した", new ReReConversationContext(
            knownClues: new[] { "confirmed" }), 0);
        var item = engine.Respond("アイテムを使う", new ReReConversationContext(
            inventoryItems: new[] { "社員証" }), 0);

        Assert.That(clue.ResponseId, Is.EqualTo("clue_confirmed"));
        Assert.That(item.ResponseId, Is.EqualTo("inventory_clue"));
    }

    [Test]
    public void ResponseSelection_IsDeterministicForSameTurn()
    {
        var engine = new ReReConversationEngine();
        var context = new ReReConversationContext(storyProgress: 1, activeQuestId: "chapter_1");

        var first = engine.Respond("助けて", context, 3);
        var second = engine.Respond("助けて", context, 3);

        Assert.That(second.ResponseId, Is.EqualTo(first.ResponseId));
        Assert.That(second.Text, Is.EqualTo(first.Text));
        Assert.That(second.Expression, Is.EqualTo(first.Expression));
    }

    [Test]
    public void SemanticProvider_OnlySelectsAnAuthoredResponseId()
    {
        var provider = new StubSemanticProvider("thanks", 0.91f);
        var engine = new ReReConversationEngine(null, provider);

        var result = engine.Respond("まったく別の言い方", ReReConversationContext.Empty, 0);

        Assert.That(result.HasResponse, Is.True);
        Assert.That(result.ResponseId, Is.EqualTo("thanks"));
        Assert.That(result.UsedSemanticRetrieval, Is.True);
        Assert.That(result.Text, Is.Not.Empty);
        Assert.That(result.Text, Does.Not.Contain("まったく別の言い方"));
    }

    [Test]
    public void PapaCafeContext_PrioritizesAuthoredBriefingAndHeldKeyAdvice()
    {
        var engine = new ReReConversationEngine();
        var briefing = engine.Respond("カフェでは何を見ればいい？", new ReReConversationContext(
            activeContextId: "papa_cafe_briefing",
            contextTags: new[] { "papa_cafe", "papa_cafe_briefing" }), 0);
        var heldKey = engine.Respond("この鍵を見せる？", new ReReConversationContext(
            activeContextId: "papa_cafe_negotiation",
            contextTags: new[] { "papa_cafe" },
            inventoryItems: new[] { "old_key" }), 0);

        Assert.That(briefing.ResponseId, Is.EqualTo("papa_cafe_briefing"));
        Assert.That(heldKey.ResponseId, Is.EqualTo("papa_cafe_key"));
    }

    [Test]
    public void ConversationUi_SubmissionPublishesResponseAndLifecycleStates()
    {
        var gameObject = new GameObject("ReReConversationTest");
        var conversation = gameObject.AddComponent<ReReConversationUI>();
        var states = new List<ReReConversationState>();
        var responses = 0;
        conversation.StateChanged += states.Add;
        conversation.ResponseReceived += _ => responses++;

        conversation.SubmitInput("返済");

        Assert.That(responses, Is.EqualTo(1));
        Assert.That(states, Does.Contain(ReReConversationState.Listening));
        Assert.That(states, Does.Contain(ReReConversationState.Responding));
        Assert.That(conversation.CurrentState, Is.EqualTo(ReReConversationState.Idle));
        Assert.That(conversation.LastResponseText, Is.Not.Empty);
        Object.DestroyImmediate(gameObject);
    }

    private sealed class StubSemanticProvider : IReReSemanticRetrievalProvider
    {
        private readonly string responseId;
        private readonly float score;

        public StubSemanticProvider(string responseId, float score)
        {
            this.responseId = responseId;
            this.score = score;
        }

        public bool TryRetrieve(string normalizedInput, IReadOnlyList<ReReResponseEntry> candidates, out ReReSemanticMatch match)
        {
            match = new ReReSemanticMatch(responseId, score);
            return true;
        }
    }
}
