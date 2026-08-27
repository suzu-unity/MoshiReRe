using System;
using System.Globalization;
using Naninovel;
using MoshiReRe.Exploration.State;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MoshiReRe.Exploration
{
    /// <summary>Runs a Naninovel script and pauses the interacting player's movement until it completes.</summary>
    public class NaninovelDialogueInteractable : ExplorationInteractable
    {
        public const string NovelHostSceneName = "CommonUIHub";

        [SerializeField, Tooltip("Naninovel script path, for example Scenario/Exploration/Shopkeeper.")]
        private string naninovelScriptPath;
        [SerializeField, Tooltip("Optional label inside the script. Use one map-level .nani file for many interactables instead of one file per object.")]
        private string naninovelScriptLabel;
        [SerializeField, Min(0f)] private float initializationTimeout = 3f;
        [SerializeField] private string textPrinterId = "Dialogue";
        [SerializeField] private int textPrinterSortingOrder = 300;
        [SerializeField] private int choiceHandlerSortingOrder = 310;
        [SerializeField] private ExplorationDialogueOverlay fallbackOverlay;
        [Header("Exploration ADV portraits")]
        [SerializeField] private ExplorationDialoguePortraits dialoguePortraits;
        [SerializeField] private bool showNpcPortrait = true;
        [SerializeField, Tooltip("Optional direct portrait override. Uses the player's current sliced sprite when empty.")]
        private Sprite protagonistPortrait;
        [SerializeField, Tooltip("Optional direct portrait override. Uses this object's current sliced sprite when empty.")]
        private Sprite npcPortrait;
        [SerializeField, Tooltip("Optional protagonist expression ID resolved from the presenter or local variants.")]
        private string protagonistPortraitVariant;
        [SerializeField, Tooltip("Optional NPC expression ID resolved from the presenter or local variants.")]
        private string npcPortraitVariant;
        [SerializeField, Tooltip("Expression sprites available only while this interaction is active.")]
        private ExplorationPortraitVariant[] portraitVariants = Array.Empty<ExplorationPortraitVariant>();
        [SerializeField] private string fallbackSpeaker = "仮置きのNPC";
        [SerializeField, TextArea] private string[] fallbackLines;
        [SerializeField] private ExplorationSpriteAnimator outfitAnimator;
        [SerializeField] private bool requireOutfit;
        [SerializeField] private ExplorationOutfit requiredOutfit = ExplorationOutfit.Wardrobe;
        [SerializeField] private string unavailableNaninovelScriptPath;
        [SerializeField] private string nextNaninovelScriptPath;
        [SerializeField, Tooltip("Optional label to play after loading the next Naninovel script.")]
        private string nextNaninovelLabel;
        [SerializeField, Tooltip("Optional Naninovel custom variable required to use this exit.")]
        private string requiredVariableName;
        [SerializeField, Tooltip("Required custom variable value. Supports boolean, number, or exact string comparison.")]
        private string requiredVariableValue;
        [SerializeField, Tooltip("Legacy serialized option. Exploration now exits only when the dialogue executes @returnToNovel.")]
        private bool completeExplorationOnDialogueEnd;
        [SerializeField, Tooltip("Legacy fallback scene for @returnToNovel when its scene parameter is empty. Defaults to CommonUIHub.")]
        private string nextUnitySceneName;

        private bool dialoguePlaying;
        private bool continuePulseActive;
        private bool submitPulseActive;
        private bool toggleUiWasEnabled;
        private bool toggleUiSuppressed;
        private int dialogueOpenedFrame;
        private ExplorationDialoguePortraits activePortraitPresenter;
        private int portraitPresentationId;

        public event Action DialogueStarted;
        public event Action DialogueFinished;

        /// <summary>
        /// Re-targets a reusable interaction without rebuilding the scene. This is used by
        /// authored map variants which share the same exploration geometry.
        /// </summary>
        public void ConfigureScenario(string scriptPath, string label)
        {
            naninovelScriptPath = scriptPath ?? string.Empty;
            naninovelScriptLabel = label ?? string.Empty;
        }

        /// <summary>Configures the lightweight fallback shown when Naninovel is unavailable.</summary>
        public void ConfigureFallback(string speaker, params string[] lines)
        {
            fallbackSpeaker = string.IsNullOrWhiteSpace(speaker) ? "ReRe" : speaker;
            fallbackLines = lines ?? Array.Empty<string>();
        }

        /// <summary>Chooses shared portrait variants for this interaction.</summary>
        public void ConfigurePortraits(bool showNpc, string protagonistVariant, string npcVariant)
        {
            showNpcPortrait = showNpc;
            protagonistPortraitVariant = protagonistVariant ?? string.Empty;
            npcPortraitVariant = npcVariant ?? string.Empty;
        }

        private void Update()
        {
            if (!dialoguePlaying || (continuePulseActive || submitPulseActive) || !Engine.Initialized)
                return;

            var keyboard = Keyboard.current;
            if (keyboard == null ||
                !ShouldForwardDialogueInput(
                    dialogueOpenedFrame,
                    Time.frameCount,
                    keyboard.eKey.wasPressedThisFrame,
                    keyboard.spaceKey.wasPressedThisFrame))
                return;

            if (CountPendingChoices() > 0)
                PulseSubmitInputAsync();
            else
                PulseContinueInputAsync();
        }

        protected override void OnInteract(ExplorationPlayerController player)
        {
            if (dialoguePlaying) return;

            var animator = outfitAnimator != null ? outfitAnimator : player?.SpriteAnimator;
            var meetsOutfitRequirement = ShouldUseRequiredOutfit(
                animator != null ? animator.Outfit : ExplorationOutfit.Default,
                requireOutfit, requiredOutfit);
            var meetsVariableRequirement = DoesRequiredVariableMatch(
                requiredVariableName,
                requiredVariableValue,
                GetCustomVariableValue(requiredVariableName));
            var meetsRequirement = meetsOutfitRequirement && meetsVariableRequirement;
            var scriptPath = meetsRequirement || string.IsNullOrWhiteSpace(unavailableNaninovelScriptPath)
                ? naninovelScriptPath : unavailableNaninovelScriptPath;
            var nextPath = meetsRequirement ? nextNaninovelScriptPath : string.Empty;
            var nextLabel = meetsRequirement ? nextNaninovelLabel : string.Empty;
            if (!string.IsNullOrWhiteSpace(scriptPath))
                PlayDialogueAsync(player, scriptPath, nextPath, nextLabel);
        }

        private async void PlayDialogueAsync(
            ExplorationPlayerController player,
            string scriptPath,
            string nextScriptPath,
            string nextLabel)
        {
            var coordinator = ExplorationStateCoordinator.Instance;
            // Requests are scoped to exactly one dialogue. An ordinary @stop must leave the
            // player on the exploration map, even when legacy Next fields are serialized.
            coordinator.ClearReturnToNovelRequest();
            dialoguePlaying = true;
            dialogueOpenedFrame = Time.frameCount;
            var restoreMovement = player != null && player.MovementEnabled;
            var transitioningToNovel = false;
            player?.SetMovementEnabled(false);
            SuppressToggleUiInput();
            BeginPortraitPresentation(player);
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

                    await PrepareChoiceHandlerAsync();
                    var scriptPlayer = Engine.GetService<IScriptPlayer>();
                    if (scriptPlayer == null)
                        throw new InvalidOperationException("Naninovel script player is unavailable.");

                    if (string.IsNullOrWhiteSpace(naninovelScriptLabel))
                        await scriptPlayer.LoadAndPlay(scriptPath);
                    else
                        await scriptPlayer.LoadAndPlayAtLabel(scriptPath, naninovelScriptLabel);
                    await ReapplyTextPrinterPresentationAsync(scriptPlayer);
                    while (ShouldWaitForDialogueCompletion(
                               scriptPlayer.Playing,
                               scriptPlayer.WaitingForInput,
                               coordinator.ReturnToNovelRequested ? 0 : CountPendingChoices(),
                               !coordinator.ReturnToNovelRequested && IsTextPrinterVisible()))
                        await AsyncUtils.WaitEndOfFrame();

                    if (coordinator.TryConsumeReturnToNovelRequest(out var returnRequest))
                    {
                        var target = ResolveReturnTarget(
                            returnRequest,
                            nextUnitySceneName,
                            nextScriptPath,
                            nextLabel);

                        // Exploration forces the Dialogue printer into an overlay canvas and runs
                        // in its own Unity scene. Discard that actor and unload the whole scene so
                        // the novel host recreates the printer and camera at their authored values.
                        ClearChoiceHandlers();
                        Engine.GetService<ITextPrinterManager>()?.RemoveActor(textPrinterId);
                        transitioningToNovel = true;
                        FinishDialogueBeforeSceneUnload();

                        var loadOperation = SceneManager.LoadSceneAsync(
                            target.SceneName, LoadSceneMode.Single);
                        if (loadOperation == null)
                            throw new InvalidOperationException(
                                $"Could not start loading exploration return scene '{target.SceneName}'.");

                        while (!loadOperation.isDone)
                            await AsyncUtils.WaitEndOfFrame();

                        coordinator.CompleteSession();
                        if (!string.IsNullOrWhiteSpace(target.ScriptPath))
                        {
                            if (string.IsNullOrWhiteSpace(target.Label))
                                await scriptPlayer.LoadAndPlay(target.ScriptPath);
                            else
                                await scriptPlayer.LoadAndPlayAtLabel(target.ScriptPath, target.Label);
                        }
                    }
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
                if (transitioningToNovel)
                {
                    // The Single scene load destroys this component and the captured player.
                    // Do not touch either Unity object after the transition has started.
                    Debug.LogException(exception);
                }
                else
                {
                    Debug.LogException(exception, this);
                    await PlayFallbackAsync();
                }
            }
            finally
            {
                if (!transitioningToNovel)
                {
                    coordinator.ClearReturnToNovelRequest();
                    ReleaseContinueInput();
                    RestoreToggleUiInput();
                    EndPortraitPresentation();
                    if (restoreMovement)
                        player?.SetMovementEnabled(true);

                    dialoguePlaying = false;
                    DialogueFinished?.Invoke();
                }
            }
        }

        private void FinishDialogueBeforeSceneUnload()
        {
            ReleaseContinueInput();
            RestoreToggleUiInput();
            EndPortraitPresentation();
            dialoguePlaying = false;
            DialogueFinished?.Invoke();
        }

        public static bool ShouldForwardContinueInput(
            int openedFrame,
            int currentFrame,
            bool ePressed,
            bool spacePressed)
        {
            return currentFrame > openedFrame && (ePressed || spacePressed);
        }

        public static bool ShouldForwardDialogueInput(
            int openedFrame,
            int currentFrame,
            bool ePressed,
            bool spacePressed)
        {
            return ShouldForwardContinueInput(openedFrame, currentFrame, ePressed, spacePressed);
        }

        public static bool ShouldUseRequiredOutfit(
            ExplorationOutfit currentOutfit,
            bool requiresOutfit,
            ExplorationOutfit requiredOutfit)
        {
            return !requiresOutfit || currentOutfit == requiredOutfit;
        }

        /// <summary>Checks an optional custom-variable requirement using bool, numeric, or exact string semantics.</summary>
        public static bool DoesRequiredVariableMatch(
            string requiredVariableName,
            string requiredVariableValue,
            string currentVariableValue)
        {
            return string.IsNullOrWhiteSpace(requiredVariableName) ||
                   DoCustomVariableValuesMatch(currentVariableValue, requiredVariableValue);
        }

        /// <summary>Compares custom-variable string representations without depending on Naninovel services.</summary>
        public static bool DoCustomVariableValuesMatch(string currentValue, string requiredValue)
        {
            var normalizedCurrent = currentValue?.Trim() ?? string.Empty;
            var normalizedRequired = requiredValue?.Trim() ?? string.Empty;

            if (bool.TryParse(normalizedCurrent, out var currentBool) &&
                bool.TryParse(normalizedRequired, out var requiredBool))
                return currentBool == requiredBool;

            if (decimal.TryParse(normalizedCurrent, NumberStyles.Number, CultureInfo.InvariantCulture, out var currentNumber) &&
                decimal.TryParse(normalizedRequired, NumberStyles.Number, CultureInfo.InvariantCulture, out var requiredNumber))
                return currentNumber == requiredNumber;

            return string.Equals(normalizedCurrent, normalizedRequired, StringComparison.Ordinal);
        }

        /// <summary>Determines whether exploration must use its local dialogue window instead of a Naninovel printer.</summary>
        public static bool ShouldUseFallbackWhenPrinterUnavailable(bool engineInitialized, bool printerPrepared)
        {
            return !engineInitialized || !printerPrepared;
        }

        public static bool ShouldTransitionToNovel(string nextScriptPath)
        {
            // Retained for callers that inspect legacy Next configuration. Runtime exit
            // decisions are made exclusively from ExplorationReturnRequest.
            return !string.IsNullOrWhiteSpace(nextScriptPath);
        }

        public static bool ShouldTransitionToNovel(ExplorationReturnRequest request) => request.Requested;

        /// <summary>
        /// Resolves each explicit command value before legacy Inspector defaults. Session return
        /// values are intentionally excluded so @enterExploration cannot override this branch.
        /// </summary>
        public static ExplorationNovelTarget ResolveReturnTarget(
            ExplorationReturnRequest request,
            string inspectorSceneName,
            string inspectorScriptPath,
            string inspectorLabel)
        {
            return new ExplorationNovelTarget(
                FirstNotEmpty(request.SceneName, inspectorSceneName, NovelHostSceneName),
                FirstNotEmpty(request.ScriptPath, inspectorScriptPath),
                FirstNotEmpty(request.Label, inspectorLabel));
        }

        private static string GetCustomVariableValue(string variableName)
        {
            if (string.IsNullOrWhiteSpace(variableName) || !Engine.Initialized)
                return null;

            var variables = Engine.GetService<ICustomVariableManager>();
            return variables != null && variables.VariableExists(variableName)
                ? variables.GetVariableValue(variableName).ToString()
                : null;
        }

        private static string FirstNotEmpty(params string[] values)
        {
            for (var i = 0; i < values.Length; i++)
                if (!string.IsNullOrWhiteSpace(values[i]))
                    return values[i];
            return string.Empty;
        }

        public static bool ShouldWaitForDialogueCompletion(bool scriptPlaying, int pendingChoiceCount)
        {
            return scriptPlaying || pendingChoiceCount > 0;
        }

        public static bool ShouldWaitForDialogueCompletion(
            bool scriptPlaying,
            bool waitingForInput,
            int pendingChoiceCount,
            bool printerVisible)
        {
            return scriptPlaying || waitingForInput || pendingChoiceCount > 0 || printerVisible;
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

        private async void PulseSubmitInputAsync()
        {
            var inputManager = Engine.GetService<IInputManager>();
            var submitInput = inputManager?.GetSubmit() ?? inputManager?.GetContinue();
            if (submitInput == null)
                return;

            submitPulseActive = true;
            submitInput.Activate(1f);
            try
            {
                await AsyncUtils.WaitEndOfFrame();
            }
            finally
            {
                submitInput.Activate(0f);
                submitPulseActive = false;
            }
        }

        private bool IsTextPrinterVisible()
        {
            var manager = Engine.GetService<ITextPrinterManager>();
            return manager != null && manager.ActorExists(textPrinterId) &&
                   manager.GetActor(textPrinterId)?.Visible == true;
        }

        private void SuppressToggleUiInput()
        {
            if (toggleUiSuppressed || !Engine.Initialized)
                return;

            var toggleUi = Engine.GetService<IInputManager>()?.GetToggleUI();
            if (toggleUi == null)
                return;

            toggleUiWasEnabled = toggleUi.Enabled;
            toggleUi.Enabled = false;
            toggleUiSuppressed = true;
        }

        private void RestoreToggleUiInput()
        {
            if (!toggleUiSuppressed || !Engine.Initialized)
                return;

            var toggleUi = Engine.GetService<IInputManager>()?.GetToggleUI();
            if (toggleUi != null)
                toggleUi.Enabled = toggleUiWasEnabled;

            toggleUiSuppressed = false;
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
            if (!uiPrinter.PrinterPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException(
                    $"Naninovel text printer '{textPrinterId}' is under an inactive UI hierarchy.");
            NormalizePrinterContentDepth(uiPrinter.PrinterPanel.Content);

            // UITextPrinter initializes hidden. Show it on the same frame as script playback.
            uiPrinter.Visible = true;
        }

        private async UniTask PrepareChoiceHandlerAsync()
        {
            var manager = Engine.GetService<IChoiceHandlerManager>();
            if (manager == null)
                throw new InvalidOperationException("Naninovel choice handler manager is unavailable.");

            var handler = await manager.GetOrAddActor(manager.Configuration.DefaultHandlerId);
            if (handler is not UIChoiceHandler uiHandler || uiHandler.GameObject == null)
                throw new InvalidOperationException("Naninovel default choice handler could not create its UI panel.");

            ConfigureChoiceHandlerCanvas(uiHandler.GameObject.transform, choiceHandlerSortingOrder);
        }

        private static int CountPendingChoices()
        {
            var manager = Engine.GetService<IChoiceHandlerManager>();
            if (manager == null)
                return 0;

            var count = 0;
            foreach (var handler in manager.Actors)
                count += handler.Choices.Count;
            return count;
        }

        private static void ClearChoiceHandlers()
        {
            Engine.GetService<IChoiceHandlerManager>()?.RemoveAllActors();
        }

        private async UniTask PlayFallbackAsync()
        {
            if (fallbackOverlay != null)
                await fallbackOverlay.PlayAsync(fallbackSpeaker, fallbackLines);
        }

        private void BeginPortraitPresentation(ExplorationPlayerController player)
        {
            activePortraitPresenter = dialoguePortraits != null
                ? dialoguePortraits
                : fallbackOverlay != null
                    ? fallbackOverlay.GetComponent<ExplorationDialoguePortraits>()
                    : FindFirstObjectByType<ExplorationDialoguePortraits>(FindObjectsInactive.Include);
            if (activePortraitPresenter == null)
                return;

            var playerSprite = protagonistPortrait != null
                ? protagonistPortrait
                : FindCurrentSprite(player);
            var interactionSprite = showNpcPortrait
                ? npcPortrait != null ? npcPortrait : FindCurrentSprite(this)
                : null;
            portraitPresentationId = activePortraitPresenter.BeginPresentation(
                playerSprite,
                interactionSprite,
                protagonistPortraitVariant,
                npcPortraitVariant,
                portraitVariants);
        }

        private void EndPortraitPresentation()
        {
            if (activePortraitPresenter != null)
                activePortraitPresenter.EndPresentation(portraitPresentationId);
            activePortraitPresenter = null;
            portraitPresentationId = 0;
        }

        private static Sprite FindCurrentSprite(Component source)
        {
            if (source == null)
                return null;
            var renderers = source.GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
                if (renderers[i].sprite != null)
                    return renderers[i].sprite;
            return null;
        }

        private async UniTask ReapplyTextPrinterPresentationAsync(IScriptPlayer scriptPlayer)
        {
            // @printer/@showPrinter can restore the prefab's original camera mode after the
            // initial setup. Apply exploration presentation after those commands have started.
            await AsyncUtils.WaitEndOfFrame();
            if (scriptPlayer.Playing)
                await PrepareTextPrinterAsync();
        }

        /// <summary>Places the printer above exploration UI using screen-space overlay coordinates.</summary>
        public static Canvas ConfigurePrinterCanvas(Component printerPanel, int sortingOrder)
        {
            if (printerPanel == null)
                throw new ArgumentNullException(nameof(printerPanel));

            // Dialogue.prefab stores the Canvas on its root while UITextPrinterPanel is nested below it.
            var canvas = printerPanel.GetComponentInParent<Canvas>(true);
            if (canvas == null)
                throw new InvalidOperationException("Naninovel text printer has no parent Canvas.");

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            canvas.sortingOrder = sortingOrder;
            canvas.transform.localScale = Vector3.one;
            return canvas;
        }

        /// <summary>Places exploration choices above the local dialogue printer.</summary>
        public static Canvas ConfigureChoiceHandlerCanvas(Component choicePanel, int sortingOrder)
        {
            if (choicePanel == null)
                throw new ArgumentNullException(nameof(choicePanel));

            var canvas = choicePanel.GetComponentInParent<Canvas>(true);
            if (canvas == null)
                throw new InvalidOperationException("Naninovel choice handler has no parent Canvas.");

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            canvas.sortingOrder = sortingOrder;
            return canvas;
        }

        /// <summary>Removes camera-space depth left on printer content when rendering as an overlay.</summary>
        public static void NormalizePrinterContentDepth(Transform content)
        {
            if (content == null)
                return;

            var position = content.localPosition;
            position.z = 0f;
            content.localPosition = position;
        }

        private void ReleaseContinueInput()
        {
            if (!Engine.Initialized)
                return;

            var inputManager = Engine.GetService<IInputManager>();
            if (continuePulseActive)
                inputManager?.GetContinue()?.Activate(0f);
            if (submitPulseActive)
                (inputManager?.GetSubmit() ?? inputManager?.GetContinue())?.Activate(0f);
            continuePulseActive = false;
            submitPulseActive = false;
        }
    }

    public readonly struct ExplorationNovelTarget
    {
        public string SceneName { get; }
        public string ScriptPath { get; }
        public string Label { get; }

        public ExplorationNovelTarget(string sceneName, string scriptPath, string label)
        {
            SceneName = sceneName;
            ScriptPath = scriptPath;
            Label = label;
        }
    }
}
