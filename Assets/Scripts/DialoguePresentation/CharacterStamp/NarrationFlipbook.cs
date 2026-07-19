using UnityEngine;
using UnityEngine.UI;

namespace MoshiReRe.DialoguePresentation.CharacterStamp
{
    /// <summary>
    /// Plays the narration book sprites assigned by the dialogue prefab builder.
    /// </summary>
    public sealed class NarrationFlipbook : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.01f)] private float fps = 6f;

        private float elapsed;
        private int frameIndex;

        public void Show ()
        {
            elapsed = 0f;
            frameIndex = 0;
            gameObject.SetActive(true);
            ApplyFrame();
        }

        public void Hide ()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable ()
        {
            ApplyFrame();
        }

        private void Update ()
        {
            if (frames == null || frames.Length < 2) return;

            elapsed += Time.unscaledDeltaTime;
            var frameDuration = 1f / Mathf.Max(0.01f, fps);
            if (elapsed < frameDuration) return;

            elapsed %= frameDuration;
            frameIndex = (frameIndex + 1) % frames.Length;
            ApplyFrame();
        }

        private void ApplyFrame ()
        {
            if (!image) return;

            var hasFrames = frames != null && frames.Length > 0;
            image.enabled = hasFrames;
            image.sprite = hasFrames ? frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)] : null;
        }
    }
}
