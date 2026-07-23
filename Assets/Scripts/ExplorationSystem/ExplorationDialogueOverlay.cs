using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MoshiReRe.Exploration
{
    /// <summary>Small standalone dialogue fallback used when the Naninovel engine is not ready.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationDialogueOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private string defaultSpeaker = "仮置きのNPC";

        private TaskCompletionSource<bool> completion;
        private string[] lines = Array.Empty<string>();
        private int lineIndex;
        private int openedFrame;

        public bool Visible => completion != null;

        private void Awake()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void Update()
        {
            if (!Visible || Time.frameCount == openedFrame)
                return;

            var keyboard = Keyboard.current;
            if (keyboard != null &&
                (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame ||
                 keyboard.enterKey.wasPressedThisFrame))
                Advance();
        }

        private void OnDisable()
        {
            if (completion != null)
                Close();
        }

        public Task PlayAsync(string speaker, string[] dialogueLines)
        {
            if (completion != null)
                Close();

            lines = dialogueLines == null || dialogueLines.Length == 0
                ? new[] { "これは探索ADV用のダミー会話です。" }
                : dialogueLines;
            lineIndex = 0;
            openedFrame = Time.frameCount;
            completion = new TaskCompletionSource<bool>();

            if (speakerText != null)
                speakerText.text = string.IsNullOrWhiteSpace(speaker) ? defaultSpeaker : speaker;
            if (panelRoot != null)
                panelRoot.SetActive(true);
            RefreshLine();
            return completion.Task;
        }

        public void Advance()
        {
            if (!Visible)
                return;

            if (ShouldCloseAfterAdvance(lineIndex, lines.Length))
            {
                Close();
                return;
            }

            lineIndex++;
            RefreshLine();
        }

        public static bool ShouldCloseAfterAdvance(int currentLineIndex, int lineCount)
        {
            return lineCount <= 0 || currentLineIndex >= lineCount - 1;
        }

        private void RefreshLine()
        {
            if (bodyText != null)
                bodyText.text = lines[Mathf.Clamp(lineIndex, 0, lines.Length - 1)];
        }

        private void Close()
        {
            var pending = completion;
            completion = null;

            if (panelRoot != null)
                panelRoot.SetActive(false);

            pending?.TrySetResult(true);
        }
    }
}
