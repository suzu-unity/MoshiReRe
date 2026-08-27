using UnityEngine;
using MoshiReRe.Exploration.State;

namespace MoshiReRe.Exploration
{
    public interface IExplorationInteractable
    {
        string PromptText { get; }
        bool IsAvailable { get; }
        void Interact(ExplorationPlayerController player);
    }

    /// <summary>Base component for objects that can be selected by the exploration interaction controller.</summary>
    public abstract class ExplorationInteractable : MonoBehaviour, IExplorationInteractable
    {
        [SerializeField] private string promptText = "調べる";
        [SerializeField] private bool interactable = true;

        public string PromptText => promptText;
        public bool IsAvailable => interactable && isActiveAndEnabled;

        /// <summary>Updates the prompt for reusable authored map variants.</summary>
        public void ConfigurePrompt(string value)
        {
            promptText = string.IsNullOrWhiteSpace(value) ? "調べる" : value;
        }

        public void Interact(ExplorationPlayerController player)
        {
            if (IsAvailable)
            {
                GetComponent<ExplorationStatefulObject>()?.MarkInteracted();
                OnInteract(player);
            }
        }

        protected abstract void OnInteract(ExplorationPlayerController player);
    }
}
