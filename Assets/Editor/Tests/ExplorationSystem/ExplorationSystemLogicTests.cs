using System.Collections.Generic;
using MoshiReRe.Exploration;
using NUnit.Framework;
using UnityEngine;

namespace MoshiReRe.EditorTests.ExplorationSystem
{
    public sealed class ExplorationSystemLogicTests
    {
        [TestCase(5, 0f, 8f, 0)]
        [TestCase(5, 0.13f, 8f, 1)]
        [TestCase(5, 0.75f, 8f, 1)]
        [TestCase(5, 1.25f, 8f, 0)]
        public void CalculateFrameIndex_CyclesAcrossWalkFrames(int frameCount, float elapsed, float framesPerSecond, int expected)
        {
            Assert.That(ExplorationSpriteAnimator.CalculateFrameIndex(frameCount, elapsed, framesPerSecond), Is.EqualTo(expected));
        }

        [Test]
        public void FindNearestIndex_ChoosesClosestCandidateWithinRadius()
        {
            var candidates = new List<Vector2> { new(-1f, 0f), new(0.25f, 0f), new(3f, 0f) };

            Assert.That(ExplorationInteractionController.FindNearestIndex(Vector2.zero, candidates, 2f), Is.EqualTo(1));
        }

        [Test]
        public void FindNearestIndex_ReturnsMinusOneWhenNothingIsInRange()
        {
            var candidates = new List<Vector2> { new(2f, 0f) };

            Assert.That(ExplorationInteractionController.FindNearestIndex(Vector2.zero, candidates, 1f), Is.EqualTo(-1));
        }

        [TestCase(12f, true, -3f, 5f, 5f)]
        [TestCase(-12f, true, -3f, 5f, -3f)]
        [TestCase(12f, false, -3f, 5f, 12f)]
        [TestCase(12f, true, 5f, -3f, 5f)]
        public void ClampHorizontalPosition_UsesOptionalNormalizedBounds(float positionX, bool enabled, float minX, float maxX, float expected)
        {
            Assert.That(ExplorationPlayerController.ClampHorizontalPosition(positionX, enabled, minX, maxX), Is.EqualTo(expected));
            Assert.That(SideScrollCamera.ClampHorizontalPosition(positionX, enabled, minX, maxX), Is.EqualTo(expected));
        }

        [TestCase(0f, 0.5f, 1f, 0f)]
        [TestCase(0f, 2f, 0.5f, 1.5f)]
        [TestCase(3f, 1f, 0.5f, 1.5f)]
        [TestCase(0f, 2f, 0f, 2f)]
        public void CalculateFollowX_UsesDeadZoneBeforePanning(float cameraX, float targetX, float deadZone, float expected)
        {
            Assert.That(SideScrollCamera.CalculateFollowX(cameraX, targetX, deadZone), Is.EqualTo(expected));
        }

        [TestCase(0, 1, true)]
        [TestCase(1, 2, true)]
        [TestCase(0, 2, false)]
        [TestCase(0, 0, true)]
        public void ShouldCloseAfterAdvance_ClosesOnlyOnFinalLine(int currentLineIndex, int lineCount, bool expected)
        {
            Assert.That(ExplorationDialogueOverlay.ShouldCloseAfterAdvance(currentLineIndex, lineCount), Is.EqualTo(expected));
        }
    }
}
