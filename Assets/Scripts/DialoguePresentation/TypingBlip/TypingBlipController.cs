using Naninovel;
using Naninovel.UI;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.TypingBlip
{
    /// <summary>
    /// Plays short author-specific blips while a Naninovel text printer reveals a line.
    /// Add this component to a persistent audio object and assign a profile.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class TypingBlipController : MonoBehaviour
    {
        [Tooltip("Author-aware blip settings. Empty profile is valid and remains silent.")]
        [SerializeField] private TypingBlipProfile profile;
        [Tooltip("One-shot source used for the typing blips. The component's AudioSource is used by default.")]
        [SerializeField] private AudioSource audioSource;

        private ITextPrinterManager textPrinterManager;
        private ITextPrinterActor currentPrinter;
        private string currentText;
        private PrintedMessage currentMessage;
        private TypingBlipProfileEntry currentEntry;
        private readonly TypingBlipRevealState revealState = new();
        private readonly TypingBlipRateLimiter rateLimiter = new();
        private bool subscribed;

        // Kept in memory so a fresh checkout has an audible default without an external sound file.
        private static AudioClip generatedBlip;

        private void Awake()
        {
            if (!audioSource) audioSource = GetComponent<AudioSource>();
            if (audioSource)
            {
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }

        private void OnEnable()
        {
            Engine.OnInitializationFinished -= HandleEngineInitialized;
            Engine.OnInitializationFinished += HandleEngineInitialized;
            Engine.OnDestroyed -= HandleEngineDestroyed;
            Engine.OnDestroyed += HandleEngineDestroyed;
            if (Engine.Initialized) HandleEngineInitialized();
        }

        private void OnDisable()
        {
            Engine.OnInitializationFinished -= HandleEngineInitialized;
            Engine.OnDestroyed -= HandleEngineDestroyed;
            Unsubscribe();
            ClearCurrentLine();
        }

        private void HandleEngineDestroyed()
        {
            Unsubscribe();
            ClearCurrentLine();
        }

        private void Update()
        {
            if (currentPrinter == null) return;
            if (!Engine.Initialized || IsSkipActive() || IsBacklogVisible())
            {
                ClearCurrentLine();
                return;
            }

            ProcessRevealProgress(currentPrinter.RevealProgress);
        }

        private void HandleEngineInitialized()
        {
            if (!isActiveAndEnabled || !Engine.Initialized || subscribed) return;
            if (!Engine.TryGetService<ITextPrinterManager>(out textPrinterManager)) return;

            textPrinterManager.OnPrintStarted += HandlePrintStarted;
            textPrinterManager.OnPrintFinished += HandlePrintFinished;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (textPrinterManager != null)
            {
                textPrinterManager.OnPrintStarted -= HandlePrintStarted;
                textPrinterManager.OnPrintFinished -= HandlePrintFinished;
            }

            textPrinterManager = null;
            subscribed = false;
        }

        private void HandlePrintStarted(PrintMessageArgs args)
        {
            ClearCurrentLine();
            if (args.Printer == null || !Engine.Initialized || IsSkipActive() || IsBacklogVisible()) return;

            var metadata = textPrinterManager.Configuration.GetMetadataOrDefault(args.Printer.Id);
            if (metadata == null || !TypingBlipEligibility.CanStart(IsSkipActive(), metadata.RevealInstantly, IsBacklogVisible())) return;

            var authorId = args.Message.Author.HasValue ? args.Message.Author.Value.Id : string.Empty;
            currentPrinter = args.Printer;
            currentMessage = args.Message;
            currentText = TypingBlipTextUtility.StripRichTextTags((string)args.Message.Text);
            currentEntry = profile ? profile.FindEntry(authorId) : null;
            revealState.Reset();

            if (currentText.Length == 0) ClearCurrentLine();
        }

        private void HandlePrintFinished(PrintMessageArgs args)
        {
            if (!IsCurrentLine(args)) return;

            ProcessRevealProgress(1f);
            ClearCurrentLine();
        }

        private bool IsCurrentLine(PrintMessageArgs args)
        {
            return currentPrinter != null && ReferenceEquals(currentPrinter, args.Printer) && currentMessage.Equals(args.Message);
        }

        private void ProcessRevealProgress(float progress)
        {
            var revealedCount = TypingBlipRevealMath.GetRevealedCharacterCount(progress, currentText.Length);
            var blips = revealState.Consume(currentText, revealedCount, currentEntry?.CharactersPerBlip ?? 1);
            for (var i = 0; i < blips; i++) PlayBlip();
        }

        private void PlayBlip()
        {
            if (!audioSource || currentEntry == null) return;
            if (!rateLimiter.TryAcquire(Time.unscaledTime, currentEntry.MinimumInterval)) return;

            audioSource.pitch = Mathf.Clamp(currentEntry.Pitch + Random.Range(-currentEntry.PitchRandomness, currentEntry.PitchRandomness), 0.01f, 3f);
            audioSource.PlayOneShot(currentEntry.Clip ? currentEntry.Clip : GetGeneratedBlip(), currentEntry.Volume);
        }

        private static AudioClip GetGeneratedBlip()
        {
            if (generatedBlip) return generatedBlip;

            const int sampleRate = 22050;
            const float duration = .035f;
            const float frequency = 680f;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var time = i / (float)sampleRate;
                var envelope = 1f - i / (float)sampleCount;
                var squareWave = Mathf.Sin(2f * Mathf.PI * frequency * time) >= 0f ? 1f : -1f;
                samples[i] = squareWave * envelope * .16f;
            }

            generatedBlip = AudioClip.Create("TypingBlip_Generated", sampleCount, 1, sampleRate, false);
            generatedBlip.SetData(samples, 0);
            return generatedBlip;
        }

        private bool IsSkipActive()
        {
            return Engine.TryGetService<IScriptPlayer>(out var player) && player.SkipActive;
        }

        private bool IsBacklogVisible()
        {
            if (!Engine.TryGetService<IUIManager>(out var uiManager)) return false;
            return uiManager.GetUI<IBacklogUI>()?.Visible ?? false;
        }

        private void ClearCurrentLine()
        {
            currentPrinter = null;
            currentText = string.Empty;
            currentEntry = null;
            currentMessage = default;
            revealState.Reset();
            rateLimiter.Reset();
        }
    }
}
