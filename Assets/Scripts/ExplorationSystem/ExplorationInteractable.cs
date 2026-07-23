using UnityEngine;

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

        public void Interact(ExplorationPlayerController player)
        {
            if (IsAvailable)
                OnInteract(player);
        }

        protected abstract void OnInteract(ExplorationPlayerController player);
    }
}
