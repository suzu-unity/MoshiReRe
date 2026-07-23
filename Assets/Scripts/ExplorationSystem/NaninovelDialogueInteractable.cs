using System;
using Naninovel;
using UnityEngine;

namespace MoshiReRe.Exploration
{
    /// <summary>Runs a Naninovel script and pauses the interacting player's movement until it completes.</summary>
    public sealed class NaninovelDialogueInteractable : ExplorationInteractable
    {
        [SerializeField, Tooltip("Naninovel script path, for example Scenario/Exploration/Shopkeeper.")]
        private string naninovelScriptPath;
        [SerializeField, Min(0f)] private float initializationTimeout = 3f;
        [SerializeField] private ExplorationDialogueOverlay fallbackOverlay;
        [SerializeField] private string fallbackSpeaker = "仮置きのNPC";
        [SerializeField, TextArea] private string[] fallbackLines;

        private bool dialoguePlaying;

        public event Action DialogueStarted;
        public event Action DialogueFinished;

        protected override void OnInteract(ExplorationPlayerController player)
        {
            if (!dialoguePlaying && !string.IsNullOrWhiteSpace(naninovelScriptPath))
                PlayDialogueAsync(player);
        }

        private async void PlayDialogueAsync(ExplorationPlayerController player)
        {
            dialoguePlaying = true;
            var restoreMovement = player != null && player.MovementEnabled;
            player?.SetMovementEnabled(false);
            DialogueStarted?.Invoke();

            try
            {
                var initializationStarted = Time.realtimeSinceStartup;
                while (!Engine.Initialized &&
                       Time.realtimeSinceStartup - initializationStarted < initializationTimeout)
                    await AsyncUtils.WaitEndOfFrame();

                if (Engine.Initialized)
                {
                    var scriptPlayer = Engine.GetService<IScriptPlayer>();
                    if (scriptPlayer == null)
                        throw new InvalidOperationException("Naninovel script player is unavailable.");

                    await scriptPlayer.LoadAndPlay(naninovelScriptPath);
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
                if (restoreMovement)
                    player?.SetMovementEnabled(true);

                dialoguePlaying = false;
                DialogueFinished?.Invoke();
            }
        }
    }
}
