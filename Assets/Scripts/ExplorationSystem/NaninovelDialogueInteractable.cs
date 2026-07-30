using System;
using Naninovel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MoshiReRe.Exploration
{
    /// <summary>Runs a Naninovel script and pauses the interacting player's movement until it completes.</summary>
    public class NaninovelDialogueInteractable : ExplorationInteractable
    {
        [SerializeField, Tooltip("Naninovel script path, for example Scenario/Exploration/Shopkeeper.")]
        private string naninovelScriptPath;
        [SerializeField, Min(0f)] private float initializationTimeout = 3f;
        [SerializeField] private string textPrinterId = "Dialogue";
        [SerializeField] private int textPrinterSortingOrder = 300;
        [SerializeField] private ExplorationDialogueOverlay fallbackOverlay;
        [SerializeField] private string fallbackSpeaker = "仮置きのNPC";
        [SerializeField, TextArea] private string[] fallbackLines;
        [SerializeField] private ExplorationSpriteAnimator outfitAnimator;
        [SerializeField] private bool requireOutfit;
        [SerializeField] private ExplorationOutfit requiredOutfit = ExplorationOutfit.Wardrobe;
        [SerializeField] private string unavailableNaninovelScriptPath;
        [SerializeField] private string nextNaninovelScriptPath;

        private bool dialoguePlaying;
        private bool continuePulseActive;
        private int dialogueOpenedFrame;

        public event Action DialogueStarted;
        public event Action DialogueFinished;

        private void Update()
        {
            if (!dialoguePlaying || continuePulseActive || !Engine.Initialized)
                return;

            var keyboard = Keyboard.current;
            if (keyboard == null ||
                !ShouldForwardContinueInput(
                    dialogueOpenedFrame,
                    Time.frameCount,
                    keyboard.eKey.wasPressedThisFrame,
                    keyboard.spaceKey.wasPressedThisFrame))
                return;

            PulseContinueInputAsync();
        }

        protected override void OnInteract(ExplorationPlayerController player)
        {
            if (dialoguePlaying) return;

            var animator = outfitAnimator != null ? outfitAnimator : player?.SpriteAnimator;
            var meetsRequirement = ShouldUseRequiredOutfit(
                animator != null ? animator.Outfit : ExplorationOutfit.Default,
                requireOutfit, requiredOutfit);
            var scriptPath = meetsRequirement || string.IsNullOrWhiteSpace(unavailableNaninovelScriptPath)
                ? naninovelScriptPath : unavailableNaninovelScriptPath;
            var nextPath = meetsRequirement ? nextNaninovelScriptPath : string.Empty;
            if (!string.IsNullOrWhiteSpace(scriptPath)) PlayDialogueAsync(player, scriptPath, nextPath);
        }

        private async void PlayDialogueAsync(ExplorationPlayerController player, string scriptPath, string nextScriptPath)
        {
            dialoguePlaying = true;
            dialogueOpenedFrame = Time.frameCount;
            var restoreMovement = player != null && player.MovementEnabled;
            player?.SetMovementEnabled(false);
            DialogueStarted?.Invoke();

            try
            {
                // The exploration scene can be launched directly while Naninovel is still booting.
                // In that case, show the novel-style fallback immediately instead of presenting
                // a locked player with no message window for the whole initialization timeout.
                if (!Engine.Initialized && fallbackOverlay != null)
                {
                    await PlayFallbackAsync();
                    return;
                }

                var initializationStarted = Time.realtimeSinceStartup;
                while (!Engine.Initialized &&
                       Time.realtimeSinceStartup - initializationStarted < initializationTimeout)
                    await AsyncUtils.WaitEndOfFrame();

                if (Engine.Initialized)
                {
                    var printerPrepared = await TryPrepareTextPrinterAsync();
                    if (ShouldUseFallbackWhenPrinterUnavailable(Engine.Initialized, printerPrepared))
                    {
                        await PlayFallbackAsync();
                        return;
                    }

                    var scriptPlayer = Engine.GetService<IScriptPlayer>();
                    if (scriptPlayer == null)
                        throw new InvalidOperationException("Naninovel script player is unavailable.");

                    await scriptPlayer.LoadAndPlay(scriptPath);
                    while (scriptPlayer.Playing)
                        await AsyncUtils.WaitEndOfFrame();

                    if (!string.IsNullOrWhiteSpace(nextScriptPath))
                        await scriptPlayer.LoadAndPlay(nextScriptPath);
                }
                else if (fallbackOverlay != null)
                {
                    await PlayFallbackAsync();
                }
                else
                {
                    throw new InvalidOperationException("Naninovel did not initialize and no fallback dialogue overlay is assigned.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                await PlayFallbackAsync();
            }
            finally
            {
                ReleaseContinueInput();
                if (restoreMovement)
                    player?.SetMovementEnabled(true);

                dialoguePlaying = false;
                DialogueFinished?.Invoke();
            }
        }

        public static bool ShouldForwardContinueInput(
            int openedFrame,
            int currentFrame,
            bool ePressed,
            bool spacePressed)
        {
            return currentFrame > openedFrame && (ePressed || spacePressed);
        }

        public static bool ShouldUseRequiredOutfit(
            ExplorationOutfit currentOutfit,
            bool requiresOutfit,
            ExplorationOutfit requiredOutfit)
        {
            return !requiresOutfit || currentOutfit == requiredOutfit;
        }

        /// <summary>Determines whether exploration must use its local dialogue window instead of a Naninovel printer.</summary>
        public static bool ShouldUseFallbackWhenPrinterUnavailable(bool engineInitialized, bool printerPrepared)
        {
            return !engineInitialized || !printerPrepared;
        }

        private async void PulseContinueInputAsync()
        {
            var continueInput = Engine.GetService<IInputManager>()?.GetContinue();
            if (continueInput == null)
                return;

            continuePulseActive = true;
            continueInput.Activate(1f);
            try
            {
                await AsyncUtils.WaitEndOfFrame();
            }
            finally
            {
                continueInput.Activate(0f);
                continuePulseActive = false;
            }
        }

        private async UniTask<bool> TryPrepareTextPrinterAsync()
        {
            try
            {
                await PrepareTextPrinterAsync();
                return true;
            }
            catch (TimeoutException exception)
            {
                Debug.LogWarning(
                    $"Naninovel dialogue printer '{textPrinterId}' did not initialize within " +
                    $"{initializationTimeout:0.##} seconds; using the exploration dialogue window instead. " +
                    exception.Message, this);
                return false;
            }
        }

        private async UniTask PrepareTextPrinterAsync()
        {
            var printerManager = Engine.GetService<ITextPrinterManager>();
            if (printerManager == null)
                throw new InvalidOperationException("Naninovel text printer manager is unavailable.");

            if (printerManager.ActorExists(textPrinterId))
            {
                var existingPrinter = printerManager.GetActor(textPrinterId) as UITextPrinter;
                if (existingPrinter?.PrinterPanel == null ||
                    existingPrinter.PrinterPanel.GetComponentInParent<Canvas>(true) == null)
                    printerManager.RemoveActor(textPrinterId);
            }

            var printer = await printerManager.GetOrAddActor(textPrinterId)
                .Timeout(TimeSpan.FromSeconds(Mathf.Max(0.01f, initializationTimeout)));
            if (printer is not UITextPrinter uiPrinter || uiPrinter.PrinterPanel == null)
                throw new InvalidOperationException($"Naninovel text printer '{textPrinterId}' could not create its UI panel.");

            ConfigurePrinterCanvas(uiPrinter.PrinterPanel, textPrinterSortingOrder);

            // UITextPrinter initializes hidden. Show it on the same frame as script playback
            // so an inactive UI hierarchy can't leave the player without a dialogue window.
            uiPrinter.Visible = true;
        }

        private async UniTask PlayFallbackAsync()
        {
            if (fallbackOverlay != null)
                await fallbackOverlay.PlayAsync(fallbackSpeaker, fallbackLines);
        }

        /// <summary>Restores a printer's UI hierarchy and makes its root canvas render above exploration UI.</summary>
        public static Canvas ConfigurePrinterCanvas(Component printerPanel, int sortingOrder)
        {
            if (printerPanel == null)
                throw new ArgumentNullException(nameof(printerPanel));

            // Dialogue.prefab stores the Canvas on its root while UITextPrinterPanel is nested below it.
            var canvas = printerPanel.GetComponentInParent<Canvas>(true);
            if (canvas == null)
                throw new InvalidOperationException("Naninovel text printer has no parent Canvas.");

            // The persistent Naninovel UI can retain an inactive parent after a scene transition.
            // Restore every parent required for the printer to enter the active hierarchy.
            for (var current = canvas.transform; current != null; current = current.parent)
                current.gameObject.SetActive(true);

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            canvas.sortingOrder = sortingOrder;
            return canvas;
        }

        private void ReleaseContinueInput()
        {
            if (!continuePulseActive || !Engine.Initialized)
                return;

            Engine.GetService<IInputManager>()?.GetContinue()?.Activate(0f);
            continuePulseActive = false;
        }
    }
}
