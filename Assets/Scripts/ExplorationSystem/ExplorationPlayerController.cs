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
        [SerializeField, Min(0f), Tooltip("Units per second squared while input is held. 0 = reach full speed instantly.")]
        private float acceleration = 22f;
        [SerializeField, Min(0f), Tooltip("Units per second squared after input is released. 0 = stop instantly.")]
        private float deceleration = 30f;
        [SerializeField, Min(0f), Tooltip("Extra deceleration multiplier while reversing direction.")]
        private float turnBrakeMultiplier = 1.6f;
        [SerializeField] private bool clampHorizontalPosition;
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;
        [SerializeField, Tooltip("Optional Input System Value/Axis action. Keyboard arrows and A/D remain a fallback.")]
        private InputActionReference horizontalMoveAction;
        [SerializeField] private ExplorationSpriteAnimator spriteAnimator;

        private bool movementEnabled = true;
        private float velocityX;
        private bool scriptedMoveActive;
        private float scriptedMoveTargetX;
        private float scriptedMoveSpeedScale = 0.7f;

        public bool MovementEnabled => movementEnabled;
        public ExplorationSpriteAnimator SpriteAnimator => spriteAnimator;
        /// <summary>Current horizontal velocity in world units per second.</summary>
        public float VelocityX => velocityX;
        public float MovementSpeed => movementSpeed;
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
            scriptedMoveActive = false;
            velocityX = 0f;
            spriteAnimator?.SetWalking(false);
        }

        private void Update()
        {
            if (scriptedMoveActive)
            {
                UpdateScriptedMove();
                return;
            }

            if (!movementEnabled)
            {
                velocityX = 0f;
                spriteAnimator?.SetWalking(false);
                return;
            }

            var horizontal = ReadHorizontalInput();
            if (!Mathf.Approximately(horizontal, 0f))
                spriteAnimator?.SetFacingRight(horizontal > 0f);

            velocityX = StepVelocity(
                velocityX,
                horizontal * movementSpeed,
                acceleration,
                deceleration,
                turnBrakeMultiplier,
                Time.deltaTime);

            var isWalking = Mathf.Abs(velocityX) > 0.01f;
            if (isWalking)
            {
                var position = transform.position;
                var nextX = ClampHorizontalPosition(
                    position.x + velocityX * Time.deltaTime,
                    clampHorizontalPosition,
                    minX,
                    maxX);
                if (!Mathf.Approximately(nextX, position.x + velocityX * Time.deltaTime))
                    velocityX = 0f;
                position.x = nextX;
                transform.position = position;
            }

            spriteAnimator?.SetWalkSpeed(movementSpeed > 0f ? Mathf.Abs(velocityX) / movementSpeed : 0f);
            spriteAnimator?.SetWalking(isWalking);
        }

        public void SetMovementEnabled(bool value)
        {
            if (movementEnabled == value)
                return;

            movementEnabled = value;
            if (!value)
            {
                velocityX = 0f;
                spriteAnimator?.SetWalking(false);
            }

            MovementEnabledChanged?.Invoke(value);
        }

        /// <summary>True while the player is walking to a position chosen by code (input is ignored).</summary>
        public bool IsScriptedMoving => scriptedMoveActive;

        /// <summary>Walks to a world X with the normal walk animation, ignoring player input until arrival.</summary>
        public void BeginScriptedMove(float worldX, float speedScale = 0.7f)
        {
            scriptedMoveTargetX = ClampX(worldX);
            scriptedMoveSpeedScale = Mathf.Clamp(speedScale, 0.1f, 1.5f);
            scriptedMoveActive = true;
            velocityX = 0f;
        }

        public void CancelScriptedMove()
        {
            if (!scriptedMoveActive)
                return;

            scriptedMoveActive = false;
            spriteAnimator?.SetWalking(false);
        }

        private void UpdateScriptedMove()
        {
            var position = transform.position;
            var delta = scriptedMoveTargetX - position.x;
            if (Mathf.Abs(delta) <= 0.001f)
            {
                CancelScriptedMove();
                return;
            }

            spriteAnimator?.SetFacingRight(delta > 0f);
            spriteAnimator?.SetWalkSpeed(scriptedMoveSpeedScale);
            spriteAnimator?.SetWalking(true);
            position.x = Mathf.MoveTowards(
                position.x,
                scriptedMoveTargetX,
                Mathf.Max(0.5f, movementSpeed * scriptedMoveSpeedScale) * Time.deltaTime);
            transform.position = position;
        }

        /// <summary>Applies this player's configured horizontal bounds to a world X position.</summary>
        public float ClampX(float worldX) => ClampHorizontalPosition(worldX, clampHorizontalPosition, minX, maxX);

        /// <summary>Turns the character toward a world X position without moving it.</summary>
        public void FaceTowards(float worldX)
        {
            var delta = worldX - transform.position.x;
            if (!Mathf.Approximately(delta, 0f))
                spriteAnimator?.SetFacingRight(delta > 0f);
        }

        /// <summary>Moves one velocity step toward the target with separate accelerate, brake and turn rates.</summary>
        public static float StepVelocity(
            float current,
            float target,
            float acceleration,
            float deceleration,
            float turnBrakeMultiplier,
            float deltaTime)
        {
            var reversing = !Mathf.Approximately(current, 0f) &&
                            !Mathf.Approximately(target, 0f) &&
                            Mathf.Sign(current) != Mathf.Sign(target);
            var speedingUp = !reversing && Mathf.Abs(target) > Mathf.Abs(current);
            var rate = speedingUp
                ? acceleration
                : deceleration * (reversing ? Mathf.Max(1f, turnBrakeMultiplier) : 1f);
            if (rate <= 0f)
                return target;

            return Mathf.MoveTowards(current, target, rate * Mathf.Max(0f, deltaTime));
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
