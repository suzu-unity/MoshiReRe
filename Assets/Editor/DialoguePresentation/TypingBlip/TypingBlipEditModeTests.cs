using MoshiReRe.DialoguePresentation.TypingBlip;
using NUnit.Framework;

public sealed class TypingBlipEditModeTests
{
    [Test]
    public void StripRichTextTags_RemovesTagsButKeepsText()
    {
        Assert.AreEqual("Hello 世界!", TypingBlipTextUtility.StripRichTextTags("<b>Hello</b> 世界!"));
    }

    [Test]
    public void CountSpeakableCharacters_IgnoresPunctuationAndWhitespace()
    {
        Assert.AreEqual(4, TypingBlipTextUtility.CountSpeakableCharacters("あ、 い!<color=red>U2</color>"));
    }

    [Test]
    public void RevealState_EmitsAfterConfiguredCharacterCount()
    {
        var state = new TypingBlipRevealState();

        Assert.AreEqual(0, state.Consume("A B!C", 1, 2));
        Assert.AreEqual(1, state.Consume("A B!C", 3, 2));
        Assert.AreEqual(0, state.Consume("A B!C", 5, 2));
    }

    [Test]
    public void RevealMath_ClampsProgressToVisibleCharacters()
    {
        Assert.AreEqual(0, TypingBlipRevealMath.GetRevealedCharacterCount(-1f, 8));
        Assert.AreEqual(4, TypingBlipRevealMath.GetRevealedCharacterCount(.5f, 8));
        Assert.AreEqual(8, TypingBlipRevealMath.GetRevealedCharacterCount(2f, 8));
    }

    [Test]
    public void Profile_SelectsAuthorCaseInsensitivelyThenFallback()
    {
        var profile = UnityEngine.ScriptableObject.CreateInstance<TypingBlipProfile>();
        var authorEntry = new TypingBlipProfileEntry("ReRe", null, 1f, 0f, 1f, 0f, 1);
        var fallbackEntry = new TypingBlipProfileEntry(string.Empty, null, 1f, 0f, 1f, 0f, 1);
        profile.ReplaceEntries(new[] { authorEntry, fallbackEntry });

        Assert.AreSame(authorEntry, profile.FindEntry("rere"));
        Assert.AreSame(fallbackEntry, profile.FindEntry("Unknown"));

        UnityEngine.Object.DestroyImmediate(profile);
    }

    [Test]
    public void Eligibility_RejectsSkipInstantAndBacklog()
    {
        Assert.IsFalse(TypingBlipEligibility.CanStart(true, false, false));
        Assert.IsFalse(TypingBlipEligibility.CanStart(false, true, false));
        Assert.IsFalse(TypingBlipEligibility.CanStart(false, false, true));
        Assert.IsTrue(TypingBlipEligibility.CanStart(false, false, false));
    }

    [Test]
    public void RateLimiter_EnforcesMinimumInterval()
    {
        var limiter = new TypingBlipRateLimiter();

        Assert.IsTrue(limiter.TryAcquire(1f, .1f));
        Assert.IsFalse(limiter.TryAcquire(1.05f, .1f));
        Assert.IsTrue(limiter.TryAcquire(1.1f, .1f));
    }
}
