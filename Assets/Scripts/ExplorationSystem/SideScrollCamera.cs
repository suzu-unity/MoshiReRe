using UnityEngine;

namespace MoshiReRe.Exploration
{
    /// <summary>Follows a target horizontally while retaining the camera's authored Y and Z values.</summary>
    [DisallowMultipleComponent]
    public sealed class SideScrollCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float horizontalOffset;
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;
        [SerializeField, Min(0f), Tooltip("Keeps the player within this horizontal camera dead zone before the background starts scrolling.")]
        private float horizontalDeadZone = 0.75f;
        [SerializeField] private bool clampHorizontalPosition;
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;

        private float horizontalVelocity;

        private void LateUpdate()
        {
            if (target == null)
                return;

            var position = transform.position;
            var followX = CalculateFollowX(position.x, target.position.x + horizontalOffset, horizontalDeadZone);
            var targetX = ClampHorizontalPosition(
                followX,
                clampHorizontalPosition,
                minX,
                maxX);
            position.x = smoothTime <= 0f
                ? targetX
                : Mathf.SmoothDamp(position.x, targetX, ref horizontalVelocity, smoothTime);
            transform.position = position;
        }

        public static float ClampHorizontalPosition(float positionX, bool clampEnabled, float minX, float maxX)
        {
            if (!clampEnabled)
                return positionX;

            return Mathf.Clamp(positionX, Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));
        }

        public static float CalculateFollowX(float cameraX, float targetX, float deadZone)
        {
            var clampedDeadZone = Mathf.Max(0f, deadZone);
            var delta = targetX - cameraX;
            if (Mathf.Abs(delta) <= clampedDeadZone)
                return cameraX;

            return targetX - Mathf.Sign(delta) * clampedDeadZone;
        }
    }
}
