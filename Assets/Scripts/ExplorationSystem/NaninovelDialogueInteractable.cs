using System;
using Naninovel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MoshiReRe.Exploration
{
    /// <summary>Runs a Naninovel script and pauses the interacting player's movement until it completes.</summary>
    public sealed class NaninovelDialogueInteractable : ExplorationInteractable
    {
        [SerializeField, Tooltip("Naninovel script path, for example Scenario/Exploration/Shopkeeper.")]
        private string naninovelScriptPath;
        [SerializeField, Min(0f)] private float initializationTimeout = 3f;
        [SerializeField] private string textPrinterId = "Dialogue";
        [SerializeField] private int textPrinterSortingOrder = 300;
        [SerializeField] private ExplorationDialogueOverlay fallbackOverlay;
        [SerializeField] private string fallbackSpeaker = "仮置きのNPC";
        [SerializeField, TextArea] private string[] fallbackLines;

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
            if (!dialoguePlaying && !string.IsNullOrWhiteSpace(naninovelScriptPath))
                PlayDialogueAsync(player);
        }

        private async void PlayDialogueAsync(ExplorationPlayerController player)
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
                    await fallbackOverlay.PlayAsync(fallbackSpeaker, fallbackLines);
                    return;
                }

                var initializationStarted = Time.realtimeSinceStartup;
                while (!Engine.Initialized &&
                       Time.realtimeSinceStartup - initializationStarted < initializationTimeout)
                    await AsyncUtils.WaitEndOfFrame();

                if (Engine.Initialized)
                {
                    await PrepareTextPrinterAsync();
                    var scriptPlayer = Engine.GetService<IScriptPlayer>();
                    if (scriptPlayer == null)
                        throw new InvalidOperationException("Naninovel script player is unavailable.");

                    await scriptPlayer.LoadAndPlay(naninovelScriptPath);
                    while (scriptPlayer.Playing)
                        await AsyncUtils.WaitEndOfFrame();
                }
                else if (fallbackOverlay != null)
                {
                    await fallbackOverlay.PlayAsync(fallbackSpeaker, fallbackLines);
                }
                else
                {
                    throw new InvalidOperationException("Naninovel did not initialize and no fallback dialogue overlay is assigned.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
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

        private async UniTask PrepareTextPrinterAsync()
        {
            var printerManager = Engine.GetService<ITextPrinterManager>();
            if (printerManager == null)
                throw new InvalidOperationException("Naninovel text printer manager is unavailable.");

            var printer = await printerManager.GetOrAddActor(textPrinterId);
            if (printer is not UITextPrinter uiPrinter || uiPrinter.PrinterPanel == null)
                return;

            var canvas = uiPrinter.PrinterPanel.GetComponent<Canvas>();
            if (canvas == null)
                return;

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            canvas.overrideSorting = true;
            canvas.sortingOrder = textPrinterSortingOrder;
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
