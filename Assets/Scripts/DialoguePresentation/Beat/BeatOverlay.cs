using UnityEngine;
using UnityEngine.UI;

namespace MoshiReRe.DialoguePresentation.Beat
{
    /// <summary>
    /// Small, raycast-free global overlay used by beat presets.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BeatOverlay : MonoBehaviour
    {
        private const int SortingOrder = 32000;

        [SerializeField] private Canvas canvas;
        [SerializeField] private Image flashImage;
        [SerializeField] private Image blackoutImage;

        public static BeatOverlay FindOrCreateRuntime()
        {
            var existing = FindAnyObjectByType<BeatOverlay>();
            if (existing) return existing;

            var root = new GameObject("BeatOverlay");
            DontDestroyOnLoad(root);
            var overlay = root.AddComponent<BeatOverlay>();
            overlay.EnsureVisuals();
            return overlay;
        }

        public void Set(Color flashColor, float flashAlpha, float blackoutAlpha)
        {
            EnsureVisuals();
            var safeFlashAlpha = Mathf.Clamp01(flashAlpha) * Mathf.Clamp01(flashColor.a);
            var safeBlackoutAlpha = Mathf.Clamp01(blackoutAlpha);
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, safeFlashAlpha);
            blackoutImage.color = new Color(0f, 0f, 0f, safeBlackoutAlpha);
            canvas.enabled = safeFlashAlpha > 0f || safeBlackoutAlpha > 0f;
        }

        public void Clear()
        {
            if (!canvas) return;
            if (!flashImage || !blackoutImage) EnsureVisuals();
            flashImage.color = Color.clear;
            blackoutImage.color = Color.clear;
            canvas.enabled = false;
        }

        private void EnsureVisuals()
        {
            if (!canvas) canvas = GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = SortingOrder;

            if (!flashImage) flashImage = FindImage("Flash") ?? CreateImage("Flash");
            if (!blackoutImage) blackoutImage = FindImage("Blackout") ?? CreateImage("Blackout");

            flashImage.raycastTarget = false;
            blackoutImage.raycastTarget = false;
            flashImage.color = Color.clear;
            blackoutImage.color = Color.clear;
            canvas.enabled = false;
        }

        private Image FindImage(string objectName)
        {
            var child = transform.Find(objectName);
            return child ? child.GetComponent<Image>() : null;
        }

        private Image CreateImage(string objectName)
        {
            var child = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            child.transform.SetParent(transform, false);
            var rect = child.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return child.GetComponent<Image>();
        }
    }
}
