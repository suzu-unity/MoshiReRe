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
        [SerializeField] private bool clampHorizontalPosition;
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;

        private float horizontalVelocity;

        private void LateUpdate()
        {
            if (target == null)
                return;

            var position = transform.position;
            var targetX = ClampHorizontalPosition(
                target.position.x + horizontalOffset,
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
    }
}
