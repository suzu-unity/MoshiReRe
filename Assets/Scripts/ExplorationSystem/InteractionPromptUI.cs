using TMPro;
using UnityEngine;

namespace MoshiReRe.Exploration
{
    /// <summary>Binds a TextMesh Pro prompt to the currently selected interactable.</summary>
    [DisallowMultipleComponent]
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private ExplorationInteractionController interactionController;
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private string promptFormat = "E：{0}";

        private void Reset()
        {
            promptText = GetComponentInChildren<TMP_Text>(true);
            promptRoot = promptText == null ? null : promptText.gameObject;
        }

        private void OnEnable()
        {
            if (interactionController != null)
                interactionController.NearestChanged += Refresh;

            Refresh(interactionController == null ? null : interactionController.Nearest);
        }

        private void OnDisable()
        {
            if (interactionController != null)
                interactionController.NearestChanged -= Refresh;
        }

        public void Refresh(ExplorationInteractable interactable)
        {
            var visible = interactable != null;
            if (promptRoot != null)
                promptRoot.SetActive(visible);
            else if (promptText != null)
                promptText.enabled = visible;

            if (visible && promptText != null)
                promptText.text = string.Format(promptFormat, interactable.PromptText);
        }
    }
}
