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
        [SerializeField, Tooltip("Assign the five default-outfit walk frames in display order.")]
        private Sprite[] defaultWalkFrames;
        [SerializeField, Tooltip("Assign the five wardrobe-outfit walk frames in display order.")]
        private Sprite[] wardrobeWalkFrames;
        [SerializeField] private Sprite defaultIdleSprite;
        [SerializeField] private Sprite wardrobeIdleSprite;

        private ExplorationOutfit outfit;
        private bool walking;
        private float walkElapsed;

        public ExplorationOutfit Outfit => outfit;

        private void Reset()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            RefreshSprite();
        }

        private void Update()
        {
            if (!walking)
                return;

            walkElapsed += Time.deltaTime;
            RefreshSprite();
        }

        public void SetWalking(bool value)
        {
            if (walking == value)
                return;

            walking = value;
            walkElapsed = 0f;
            RefreshSprite();
        }

        public void SetFacingRight(bool facingRight)
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = !facingRight;
        }

        public void SetOutfit(ExplorationOutfit value)
        {
            if (outfit == value)
                return;

            outfit = value;
            walkElapsed = 0f;
            RefreshSprite();
        }

        public static int CalculateFrameIndex(int frameCount, float elapsed, float framesPerSecond)
        {
            if (frameCount <= 0 || framesPerSecond <= 0f)
                return 0;

            var frame = Mathf.FloorToInt(Mathf.Max(0f, elapsed) * framesPerSecond);
            return frame % frameCount;
        }

        private void RefreshSprite()
        {
            if (spriteRenderer == null)
                return;

            var frames = outfit == ExplorationOutfit.Wardrobe ? wardrobeWalkFrames : defaultWalkFrames;
            if (walking && frames != null && frames.Length > 0)
            {
                spriteRenderer.sprite = frames[CalculateFrameIndex(frames.Length, walkElapsed, framesPerSecond)];
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
