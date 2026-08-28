using Naninovel;
using Naninovel.UI;
using UnityEngine;

namespace MoshiReRe.Exploration
{
    /// <summary>Keeps Naninovel's title UI out of the exploration-only scene.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationNaninovelUiGuard : MonoBehaviour
    {
        private const int InitializationRaceGuardFrames = 3;

        private int hideFramesRemaining;

        private void OnEnable()
        {
            if (Engine.Initialized)
                ScheduleTitleHide();
            else
                Engine.OnInitializationFinished += ScheduleTitleHide;
        }

        private void OnDisable()
        {
            Engine.OnInitializationFinished -= ScheduleTitleHide;
        }

        private void LateUpdate()
        {
            if (!Engine.Initialized)
                return;

            // Loading an interaction script can restore Naninovel's background actor after
            // the scene-start race guard has elapsed. Exploration owns its world background
            // with a SpriteRenderer, so keep novel backgrounds suppressed for the full scene.
            HideNaninovelBackgrounds();

            if (hideFramesRemaining > 0)
            {
                Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
                hideFramesRemaining--;
            }
        }

        private void ScheduleTitleHide()
        {
            Engine.OnInitializationFinished -= ScheduleTitleHide;
            hideFramesRemaining = InitializationRaceGuardFrames;
        }

        private static void HideNaninovelBackgrounds()
        {
            var backgroundManager = Engine.GetService<IBackgroundManager>();
            if (backgroundManager == null)
                return;

            foreach (var background in backgroundManager.Actors)
                if (background.Visible)
                    background.Visible = false;
        }
    }
}
