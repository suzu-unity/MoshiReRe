using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MoshiReRe.Exploration
{
    /// <summary>Minimal horizontal movement controller for an exploration player.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationPlayerController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float movementSpeed = 4f;
        [SerializeField] private bool clampHorizontalPosition;
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;
        [SerializeField, Tooltip("Optional Input System Value/Axis action. Keyboard arrows and A/D remain a fallback.")]
        private InputActionReference horizontalMoveAction;
        [SerializeField] private ExplorationSpriteAnimator spriteAnimator;

        private bool movementEnabled = true;

        public bool MovementEnabled => movementEnabled;
        public ExplorationSpriteAnimator SpriteAnimator => spriteAnimator;
        public event Action<bool> MovementEnabledChanged;

        private void Reset()
        {
            spriteAnimator = GetComponent<ExplorationSpriteAnimator>();
        }

        private void OnEnable()
        {
            horizontalMoveAction?.action?.Enable();
        }

        private void OnDisable()
        {
            horizontalMoveAction?.action?.Disable();
            spriteAnimator?.SetWalking(false);
        }

        private void Update()
        {
            if (!movementEnabled)
            {
                spriteAnimator?.SetWalking(false);
                return;
            }

            var horizontal = ReadHorizontalInput();
            var isWalking = !Mathf.Approximately(horizontal, 0f);
            if (isWalking)
            {
                var position = transform.position;
                position.x = ClampHorizontalPosition(
                    position.x + horizontal * movementSpeed * Time.deltaTime,
                    clampHorizontalPosition,
                    minX,
                    maxX);
                transform.position = position;
                spriteAnimator?.SetFacingRight(horizontal > 0f);
            }

            spriteAnimator?.SetWalking(isWalking);
        }

        public void SetMovementEnabled(bool value)
        {
            if (movementEnabled == value)
                return;

            movementEnabled = value;
            if (!value)
                spriteAnimator?.SetWalking(false);

            MovementEnabledChanged?.Invoke(value);
        }

        public static float ClampHorizontalPosition(float positionX, bool clampEnabled, float minX, float maxX)
        {
            if (!clampEnabled)
                return positionX;

            return Mathf.Clamp(positionX, Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));
        }

        private float ReadHorizontalInput()
        {
            var action = horizontalMoveAction?.action;
            if (action != null && action.enabled)
                return Mathf.Clamp(action.ReadValue<float>(), -1f, 1f);

            var keyboard = Keyboard.current;
            if (keyboard == null)
                return 0f;

            var right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            var left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            return right == left ? 0f : right ? 1f : -1f;
        }
    }
}
