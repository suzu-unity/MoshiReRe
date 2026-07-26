using UnityEngine;

namespace MoshiReRe.Exploration
{
    public enum ExplorationOutfit
    {
        Default,
        Wardrobe
    }

    /// <summary>Plays a supplied side-view walk-frame sequence without assuming a sprite-sheet layout.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField, Min(0.01f)] private float framesPerSecond = 8f;
        [SerializeField, Tooltip("Assign the default-outfit walk frames in display order.")]
        private Sprite[] defaultWalkFrames;
        [SerializeField, Tooltip("Assign the wardrobe-outfit walk frames in display order.")]
        private Sprite[] wardrobeWalkFrames;
        [SerializeField] private Sprite defaultIdleSprite;
        [SerializeField] private Sprite wardrobeIdleSprite;
        [SerializeField, Tooltip("Primary contact-pose frame used when the default outfit comes to rest.")]
        private int defaultIdleFrameIndex = 2;
        [SerializeField, Tooltip("Primary contact-pose frame used when the wardrobe outfit comes to rest.")]
        private int wardrobeIdleFrameIndex;
        [SerializeField, Range(0f, 0.03f)] private float idleBreathScale = 0.006f;
        [SerializeField, Min(0.01f)] private float idleBreathCyclesPerSecond = 0.22f;
        [SerializeField, Tooltip("Optional cutout rig. When assigned, it replaces frame sprites while preserving this component's public API.")]
        private ExplorationCutoutRigController cutoutRig;

        private ExplorationOutfit outfit;
        private bool walking;
        private float walkElapsed;
        private bool settling;
        private int settleStartFrame;
        private int settleTargetFrame;
        private int settleFrameCount;
        private float settleElapsed;
        private float idleElapsed;
        private Sprite activeIdleSprite;
        private Vector3 visualBaseScale = Vector3.one;

        public ExplorationOutfit Outfit => outfit;

        private void Reset()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
                visualBaseScale = spriteRenderer.transform.localScale;

            if (cutoutRig != null)
            {
                if (spriteRenderer != null)
                    spriteRenderer.enabled = false;
                cutoutRig.SetOutfit(outfit);
            }

            walkElapsed = GetPrimaryIdleFrameIndex() / framesPerSecond;
            RefreshSprite();
        }

        private void Update()
        {
            if (cutoutRig != null)
                return;

            if (walking)
            {
                walkElapsed += Time.deltaTime;
                RestoreVisualScale();
                RefreshSprite();
                return;
            }

            if (settling)
            {
                UpdateSettling(Time.deltaTime);
                return;
            }

            idleElapsed += Time.deltaTime;
            ApplyIdleBreath();
        }

        public void SetWalking(bool value)
        {
            cutoutRig?.SetWalking(value);
            if (walking == value)
                return;

            walking = value;
            idleElapsed = 0f;
            RestoreVisualScale();

            if (walking)
            {
                settling = false;
                activeIdleSprite = null;
                RefreshSprite();
                return;
            }

            BeginSettling();
            RefreshSprite();
        }

        public void SetFacingRight(bool facingRight)
        {
            cutoutRig?.SetFacingRight(facingRight);
            if (spriteRenderer != null)
                spriteRenderer.flipX = !facingRight;
        }

        public void SetOutfit(ExplorationOutfit value)
        {
            cutoutRig?.SetOutfit(value);
            if (outfit == value)
                return;

            outfit = value;
            settling = false;
            activeIdleSprite = null;
            idleElapsed = 0f;
            walkElapsed = GetPrimaryIdleFrameIndex() / framesPerSecond;
            RestoreVisualScale();
            RefreshSprite();
        }

        public static int CalculateFrameIndex(int frameCount, float elapsed, float framesPerSecond)
        {
            if (frameCount <= 0 || framesPerSecond <= 0f)
                return 0;

            var frame = Mathf.FloorToInt(Mathf.Max(0f, elapsed) * framesPerSecond);
            return frame % frameCount;
        }

        public static int CalculateStopFrameIndex(int frameCount, int currentFrameIndex, int primaryIdleFrameIndex)
        {
            if (frameCount <= 0)
                return 0;

            var current = PositiveModulo(currentFrameIndex, frameCount);
            var primary = PositiveModulo(primaryIdleFrameIndex, frameCount);
            var opposite = PositiveModulo(primary + frameCount / 2, frameCount);
            var primaryDistance = PositiveModulo(primary - current, frameCount);
            var oppositeDistance = PositiveModulo(opposite - current, frameCount);
            return oppositeDistance < primaryDistance ? opposite : primary;
        }

        private void BeginSettling()
        {
            var frames = GetCurrentWalkFrames();
            if (frames == null || frames.Length == 0)
            {
                settling = false;
                return;
            }

            settleStartFrame = CalculateFrameIndex(frames.Length, walkElapsed, framesPerSecond);
            settleTargetFrame = CalculateStopFrameIndex(
                frames.Length,
                settleStartFrame,
                GetPrimaryIdleFrameIndex());
            settleFrameCount = PositiveModulo(settleTargetFrame - settleStartFrame, frames.Length);
            settleElapsed = 0f;

            if (settleFrameCount == 0)
            {
                CompleteSettling(frames);
                return;
            }

            settling = true;
            spriteRenderer.sprite = frames[settleStartFrame];
        }

        private void UpdateSettling(float deltaTime)
        {
            var frames = GetCurrentWalkFrames();
            if (frames == null || frames.Length == 0)
            {
                settling = false;
                return;
            }

            settleElapsed += Mathf.Max(0f, deltaTime);
            var advancedFrames = Mathf.Min(
                settleFrameCount,
                Mathf.FloorToInt(settleElapsed * framesPerSecond));
            var frameIndex = (settleStartFrame + advancedFrames) % frames.Length;
            walkElapsed = frameIndex / framesPerSecond;
            spriteRenderer.sprite = frames[frameIndex];

            if (advancedFrames >= settleFrameCount)
                CompleteSettling(frames);
        }

        private void CompleteSettling(Sprite[] frames)
        {
            settling = false;
            activeIdleSprite = frames[settleTargetFrame];
            walkElapsed = settleTargetFrame / framesPerSecond;
            idleElapsed = 0f;
            spriteRenderer.sprite = activeIdleSprite;
        }

        private void ApplyIdleBreath()
        {
            if (spriteRenderer == null)
                return;

            var phase = idleElapsed * idleBreathCyclesPerSecond * Mathf.PI * 2f;
            var breath = (0.5f - 0.5f * Mathf.Cos(phase)) * idleBreathScale;
            spriteRenderer.transform.localScale = new Vector3(
                visualBaseScale.x * (1f - breath * 0.25f),
                visualBaseScale.y * (1f + breath),
                visualBaseScale.z);
        }

        private void RestoreVisualScale()
        {
            if (spriteRenderer != null)
                spriteRenderer.transform.localScale = visualBaseScale;
        }

        private Sprite[] GetCurrentWalkFrames() =>
            outfit == ExplorationOutfit.Wardrobe ? wardrobeWalkFrames : defaultWalkFrames;

        private int GetPrimaryIdleFrameIndex() =>
            outfit == ExplorationOutfit.Wardrobe ? wardrobeIdleFrameIndex : defaultIdleFrameIndex;

        private static int PositiveModulo(int value, int modulus)
        {
            var result = value % modulus;
            return result < 0 ? result + modulus : result;
        }

        private void RefreshSprite()
        {
            if (cutoutRig != null)
                return;

            if (spriteRenderer == null)
                return;

            var frames = GetCurrentWalkFrames();
            if (walking && frames != null && frames.Length > 0)
            {
                spriteRenderer.sprite = frames[CalculateFrameIndex(frames.Length, walkElapsed, framesPerSecond)];
                return;
            }

            if (settling)
                return;

            if (activeIdleSprite != null)
            {
                spriteRenderer.sprite = activeIdleSprite;
                return;
            }

            var idleSprite = outfit == ExplorationOutfit.Wardrobe ? wardrobeIdleSprite : defaultIdleSprite;
            if (idleSprite != null)
                spriteRenderer.sprite = idleSprite;
            else if (frames != null && frames.Length > 0)
                spriteRenderer.sprite = frames[0];
        }
    }
}
