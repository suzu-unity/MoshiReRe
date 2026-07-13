using System;
using System.Collections.Generic;
using System.Threading;
using Naninovel;
using Naninovel.Commands;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.Beat
{
    /// <summary>
    /// Plays one library beat at a time. Add this component to a persistent gameplay object and assign a library.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BeatController : MonoBehaviour
    {
        public static BeatController Instance { get; private set; }

        [Tooltip("Library resolved by @beat type:<name>.")]
        [SerializeField] private BeatLibrary library;
        [Tooltip("Optional overlay. A global overlay is created at runtime only when needed and enabled.")]
        [SerializeField] private BeatOverlay overlay;
        [Tooltip("Optional one-shot source for preset SFX. Missing source or clips are safe.")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Optional shake component. A local component is created only when a preset needs shake.")]
        [SerializeField] private BeatScreenShake screenShake;
        [SerializeField] private bool autoCreateOverlay = true;
        [SerializeField] private bool autoCreateShake = true;

        private readonly List<ITextPrinterActor> hiddenPrinters = new();
        private CancellationTokenSource activeCancellation;
        private int runVersion;
        private int activeShakeHandle;

        public BeatLibrary Library => library;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                enabled = false;
                return;
            }

            Instance = this;
            if (!audioSource) audioSource = GetComponent<AudioSource>();
        }

        private void OnDisable()
        {
            CancelActive();
            if (Instance == this) Instance = null;
        }

        public async UniTask Play(string type, AsyncToken token = default)
        {
            // This guard deliberately precedes every Naninovel service lookup.
            if (!Engine.Initialized || !isActiveAndEnabled) return;

            var preset = library ? library.Find(type) : null;
            if (preset == null) return;

            CancelActive();
            var version = runVersion;
            activeCancellation = new CancellationTokenSource();
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                token.CancellationToken, token.CompletionToken, activeCancellation.Token);
            var playToken = (AsyncToken)linked.Token;
            var skipActive = IsSkipActive();
            var shakeHandle = BeginEffects(preset, skipActive);
            activeShakeHandle = shakeHandle;

            try
            {
                var duration = BeatTiming.GetEffectiveDuration(preset.Duration, skipActive);
                if (duration > 0f)
                    await WaitDuration(duration, preset.TimeMode, playToken);

                if (BeatTiming.ShouldWaitForInput(preset.WaitForInput, skipActive))
                    await WaitForInput(playToken);
            }
            catch (OperationCanceledException)
            {
                // A later beat superseding this one is a normal completion path.
                if (token.Canceled) token.ThrowIfCanceled();
            }
            finally
            {
                if (version == runVersion)
                {
                    overlay?.Clear();
                    screenShake?.Stop(shakeHandle);
                    activeShakeHandle = 0;
                    RestoreTextPrinters();
                    activeCancellation?.Dispose();
                    activeCancellation = null;
                }
            }
        }

        private int BeginEffects(BeatPreset preset, bool skipActive)
        {
            if (preset.Sfx)
            {
                if (!audioSource) audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.PlayOneShot(preset.Sfx, preset.SfxVolume);
            }

            if (preset.HideTextPrinter)
                HideTextPrinters();

            if (preset.FlashAlpha > 0f || preset.BlackoutAlpha > 0f)
            {
                if (!overlay && autoCreateOverlay)
                    overlay = BeatOverlay.FindOrCreateRuntime();
                overlay?.Set(preset.FlashColor, preset.FlashAlpha, preset.BlackoutAlpha);
            }

            if (preset.ShakeAmplitude <= 0f || (skipActive && preset.Duration <= 0f)) return 0;
            if (!screenShake && autoCreateShake)
                screenShake = gameObject.AddComponent<BeatScreenShake>();
            return screenShake
                ? screenShake.Play(Camera.main ? Camera.main.transform : null, preset.ShakeAmplitude,
                    preset.ShakeFrequency, preset.TimeMode == BeatTimeMode.Unscaled)
                : 0;
        }

        private async UniTask WaitDuration(float duration, BeatTimeMode mode, AsyncToken token)
        {
            var elapsed = 0f;
            while (elapsed < duration && token.EnsureNotCanceledOrCompleted())
            {
                await AsyncUtils.WaitEndOfFrame(token);
                elapsed += BeatTiming.GetDelta(mode, Time.unscaledDeltaTime, Time.deltaTime);
            }
        }

        private async UniTask WaitForInput(AsyncToken token)
        {
            var wait = new WaitForInput();
            await wait.Execute(token);
        }

        private bool IsSkipActive()
        {
            if (!Engine.Initialized) return false;
            return Engine.TryGetService<IScriptPlayer>(out var player) && player.SkipActive;
        }

        private void HideTextPrinters()
        {
            if (!Engine.TryGetService<ITextPrinterManager>(out var manager)) return;

            foreach (var printer in manager.Actors)
            {
                if (printer == null || !printer.Visible || hiddenPrinters.Contains(printer)) continue;
                hiddenPrinters.Add(printer);
                printer.Visible = false;
            }
        }

        private void RestoreTextPrinters()
        {
            for (var i = 0; i < hiddenPrinters.Count; i++)
            {
                var printer = hiddenPrinters[i];
                if (printer != null) printer.Visible = true;
            }

            hiddenPrinters.Clear();
        }

        private void CancelActive()
        {
            runVersion++;
            activeCancellation?.Cancel();
            activeCancellation?.Dispose();
            activeCancellation = null;
            screenShake?.Stop(activeShakeHandle);
            activeShakeHandle = 0;
            overlay?.Clear();
            RestoreTextPrinters();
        }
    }
}
