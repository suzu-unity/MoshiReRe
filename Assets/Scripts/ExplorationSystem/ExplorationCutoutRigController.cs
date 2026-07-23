using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshiReRe.Exploration
{
    [Serializable]
    public struct ExplorationCutoutOutfitSprites
    {
        [FormerlySerializedAs("body")] public Sprite torso;
        public Sprite head;
        public Sprite backHair;
        [FormerlySerializedAs("leftArm")] public Sprite upperArm;
        public Sprite forearm;
        [FormerlySerializedAs("leftLeg")] public Sprite thigh;
        public Sprite calf;
        public Sprite foot;
    }

    public readonly struct ExplorationCutoutWalkPose
    {
        public readonly float LeftHipAngle;
        public readonly float RightHipAngle;
        public readonly float LeftKneeBend;
        public readonly float RightKneeBend;
        public readonly float LeftAnkleAngle;
        public readonly float RightAnkleAngle;
        public readonly float LeftShoulderAngle;
        public readonly float RightShoulderAngle;
        public readonly float LeftElbowBend;
        public readonly float RightElbowBend;
        public readonly float BodyYOffset;
        public readonly float BodyTilt;

        // Kept as convenience aliases for the first one-bone rig revision.
        public float LeftLegAngle => LeftHipAngle;
        public float RightLegAngle => RightHipAngle;
        public float LeftArmAngle => LeftShoulderAngle;
        public float RightArmAngle => RightShoulderAngle;

        public ExplorationCutoutWalkPose(
            float leftHipAngle,
            float rightHipAngle,
            float leftKneeBend,
            float rightKneeBend,
            float leftAnkleAngle,
            float rightAnkleAngle,
            float leftShoulderAngle,
            float rightShoulderAngle,
            float leftElbowBend,
            float rightElbowBend,
            float bodyYOffset,
            float bodyTilt)
        {
            LeftHipAngle = leftHipAngle;
            RightHipAngle = rightHipAngle;
            LeftKneeBend = leftKneeBend;
            RightKneeBend = rightKneeBend;
            LeftAnkleAngle = leftAnkleAngle;
            RightAnkleAngle = rightAnkleAngle;
            LeftShoulderAngle = leftShoulderAngle;
            RightShoulderAngle = rightShoulderAngle;
            LeftElbowBend = leftElbowBend;
            RightElbowBend = rightElbowBend;
            BodyYOffset = bodyYOffset;
            BodyTilt = bodyTilt;
        }
    }

    /// <summary>Drives fixed left/right cutout limb chains through a contact/down/passing/up walk cycle.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationCutoutRigController : MonoBehaviour
    {
        private struct BoneRestPose
        {
            public Vector3 Position;
            public Quaternion Rotation;

            public BoneRestPose(Transform bone)
            {
                Position = bone == null ? Vector3.zero : bone.localPosition;
                Rotation = bone == null ? Quaternion.identity : bone.localRotation;
            }
        }

        public const int WalkPoseCount = 12;

        // 0-5 are left contact -> down -> passing -> up; 6-11 exchange the left/right values.
        private static readonly ExplorationCutoutWalkPose[] WalkPoses =
        {
            new(18f, -18f, 8f, 28f, -4f, 8f, -14f, 14f, 8f, 20f, 0f, -1f),
            new(14f, -14f, 22f, 42f, -6f, 12f, -11f, 11f, 14f, 26f, -0.05f, -2f),
            new(5f, -5f, 38f, 30f, -8f, 9f, -4f, 4f, 24f, 18f, -0.025f, -1f),
            new(-8f, 8f, 46f, 12f, 0f, 3f, 7f, -7f, 30f, 8f, 0.015f, 1f),
            new(-18f, 18f, 35f, 6f, 8f, -4f, 14f, -14f, 22f, 6f, 0.04f, 2f),
            new(-12f, 12f, 18f, 10f, 5f, -2f, 9f, -9f, 12f, 8f, 0.02f, 1f),
            new(-18f, 18f, 28f, 8f, 8f, -4f, 14f, -14f, 20f, 8f, 0f, 1f),
            new(-14f, 14f, 42f, 22f, 12f, -6f, 11f, -11f, 26f, 14f, -0.05f, 2f),
            new(-5f, 5f, 30f, 38f, 9f, -8f, 4f, -4f, 18f, 24f, -0.025f, 1f),
            new(8f, -8f, 12f, 46f, 3f, 0f, -7f, 7f, 8f, 30f, 0.015f, -1f),
            new(18f, -18f, 6f, 35f, -4f, 8f, -14f, 14f, 6f, 22f, 0.04f, -2f),
            new(12f, -12f, 10f, 18f, -2f, 5f, -9f, 9f, 8f, 12f, 0.02f, -1f)
        };

        [Header("Persistent Bones")]
        [FormerlySerializedAs("body")]
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private Transform backHair;
        [FormerlySerializedAs("leftArm")]
        [SerializeField] private Transform leftUpperArm;
        [FormerlySerializedAs("rightArm")]
        [SerializeField] private Transform rightUpperArm;
        [SerializeField] private Transform leftForearm;
        [SerializeField] private Transform rightForearm;
        [FormerlySerializedAs("leftLeg")]
        [SerializeField] private Transform leftThigh;
        [FormerlySerializedAs("rightLeg")]
        [SerializeField] private Transform rightThigh;
        [SerializeField] private Transform leftCalf;
        [SerializeField] private Transform rightCalf;
        [SerializeField] private Transform leftFoot;
        [SerializeField] private Transform rightFoot;
        [SerializeField, Tooltip("Optional root to mirror. Left and right references are never exchanged.")]
        private Transform mirrorRoot;

        [Header("Renderers")]
        [FormerlySerializedAs("bodyRenderer")]
        [SerializeField] private SpriteRenderer torsoRenderer;
        [SerializeField] private SpriteRenderer headRenderer;
        [SerializeField] private SpriteRenderer backHairRenderer;
        [FormerlySerializedAs("leftArmRenderer")]
        [SerializeField] private SpriteRenderer leftUpperArmRenderer;
        [FormerlySerializedAs("rightArmRenderer")]
        [SerializeField] private SpriteRenderer rightUpperArmRenderer;
        [SerializeField] private SpriteRenderer leftForearmRenderer;
        [SerializeField] private SpriteRenderer rightForearmRenderer;
        [FormerlySerializedAs("leftLegRenderer")]
        [SerializeField] private SpriteRenderer leftThighRenderer;
        [FormerlySerializedAs("rightLegRenderer")]
        [SerializeField] private SpriteRenderer rightThighRenderer;
        [SerializeField] private SpriteRenderer leftCalfRenderer;
        [SerializeField] private SpriteRenderer rightCalfRenderer;
        [SerializeField] private SpriteRenderer leftFootRenderer;
        [SerializeField] private SpriteRenderer rightFootRenderer;
        [FormerlySerializedAs("backLegRenderer")]
        [SerializeField, Tooltip("Legacy diagnostic renderer; optional when backLimbSide is configured.")]
        private SpriteRenderer legacyBackLegRenderer;
        [FormerlySerializedAs("backArmRenderer")]
        [SerializeField, Tooltip("Legacy diagnostic renderer; optional when backLimbSide is configured.")]
        private SpriteRenderer legacyBackArmRenderer;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float walkPosesPerSecond = 8f;
        [SerializeField, Min(0f)] private float neutralLegAngle = 2f;
        [SerializeField, Min(0f)] private float neutralArmAngle = 2f;
        [SerializeField] private bool showBackLimbsInBlack;
        [SerializeField, Tooltip("The persistent limb side to draw black while diagnostics are enabled.")]
        private ExplorationCutoutRigSide backLimbSide = ExplorationCutoutRigSide.Left;

        [Header("Outfits")]
        [SerializeField] private ExplorationCutoutOutfitSprites defaultOutfit;
        [SerializeField] private ExplorationCutoutOutfitSprites wardrobeOutfit;

        private readonly Dictionary<SpriteRenderer, Color> diagnosticColors = new();
        private bool walking;
        private bool facingRight = true;
        private float walkElapsed;
        private ExplorationOutfit outfit;
        private bool restPoseCaptured;
        private BoneRestPose torsoRestPose;
        private BoneRestPose leftUpperArmRestPose;
        private BoneRestPose rightUpperArmRestPose;
        private BoneRestPose leftForearmRestPose;
        private BoneRestPose rightForearmRestPose;
        private BoneRestPose leftThighRestPose;
        private BoneRestPose rightThighRestPose;
        private BoneRestPose leftCalfRestPose;
        private BoneRestPose rightCalfRestPose;
        private BoneRestPose leftFootRestPose;
        private BoneRestPose rightFootRestPose;
        private Vector3 mirrorRootRestScale;

        // Kept for the initial public one-bone API; each now returns its root bone.
        public Transform LeftLeg => leftThigh;
        public Transform RightLeg => rightThigh;
        public Transform LeftArm => leftUpperArm;
        public Transform RightArm => rightUpperArm;
        public bool Walking => walking;
        public bool FacingRight => facingRight;
        public ExplorationOutfit Outfit => outfit;

        private void Reset()
        {
            mirrorRoot = transform;
        }

        private void Awake()
        {
            if (mirrorRoot == null)
                mirrorRoot = transform;

            CaptureRestPose();
            ApplyOutfitSprites();
            ApplyFacing();
            ApplyNeutralPose();
        }

        private void Update()
        {
            if (!walking)
                return;

            walkElapsed += Time.deltaTime;
            ApplyWalkPose(GetWalkPose(Mathf.FloorToInt(walkElapsed * walkPosesPerSecond)));
        }

        public void SetWalking(bool value)
        {
            EnsureRestPose();
            if (walking == value)
                return;

            walking = value;
            walkElapsed = 0f;
            if (walking)
                ApplyWalkPose(GetWalkPose(0));
            else
                ApplyNeutralPose();
        }

        public void SetFacingRight(bool value)
        {
            facingRight = value;
            EnsureRestPose();
            ApplyFacing();
        }

        public void SetOutfit(ExplorationOutfit value)
        {
            outfit = value;
            ApplyOutfitSprites();
        }

        public void SetBackLimbDiagnostic(bool value)
        {
            showBackLimbsInBlack = value;
            EnsureRestPose();
            ApplyDiagnosticColors();
        }

        [ContextMenu("Capture Current Rest Pose")]
        public void CaptureRestPose()
        {
            torsoRestPose = new BoneRestPose(torso);
            leftUpperArmRestPose = new BoneRestPose(leftUpperArm);
            rightUpperArmRestPose = new BoneRestPose(rightUpperArm);
            leftForearmRestPose = new BoneRestPose(leftForearm);
            rightForearmRestPose = new BoneRestPose(rightForearm);
            leftThighRestPose = new BoneRestPose(leftThigh);
            rightThighRestPose = new BoneRestPose(rightThigh);
            leftCalfRestPose = new BoneRestPose(leftCalf);
            rightCalfRestPose = new BoneRestPose(rightCalf);
            leftFootRestPose = new BoneRestPose(leftFoot);
            rightFootRestPose = new BoneRestPose(rightFoot);
            mirrorRootRestScale = mirrorRoot == null ? Vector3.one : mirrorRoot.localScale;
            CaptureDiagnosticColors();
            restPoseCaptured = true;
            ApplyDiagnosticColors();
        }

        public static ExplorationCutoutWalkPose GetWalkPose(int poseIndex)
        {
            var normalizedIndex = ((poseIndex % WalkPoseCount) + WalkPoseCount) % WalkPoseCount;
            return WalkPoses[normalizedIndex];
        }

        public static bool AreBoneReferencesStable(
            Transform originalLeftLeg,
            Transform originalRightLeg,
            Transform originalLeftArm,
            Transform originalRightArm,
            Transform currentLeftLeg,
            Transform currentRightLeg,
            Transform currentLeftArm,
            Transform currentRightArm)
        {
            return originalLeftLeg == currentLeftLeg &&
                   originalRightLeg == currentRightLeg &&
                   originalLeftArm == currentLeftArm &&
                   originalRightArm == currentRightArm;
        }

        public static bool AreLimbChainReferencesStable(IReadOnlyList<Transform> original, IReadOnlyList<Transform> current)
        {
            if (original == null || current == null || original.Count != current.Count)
                return false;

            for (var i = 0; i < original.Count; i++)
            {
                if (original[i] != current[i])
                    return false;
            }

            return true;
        }

        private void EnsureRestPose()
        {
            if (!restPoseCaptured)
                CaptureRestPose();
        }

        private void ApplyWalkPose(ExplorationCutoutWalkPose pose)
        {
            ApplyBone(torso, torsoRestPose, pose.BodyYOffset, pose.BodyTilt);
            ApplyBone(leftThigh, leftThighRestPose, 0f, pose.LeftHipAngle);
            ApplyBone(rightThigh, rightThighRestPose, 0f, pose.RightHipAngle);
            ApplyBone(leftCalf, leftCalfRestPose, 0f, pose.LeftKneeBend);
            ApplyBone(rightCalf, rightCalfRestPose, 0f, pose.RightKneeBend);
            ApplyBone(leftFoot, leftFootRestPose, 0f, pose.LeftAnkleAngle);
            ApplyBone(rightFoot, rightFootRestPose, 0f, pose.RightAnkleAngle);
            ApplyBone(leftUpperArm, leftUpperArmRestPose, 0f, pose.LeftShoulderAngle);
            ApplyBone(rightUpperArm, rightUpperArmRestPose, 0f, pose.RightShoulderAngle);
            ApplyBone(leftForearm, leftForearmRestPose, 0f, pose.LeftElbowBend);
            ApplyBone(rightForearm, rightForearmRestPose, 0f, pose.RightElbowBend);
        }

        private void ApplyNeutralPose()
        {
            ApplyBone(torso, torsoRestPose, 0f, 0f);
            ApplyBone(leftThigh, leftThighRestPose, 0f, -neutralLegAngle);
            ApplyBone(rightThigh, rightThighRestPose, 0f, neutralLegAngle);
            ApplyBone(leftCalf, leftCalfRestPose, 0f, neutralLegAngle * 2f);
            ApplyBone(rightCalf, rightCalfRestPose, 0f, neutralLegAngle * 2f);
            ApplyBone(leftFoot, leftFootRestPose, 0f, -neutralLegAngle);
            ApplyBone(rightFoot, rightFootRestPose, 0f, neutralLegAngle);
            ApplyBone(leftUpperArm, leftUpperArmRestPose, 0f, neutralArmAngle);
            ApplyBone(rightUpperArm, rightUpperArmRestPose, 0f, -neutralArmAngle);
            ApplyBone(leftForearm, leftForearmRestPose, 0f, neutralArmAngle * 2f);
            ApplyBone(rightForearm, rightForearmRestPose, 0f, neutralArmAngle * 2f);
        }

        private void ApplyFacing()
        {
            if (mirrorRoot == null)
                return;

            var scale = mirrorRootRestScale;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
            mirrorRoot.localScale = scale;
        }

        private void ApplyOutfitSprites()
        {
            var selectedOutfit = outfit == ExplorationOutfit.Wardrobe ? wardrobeOutfit : defaultOutfit;
            AssignSpriteIfProvided(torsoRenderer, selectedOutfit.torso);
            AssignSpriteIfProvided(headRenderer, selectedOutfit.head);
            AssignSpriteIfProvided(backHairRenderer, selectedOutfit.backHair);
            AssignSpriteToPair(leftUpperArmRenderer, rightUpperArmRenderer, selectedOutfit.upperArm);
            AssignSpriteToPair(leftForearmRenderer, rightForearmRenderer, selectedOutfit.forearm);
            AssignSpriteToPair(leftThighRenderer, rightThighRenderer, selectedOutfit.thigh);
            AssignSpriteToPair(leftCalfRenderer, rightCalfRenderer, selectedOutfit.calf);
            AssignSpriteToPair(leftFootRenderer, rightFootRenderer, selectedOutfit.foot);
        }

        private void CaptureDiagnosticColors()
        {
            diagnosticColors.Clear();
            CaptureColor(leftUpperArmRenderer);
            CaptureColor(rightUpperArmRenderer);
            CaptureColor(leftForearmRenderer);
            CaptureColor(rightForearmRenderer);
            CaptureColor(leftThighRenderer);
            CaptureColor(rightThighRenderer);
            CaptureColor(leftCalfRenderer);
            CaptureColor(rightCalfRenderer);
            CaptureColor(leftFootRenderer);
            CaptureColor(rightFootRenderer);
            CaptureColor(legacyBackLegRenderer);
            CaptureColor(legacyBackArmRenderer);
        }

        private void ApplyDiagnosticColors()
        {
            RestoreDiagnosticColors();
            if (!showBackLimbsInBlack)
                return;

            var useLeftSide = backLimbSide == ExplorationCutoutRigSide.Left;
            SetColor(useLeftSide ? leftUpperArmRenderer : rightUpperArmRenderer, Color.black);
            SetColor(useLeftSide ? leftForearmRenderer : rightForearmRenderer, Color.black);
            SetColor(useLeftSide ? leftThighRenderer : rightThighRenderer, Color.black);
            SetColor(useLeftSide ? leftCalfRenderer : rightCalfRenderer, Color.black);
            SetColor(useLeftSide ? leftFootRenderer : rightFootRenderer, Color.black);
            SetColor(legacyBackArmRenderer, Color.black);
            SetColor(legacyBackLegRenderer, Color.black);
        }

        private void RestoreDiagnosticColors()
        {
            foreach (var colorPair in diagnosticColors)
            {
                if (colorPair.Key != null)
                    colorPair.Key.color = colorPair.Value;
            }
        }

        private void CaptureColor(SpriteRenderer renderer)
        {
            if (renderer != null && !diagnosticColors.ContainsKey(renderer))
                diagnosticColors.Add(renderer, renderer.color);
        }

        private static void ApplyBone(Transform bone, BoneRestPose restPose, float yOffset, float zAngle)
        {
            if (bone == null)
                return;

            bone.localPosition = restPose.Position + Vector3.up * yOffset;
            bone.localRotation = restPose.Rotation * Quaternion.Euler(0f, 0f, zAngle);
        }

        private static void AssignSpriteToPair(SpriteRenderer leftRenderer, SpriteRenderer rightRenderer, Sprite sprite)
        {
            AssignSpriteIfProvided(leftRenderer, sprite);
            AssignSpriteIfProvided(rightRenderer, sprite);
        }

        private static void AssignSpriteIfProvided(SpriteRenderer renderer, Sprite sprite)
        {
            if (renderer != null && sprite != null)
                renderer.sprite = sprite;
        }

        private static void SetColor(SpriteRenderer renderer, Color color)
        {
            if (renderer != null)
                renderer.color = color;
        }
    }

    public enum ExplorationCutoutRigSide
    {
        Left,
        Right
    }
}
