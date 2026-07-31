#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Naninovel;
using UnityEngine;

namespace MoshiReRe.NaninovelDebug
{
    /// <summary>Temporarily skips all Naninovel text while either Ctrl key is held in development builds.</summary>
    public sealed class NaninovelCtrlSkipDebug : MonoBehaviour
    {
        private readonly CtrlSkipHoldState holdState = new CtrlSkipHoldState();

        private IScriptPlayer controlledPlayer;
        private bool originalSkipActive;
        private PlayerSkipMode originalSkipMode;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            var gameObject = new GameObject(nameof(NaninovelCtrlSkipDebug));
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<NaninovelCtrlSkipDebug>();
        }

        private void Update()
        {
            var leftCtrlHeld = Input.GetKey(KeyCode.LeftControl);
            var rightCtrlHeld = Input.GetKey(KeyCode.RightControl);
            var transition = holdState.Update(leftCtrlHeld, rightCtrlHeld);
            if (transition == CtrlSkipHoldState.Transition.None)
            {
                // Ctrl can be held before Naninovel finishes booting; start as soon as its player exists.
                if ((leftCtrlHeld || rightCtrlHeld) && controlledPlayer == null)
                    BeginSkip();
                return;
            }

            if (transition == CtrlSkipHoldState.Transition.Began)
                BeginSkip();
            else
                RestoreSkip();
        }

        private void OnDisable()
        {
            if (holdState.Release() == CtrlSkipHoldState.Transition.Ended)
                RestoreSkip();
        }

        private void BeginSkip()
        {
            if (!Engine.Initialized || !Engine.TryGetService<IScriptPlayer>(out var player))
                return;

            controlledPlayer = player;
            originalSkipActive = player.SkipActive;
            originalSkipMode = player.SkipMode;
            player.SkipMode = PlayerSkipMode.Everything;
            player.SetSkipEnabled(true);
        }

        private void RestoreSkip()
        {
            if (controlledPlayer == null || !Engine.Initialized)
            {
                controlledPlayer = null;
                return;
            }

            try
            {
                controlledPlayer.SkipMode = originalSkipMode;
                controlledPlayer.SetSkipEnabled(originalSkipActive);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[{nameof(NaninovelCtrlSkipDebug)}] Could not restore skip state: {exception.Message}");
            }
            finally
            {
                controlledPlayer = null;
            }
        }
    }
}
#endif
