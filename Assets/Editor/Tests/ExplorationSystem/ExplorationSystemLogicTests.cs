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
        [TestCase(12, 0.99f, 12f, 11)]
        [TestCase(12, 1f, 12f, 0)]
        public void CalculateFrameIndex_CyclesAcrossWalkFrames(int frameCount, float elapsed, float framesPerSecond, int expected)
        {
            Assert.That(ExplorationSpriteAnimator.CalculateFrameIndex(frameCount, elapsed, framesPerSecond), Is.EqualTo(expected));
        }

        [TestCase(12, 2, 2, 2)]
        [TestCase(12, 3, 2, 8)]
        [TestCase(12, 7, 2, 8)]
        [TestCase(12, 9, 2, 2)]
        [TestCase(12, 11, 2, 2)]
        [TestCase(8, 1, 0, 4)]
        [TestCase(8, 5, 0, 0)]
        public void CalculateStopFrameIndex_SelectsTheNextNearbyContactPose(
            int frameCount,
            int currentFrame,
            int primaryIdleFrame,
            int expected)
        {
            Assert.That(
                ExplorationSpriteAnimator.CalculateStopFrameIndex(
                    frameCount,
                    currentFrame,
                    primaryIdleFrame),
                Is.EqualTo(expected));
        }

        [TestCase(10, 10, true, false, false)]
        [TestCase(10, 11, true, false, true)]
        [TestCase(10, 11, false, true, true)]
        [TestCase(10, 11, false, false, false)]
        public void ShouldForwardContinueInput_IgnoresTheOpeningPressAndAcceptsEOrSpace(
            int openedFrame,
            int currentFrame,
            bool ePressed,
            bool spacePressed,
            bool expected)
        {
            Assert.That(
                NaninovelDialogueInteractable.ShouldForwardContinueInput(
                    openedFrame,
                    currentFrame,
                    ePressed,
                    spacePressed),
                Is.EqualTo(expected));
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

        [Test]
        public void InventoryDatabase_GetAcquired_ReturnsOnlyPickedUpItems()
        {
            var database = ScriptableObject.CreateInstance<InventoryDatabase>();
            var acquired = ScriptableObject.CreateInstance<InventoryItem>();
            var unacquired = ScriptableObject.CreateInstance<InventoryItem>();
            database.items.Add(acquired);
            database.items.Add(unacquired);

            try
            {
                InventoryDatabase.ClearAcquired();
                Assert.That(database.Acquire(acquired), Is.True);
                Assert.That(database.GetAcquired(), Is.EquivalentTo(new[] { acquired }));
            }
            finally
            {
                InventoryDatabase.ClearAcquired();
                Object.DestroyImmediate(database);
                Object.DestroyImmediate(acquired);
                Object.DestroyImmediate(unacquired);
            }
        }

        [TestCase(ExplorationOutfit.Default, true, ExplorationOutfit.Wardrobe, false)]
        [TestCase(ExplorationOutfit.Wardrobe, true, ExplorationOutfit.Wardrobe, true)]
        [TestCase(ExplorationOutfit.Default, false, ExplorationOutfit.Wardrobe, true)]
        public void ShouldUseRequiredOutfit_BranchesForDoorAccess(
            ExplorationOutfit current,
            bool required,
            ExplorationOutfit target,
            bool expected)
        {
            Assert.That(
                NaninovelDialogueInteractable.ShouldUseRequiredOutfit(current, required, target),
                Is.EqualTo(expected));
        }

        [TestCase(true, true, false)]
        [TestCase(true, false, true)]
        [TestCase(false, true, true)]
        [TestCase(false, false, true)]
        public void ShouldUseFallbackWhenPrinterUnavailable_CoversEngineAndPrinterFailures(
            bool engineInitialized,
            bool printerPrepared,
            bool expected)
        {
            Assert.That(
                NaninovelDialogueInteractable.ShouldUseFallbackWhenPrinterUnavailable(
                    engineInitialized, printerPrepared),
                Is.EqualTo(expected));
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("   ", false)]
        [TestCase("Scenario/scene02", true)]
        public void ShouldTransitionToNovel_RequiresANextScript(string nextScriptPath, bool expected)
        {
            Assert.That(
                NaninovelDialogueInteractable.ShouldTransitionToNovel(nextScriptPath),
                Is.EqualTo(expected));
        }

        [TestCase(false, false, false, false)]
        [TestCase(true, false, false, true)]
        [TestCase(false, true, false, true)]
        [TestCase(false, false, true, true)]
        public void ShouldInteract_UsesKeyboardFallbackEvenWhenAnActionIsConfigured(
            bool actionPressed,
            bool ePressed,
            bool spacePressed,
            bool expected)
        {
            Assert.That(
                ExplorationInteractionController.ShouldInteract(actionPressed, ePressed, spacePressed),
                Is.EqualTo(expected));
        }

        [Test]
        public void GetWalkPose_AllTwelvePosesContainFiniteValues()
        {
            for (var poseIndex = 0; poseIndex < ExplorationCutoutRigController.WalkPoseCount; poseIndex++)
            {
                var pose = ExplorationCutoutRigController.GetWalkPose(poseIndex);
                Assert.That(float.IsFinite(pose.LeftHipAngle), Is.True);
                Assert.That(float.IsFinite(pose.RightHipAngle), Is.True);
                Assert.That(float.IsFinite(pose.LeftKneeBend), Is.True);
                Assert.That(float.IsFinite(pose.RightKneeBend), Is.True);
                Assert.That(float.IsFinite(pose.LeftAnkleAngle), Is.True);
                Assert.That(float.IsFinite(pose.RightAnkleAngle), Is.True);
                Assert.That(float.IsFinite(pose.LeftShoulderAngle), Is.True);
                Assert.That(float.IsFinite(pose.RightShoulderAngle), Is.True);
                Assert.That(float.IsFinite(pose.LeftElbowBend), Is.True);
                Assert.That(float.IsFinite(pose.RightElbowBend), Is.True);
                Assert.That(float.IsFinite(pose.BodyYOffset), Is.True);
                Assert.That(float.IsFinite(pose.BodyTilt), Is.True);
            }
        }

        [Test]
        public void GetWalkPose_HalfCycleInvertsLegSwing()
        {
            for (var poseIndex = 0; poseIndex < ExplorationCutoutRigController.WalkPoseCount / 2; poseIndex++)
            {
                var firstHalfPose = ExplorationCutoutRigController.GetWalkPose(poseIndex);
                var secondHalfPose = ExplorationCutoutRigController.GetWalkPose(poseIndex + ExplorationCutoutRigController.WalkPoseCount / 2);
                Assert.That(firstHalfPose.LeftHipAngle, Is.EqualTo(secondHalfPose.RightHipAngle).Within(0.001f));
                Assert.That(firstHalfPose.RightHipAngle, Is.EqualTo(secondHalfPose.LeftHipAngle).Within(0.001f));
                Assert.That(firstHalfPose.LeftKneeBend, Is.EqualTo(secondHalfPose.RightKneeBend).Within(0.001f));
                Assert.That(firstHalfPose.RightKneeBend, Is.EqualTo(secondHalfPose.LeftKneeBend).Within(0.001f));
                Assert.That(firstHalfPose.LeftShoulderAngle, Is.EqualTo(secondHalfPose.RightShoulderAngle).Within(0.001f));
                Assert.That(firstHalfPose.RightShoulderAngle, Is.EqualTo(secondHalfPose.LeftShoulderAngle).Within(0.001f));
            }
        }

        [Test]
        public void GetWalkPose_KneeBendsNeverHyperextend()
        {
            for (var poseIndex = 0; poseIndex < ExplorationCutoutRigController.WalkPoseCount; poseIndex++)
            {
                var pose = ExplorationCutoutRigController.GetWalkPose(poseIndex);
                Assert.That(pose.LeftKneeBend, Is.GreaterThanOrEqualTo(0f));
                Assert.That(pose.RightKneeBend, Is.GreaterThanOrEqualTo(0f));
            }
        }

        [Test]
        public void AreBoneReferencesStable_RequiresEachSideToKeepItsOwnTransform()
        {
            var leftLeg = new GameObject("LeftLeg").transform;
            var rightLeg = new GameObject("RightLeg").transform;
            var leftArm = new GameObject("LeftArm").transform;
            var rightArm = new GameObject("RightArm").transform;

            try
            {
                Assert.That(ExplorationCutoutRigController.AreBoneReferencesStable(
                    leftLeg, rightLeg, leftArm, rightArm,
                    leftLeg, rightLeg, leftArm, rightArm), Is.True);
                Assert.That(ExplorationCutoutRigController.AreBoneReferencesStable(
                    leftLeg, rightLeg, leftArm, rightArm,
                    rightLeg, leftLeg, leftArm, rightArm), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(leftLeg.gameObject);
                Object.DestroyImmediate(rightLeg.gameObject);
                Object.DestroyImmediate(leftArm.gameObject);
                Object.DestroyImmediate(rightArm.gameObject);
            }
        }

        [Test]
        public void AreLimbChainReferencesStable_RejectsSwappingLeftAndRightBones()
        {
            var bones = new[]
            {
                new GameObject("LeftUpperArm").transform,
                new GameObject("LeftForearm").transform,
                new GameObject("LeftThigh").transform,
                new GameObject("LeftCalf").transform,
                new GameObject("LeftFoot").transform,
                new GameObject("RightUpperArm").transform,
                new GameObject("RightForearm").transform,
                new GameObject("RightThigh").transform,
                new GameObject("RightCalf").transform,
                new GameObject("RightFoot").transform
            };

            try
            {
                var swapped = (Transform[])bones.Clone();
                (swapped[0], swapped[5]) = (swapped[5], swapped[0]);
                Assert.That(ExplorationCutoutRigController.AreLimbChainReferencesStable(bones, bones), Is.True);
                Assert.That(ExplorationCutoutRigController.AreLimbChainReferencesStable(bones, swapped), Is.False);
            }
            finally
            {
                foreach (var bone in bones)
                    Object.DestroyImmediate(bone.gameObject);
            }
        }
    }
}
