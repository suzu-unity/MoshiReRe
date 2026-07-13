using System.Collections.Generic;
using MoshiReRe.DialoguePresentation.Beat;
using NUnit.Framework;
using UnityEngine;

public sealed class BeatEditModeTests
{
    [Test]
    public void TypeNormalizationIsTrimmedAndCaseInsensitive()
    {
        Assert.AreEqual("awkward", BeatTypeUtility.Normalize("  AwKwArD "));
        Assert.AreEqual(string.Empty, BeatTypeUtility.Normalize("  "));
    }

    [Test]
    public void LibraryFindsPresetByNormalizedType()
    {
        var library = ScriptableObject.CreateInstance<BeatLibrary>();
        var preset = new BeatPreset("Shock", .5f, BeatTimeMode.Unscaled, null, 1f,
            Color.white, .5f, .1f, false, 0f, 1f, false);
        library.ReplacePresets(new List<BeatPreset> { preset });

        Assert.AreSame(preset, library.Find(" shock "));
        Assert.IsNull(library.Find("missing"));

        Object.DestroyImmediate(library);
    }

    [Test]
    public void TimingPolicyMakesSkipImmediateAndKeepsSelectedClock()
    {
        Assert.AreEqual(0f, BeatTiming.GetEffectiveDuration(.5f, true));
        Assert.AreEqual(.25f, BeatTiming.GetDelta(BeatTimeMode.Unscaled, .25f, .01f));
        Assert.AreEqual(.01f, BeatTiming.GetDelta(BeatTimeMode.Scaled, .25f, .01f));
        Assert.IsFalse(BeatTiming.ShouldWaitForInput(true, true));
        Assert.IsTrue(BeatTiming.ShouldWaitForInput(true, false));
    }

    [Test]
    public void PresetPropertiesClampUnsafeValues()
    {
        var preset = new BeatPreset("safe", -1f, BeatTimeMode.Scaled, null, 2f,
            Color.white, 2f, -1f, false, -1f, -2f, false);

        Assert.AreEqual(0f, preset.Duration);
        Assert.AreEqual(1f, preset.SfxVolume);
        Assert.AreEqual(1f, preset.FlashAlpha);
        Assert.AreEqual(0f, preset.BlackoutAlpha);
        Assert.AreEqual(0f, preset.ShakeAmplitude);
        Assert.AreEqual(0f, preset.ShakeFrequency);
    }
}
