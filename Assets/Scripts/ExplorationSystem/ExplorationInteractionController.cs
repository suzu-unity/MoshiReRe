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
        [SerializeField, Range(0.1f, 1f), Tooltip("Walk speed, relative to the player's speed, used when stepping to a conversational distance.")]
        private float approachSpeedScale = 0.7f;
        [SerializeField, Min(0f)] private float turnPause = 0.08f;

        private readonly Collider2D[] overlapResults = new Collider2D[MaxOverlapResults];
        private Coroutine approachRoutine;
        private ExplorationInteractable nearest;
        private bool eWasHeld;
        private bool spaceWasHeld;

        public ExplorationInteractable Nearest => nearest;
        public ExplorationPlayerController Player => player;
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
            if (approachRoutine != null)
            {
                StopCoroutine(approachRoutine);
                approachRoutine = null;
                if (player != null)
                    player.CancelScriptedMove();
            }
            eWasHeld = false;
            spaceWasHeld = false;
            SetNearest(null);
        }

        private void Update()
        {
            if (player != null && !player.MovementEnabled)
            {
                SetNearest(null);
                return;
            }

            if (approachRoutine != null)
                return;

            RefreshNearest();
            if (nearest != null && WasInteractionPressed())
                BeginInteraction(nearest);
        }

        private void BeginInteraction(ExplorationInteractable target)
        {
            if (player == null)
            {
                target.Interact(null);
                return;
            }

            var standX = CalculateStandX(
                player.transform.position.x,
                target.transform.position.x,
                target.StandDistance,
                player.ClampX);
            if (Mathf.Abs(standX - player.transform.position.x) < 0.02f)
            {
                player.FaceTowards(target.transform.position.x);
                target.Interact(player);
                return;
            }

            approachRoutine = StartCoroutine(ApproachThenInteract(target, standX));
        }

        private System.Collections.IEnumerator ApproachThenInteract(ExplorationInteractable target, float standX)
        {
            // Walk (not slide) to a conversational distance, then turn to face the target.
            SetNearest(null);
            player.BeginScriptedMove(standX, approachSpeedScale);
            while (player.IsScriptedMoving && target != null)
                yield return null;

            if (target != null)
                player.FaceTowards(target.transform.position.x);
            yield return new WaitForSeconds(turnPause);

            approachRoutine = null;
            if (target != null && target.IsAvailable && player.MovementEnabled)
                target.Interact(player);
        }

        /// <summary>Keeps the player on its current side of the target at no less than the stand distance.</summary>
        public static float CalculateStandX(float playerX, float targetX, float standDistance, Func<float, float> clamp = null)
        {
            if (standDistance <= 0f || Mathf.Abs(playerX - targetX) >= standDistance)
                return playerX;

            var side = playerX < targetX ? -1f : 1f;
            var preferred = targetX + side * standDistance;
            if (clamp == null)
                return preferred;

            var clamped = clamp(preferred);
            if (Mathf.Approximately(clamped, preferred))
                return preferred;

            // No room on this side: use the other side instead of pressing into the wall.
            var opposite = clamp(targetX - side * standDistance);
            return Mathf.Abs(opposite - targetX) >= Mathf.Abs(clamped - targetX) ? opposite : clamped;
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
            var keyboard = Keyboard.current;
            var eHeld = keyboard != null && keyboard.eKey.isPressed;
            var spaceHeld = keyboard != null && keyboard.spaceKey.isPressed;
            var ePressed = keyboard != null && (keyboard.eKey.wasPressedThisFrame || (eHeld && !eWasHeld));
            var spacePressed = keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || (spaceHeld && !spaceWasHeld));
            eWasHeld = eHeld;
            spaceWasHeld = spaceHeld;

            return ShouldInteract(
                action != null && action.enabled && action.WasPressedThisFrame(),
                ePressed,
                spacePressed);
        }

        /// <summary>Combines optional action input with the always-available keyboard interaction keys.</summary>
        public static bool ShouldInteract(bool actionPressed, bool ePressed, bool spacePressed)
        {
            return actionPressed || ePressed || spacePressed;
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
