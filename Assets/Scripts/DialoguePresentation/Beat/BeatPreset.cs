using System;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.Beat
{
    public enum BeatTimeMode
    {
        Unscaled,
        Scaled
    }

    [Serializable]
    public sealed class BeatPreset
    {
        [Tooltip("Name used by @beat type:<name>.")]
        [SerializeField] private string type;
        [Tooltip("Total beat duration in seconds.")]
        [SerializeField, Min(0f)] private float duration = 0.35f;
        [Tooltip("Choose whether duration follows Time.timeScale.")]
        [SerializeField] private BeatTimeMode timeMode = BeatTimeMode.Unscaled;
        [Tooltip("Optional one-shot sound. Leaving this empty is safe and silent.")]
        [SerializeField] private AudioClip sfx;
        [Tooltip("Volume used for the optional one-shot sound.")]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        [Tooltip("Color of the transient flash overlay.")]
        [SerializeField] private Color flashColor = Color.white;
        [Tooltip("Alpha of the flash overlay at the start of the beat.")]
        [SerializeField, Range(0f, 1f)] private float flashAlpha;
        [Tooltip("Alpha of the black dim overlay at the start of the beat.")]
        [SerializeField, Range(0f, 1f)] private float blackoutAlpha;
        [Tooltip("Temporarily hide all currently visible Naninovel text printers.")]
        [SerializeField] private bool hideTextPrinter;
        [Tooltip("Local camera shake amplitude. Zero disables shake.")]
        [SerializeField, Min(0f)] private float shakeAmplitude;
        [Tooltip("Camera shake oscillation frequency.")]
        [SerializeField, Min(0f)] private float shakeFrequency = 18f;
        [Tooltip("After the visual beat, hold Naninovel until Continue input.")]
        [SerializeField] private bool waitForInput;

        public string Type => type;
        public float Duration => Mathf.Max(0f, duration);
        public BeatTimeMode TimeMode => timeMode;
        public AudioClip Sfx => sfx;
        public float SfxVolume => Mathf.Clamp01(sfxVolume);
        public Color FlashColor => flashColor;
        public float FlashAlpha => Mathf.Clamp01(flashAlpha);
        public float BlackoutAlpha => Mathf.Clamp01(blackoutAlpha);
        public bool HideTextPrinter => hideTextPrinter;
        public float ShakeAmplitude => Mathf.Max(0f, shakeAmplitude);
        public float ShakeFrequency => Mathf.Max(0f, shakeFrequency);
        public bool WaitForInput => waitForInput;

        public BeatPreset() { }

        public BeatPreset(string type, float duration, BeatTimeMode timeMode, AudioClip sfx,
            float sfxVolume, Color flashColor, float flashAlpha, float blackoutAlpha,
            bool hideTextPrinter, float shakeAmplitude, float shakeFrequency, bool waitForInput)
        {
            this.type = type;
            this.duration = duration;
            this.timeMode = timeMode;
            this.sfx = sfx;
            this.sfxVolume = sfxVolume;
            this.flashColor = flashColor;
            this.flashAlpha = flashAlpha;
            this.blackoutAlpha = blackoutAlpha;
            this.hideTextPrinter = hideTextPrinter;
            this.shakeAmplitude = shakeAmplitude;
            this.shakeFrequency = shakeFrequency;
            this.waitForInput = waitForInput;
        }
    }

    public static class BeatTypeUtility
    {
        public static string Normalize(string type)
        {
            return string.IsNullOrWhiteSpace(type) ? string.Empty : type.Trim().ToLowerInvariant();
        }
    }

    public static class BeatTiming
    {
        public static float GetEffectiveDuration(float duration, bool skipActive)
        {
            return skipActive ? 0f : Mathf.Max(0f, duration);
        }

        public static float GetDelta(BeatTimeMode mode, float unscaledDelta, float scaledDelta)
        {
            return Mathf.Max(0f, mode == BeatTimeMode.Unscaled ? unscaledDelta : scaledDelta);
        }

        public static bool ShouldWaitForInput(bool configured, bool skipActive)
        {
            return configured && !skipActive;
        }
    }
}
