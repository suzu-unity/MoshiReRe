using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MoshiReRe.Exploration
{
    /// <summary>Finds the closest enabled interactable and exposes it through a small event API.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationInteractionController : MonoBehaviour
    {
        private const int MaxOverlapResults = 32;

        [SerializeField] private ExplorationPlayerController player;
        [SerializeField, Min(0f)] private float interactionRadius = 1.25f;
        [SerializeField] private LayerMask interactableLayers = ~0;
        [SerializeField, Tooltip("Optional Input System Button action. E and Space remain a fallback.")]
        private InputActionReference interactAction;

        private readonly Collider2D[] overlapResults = new Collider2D[MaxOverlapResults];
        private ExplorationInteractable nearest;

        public ExplorationInteractable Nearest => nearest;
        public event Action<ExplorationInteractable> NearestChanged;

        private void Reset()
        {
            player = GetComponent<ExplorationPlayerController>();
        }

        private void OnEnable()
        {
            interactAction?.action?.Enable();
        }

        private void OnDisable()
        {
            interactAction?.action?.Disable();
            SetNearest(null);
        }

        private void Update()
        {
            if (player != null && !player.MovementEnabled)
            {
                SetNearest(null);
                return;
            }

            RefreshNearest();
            if (nearest != null && WasInteractionPressed())
                nearest.Interact(player);
        }

        public void RefreshNearest()
        {
            var count = Physics2D.OverlapCircleNonAlloc(transform.position, interactionRadius, overlapResults, interactableLayers);
            ExplorationInteractable closest = null;
            var closestDistanceSqr = float.PositiveInfinity;
            var origin = (Vector2)transform.position;

            for (var i = 0; i < count; i++)
            {
                var candidate = overlapResults[i] == null ? null : overlapResults[i].GetComponentInParent<ExplorationInteractable>();
                if (candidate == null || !candidate.IsAvailable)
                    continue;

                var distanceSqr = ((Vector2)candidate.transform.position - origin).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closest = candidate;
                    closestDistanceSqr = distanceSqr;
                }
            }

            SetNearest(closest);
        }

        public static int FindNearestIndex(Vector2 origin, IReadOnlyList<Vector2> candidates, float maxDistance)
        {
            if (candidates == null || maxDistance < 0f)
                return -1;

            var maxDistanceSqr = maxDistance * maxDistance;
            var nearestIndex = -1;
            var nearestDistanceSqr = maxDistanceSqr;
            for (var i = 0; i < candidates.Count; i++)
            {
                var distanceSqr = (candidates[i] - origin).sqrMagnitude;
                if (distanceSqr <= maxDistanceSqr && (nearestIndex < 0 || distanceSqr < nearestDistanceSqr))
                {
                    nearestIndex = i;
                    nearestDistanceSqr = distanceSqr;
                }
            }

            return nearestIndex;
        }

        private bool WasInteractionPressed()
        {
            var action = interactAction?.action;
            if (action != null && action.enabled)
                return action.WasPressedThisFrame();

            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame);
        }

        private void SetNearest(ExplorationInteractable value)
        {
            if (nearest == value)
                return;

            nearest = value;
            NearestChanged?.Invoke(nearest);
        }
    }
}
