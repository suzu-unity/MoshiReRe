using System;
using Naninovel;
using Naninovel.UI;
using UnityEditor;
using UnityEngine;

namespace MoshiReRe.Editor
{
    /// <summary>Starts a selected Naninovel script after the play-mode engine has initialized.</summary>
    public sealed class NaninovelScriptDebugWindow : EditorWindow
    {
        private const string PendingScriptPathKey = "MoshiReRe.NaninovelDebug.PendingScriptPath";

        private Script script;

        [MenuItem("Tools/MoshiReRe/Naninovel/Debug Play Script")]
        private static void Open()
        {
            var window = GetWindow<NaninovelScriptDebugWindow>("Naninovel Debug Play");
            window.script = Selection.activeObject as Script;
            window.Show();
        }

        [MenuItem("Assets/MoshiReRe/Debug Play Naninovel Script", true)]
        private static bool ValidatePlaySelectedScript() => Selection.activeObject is Script;

        [MenuItem("Assets/MoshiReRe/Debug Play Naninovel Script")]
        private static void PlaySelectedScript()
        {
            QueueScript(Selection.activeObject as Script);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Select any .nani asset, then start it after Naninovel initializes.", MessageType.Info);
            script = (Script)EditorGUILayout.ObjectField("Script", script, typeof(Script), false);

            using (new EditorGUI.DisabledScope(script == null || string.IsNullOrWhiteSpace(script.Path)))
            {
                if (GUILayout.Button(EditorApplication.isPlaying ? "Play Selected Script" : "Enter Play Mode and Play"))
                    QueueScript(script);
            }
        }

        private static void QueueScript(Script selectedScript)
        {
            if (selectedScript == null || string.IsNullOrWhiteSpace(selectedScript.Path))
            {
                Debug.LogError("[NaninovelScriptDebug] Select a valid .nani asset first.");
                return;
            }

            SessionState.SetString(PendingScriptPathKey, selectedScript.Path);
            EnsurePendingScriptIsStarted();
            if (!EditorApplication.isPlaying)
                EditorApplication.isPlaying = true;
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                EnsurePendingScriptIsStarted();
        }

        private static void EnsurePendingScriptIsStarted()
        {
            EditorApplication.update -= TryStartPendingScript;
            EditorApplication.update += TryStartPendingScript;
        }

        private static void TryStartPendingScript()
        {
            var scriptPath = SessionState.GetString(PendingScriptPathKey, string.Empty);
            if (string.IsNullOrWhiteSpace(scriptPath))
            {
                EditorApplication.update -= TryStartPendingScript;
                return;
            }

            if (!EditorApplication.isPlaying || !Engine.Initialized)
                return;

            EditorApplication.update -= TryStartPendingScript;
            SessionState.EraseString(PendingScriptPathKey);
            StartScriptAsync(scriptPath);
        }

        private static async void StartScriptAsync(string scriptPath)
        {
            try
            {
                var player = Engine.GetService<IScriptPlayer>();
                if (player == null)
                    throw new InvalidOperationException("Naninovel script player is unavailable.");

                var stateManager = Engine.GetService<IStateManager>();
                if (stateManager == null)
                    throw new InvalidOperationException("Naninovel state manager is unavailable.");

                await stateManager.ResetState();
                RestoreInputProcessing(Engine.GetService<IInputManager>());
                await player.LoadAndPlay(scriptPath);
                Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
                Debug.Log($"[NaninovelScriptDebug] Playing '{scriptPath}'.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        internal static void RestoreInputProcessing(IInputManager inputManager)
        {
            if (inputManager == null)
                throw new InvalidOperationException("Naninovel input manager is unavailable.");

            // A direct debug launch has no preceding script to restore input state.
            // Ctrl skip drives the player directly, but Continue requires input sampling.
            inputManager.ProcessInput = true;
        }
    }
}
