using MoshiReRe.DialoguePresentation.CharacterMotion;
using Naninovel;
using NUnit.Framework;
using UnityEngine;

public sealed class CharacterMotionEditModeTests
{
    [Test]
    public void SequenceRepeatsAndReturnsToOriginalPose()
    {
        var preset = new CharacterMotionPreset("nervous", new Vector3(.2f, 0f, 0f),
            new Vector3(1.1f, 1.1f, 1f), Vector3.zero, .4f, EasingType.Linear, true, 3, true, 0);
        var origin = new MotionPose(Vector3.zero, Vector3.one, Quaternion.identity);

        var sequence = CharacterMotionMath.BuildSequence(origin, preset);

        Assert.AreEqual(6, sequence.Count);
        Assert.That(sequence[0].Position.x, Is.EqualTo(.2f).Within(.0001f));
        Assert.That(sequence[sequence.Count - 1].Position, Is.EqualTo(origin.Position));
        Assert.That(sequence[sequence.Count - 1].Scale, Is.EqualTo(origin.Scale));
    }

    [Test]
    public void SnapProducesDiscreteIntermediateStages()
    {
        var from = new MotionPose(Vector3.zero, Vector3.one, Quaternion.identity);
        var to = new MotionPose(new Vector3(1f, 0f, 0f), new Vector3(2f, 2f, 1f), Quaternion.Euler(0f, 0f, 90f));

        var middle = CharacterMotionMath.Snap(from, to, 4, 2);

        Assert.That(middle.Position.x, Is.EqualTo(.5f).Within(.0001f));
        Assert.That(middle.Scale.x, Is.EqualTo(1.5f).Within(.0001f));
        Assert.That(middle.Rotation.eulerAngles.z, Is.EqualTo(45f).Within(.001f));
    }

    [Test]
    public void NonLoopingMotionCanLeaveTargetWhenReturnIsDisabled()
    {
        var preset = new CharacterMotionPreset("hold", new Vector3(.15f, 0f, 0f), Vector3.one,
            Vector3.zero, .2f, EasingType.Linear, false, 8, false, 0);
        var origin = new MotionPose(Vector3.zero, Vector3.one, Quaternion.identity);

        var sequence = CharacterMotionMath.BuildSequence(origin, preset);

        Assert.AreEqual(1, sequence.Count);
        Assert.That(sequence[0].Position.x, Is.EqualTo(.15f).Within(.0001f));
    }

    [Test]
    public void LibraryFindIsCaseInsensitiveAndTrimmed()
    {
        var library = ScriptableObject.CreateInstance<CharacterMotionLibrary>();
        var preset = new CharacterMotionPreset("Shock", Vector3.zero, Vector3.one, Vector3.zero,
            .1f, EasingType.Linear, false, 1, true, 0);
        library.ReplacePresets(new[] { preset });

        Assert.AreSame(preset, library.Find(" shock "));
        Assert.IsNull(library.Find("missing"));

        Object.DestroyImmediate(library);
    }
}
