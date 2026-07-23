using UnityEngine;

namespace MoshiReRe.Exploration
{
    /// <summary>Switches the player's exploration sprite set when a wardrobe is used.</summary>
    public sealed class OutfitInteractable : ExplorationInteractable
    {
        [SerializeField] private ExplorationSpriteAnimator targetAnimator;
        [SerializeField] private ExplorationOutfit outfit = ExplorationOutfit.Wardrobe;

        protected override void OnInteract(ExplorationPlayerController player)
        {
            var animator = targetAnimator != null ? targetAnimator : player?.SpriteAnimator;
            animator?.SetOutfit(outfit);
        }
    }
}
