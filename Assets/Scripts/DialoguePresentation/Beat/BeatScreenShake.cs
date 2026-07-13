using UnityEngine;

namespace MoshiReRe.DialoguePresentation.Beat
{
    /// <summary>
    /// Lightweight transform shake. The latest handle owns the target and can safely supersede an older beat.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BeatScreenShake : MonoBehaviour
    {
        private Transform target;
        private Vector3 baseLocalPosition;
        private float amplitude;
        private float frequency;
        private bool unscaled;
        private bool active;
        private int handle;

        public int Play(Transform target, float amplitude, float frequency, bool unscaled)
        {
            Stop(handle);
            this.target = target;
            this.amplitude = Mathf.Max(0f, amplitude);
            this.frequency = Mathf.Max(0f, frequency);
            this.unscaled = unscaled;
            active = this.target && this.amplitude > 0f;
            if (active) baseLocalPosition = this.target.localPosition;
            handle++;
            return handle;
        }

        public void Stop(int requestedHandle)
        {
            if (requestedHandle != handle) return;
            if (active && target) target.localPosition = baseLocalPosition;
            target = null;
            active = false;
        }

        private void LateUpdate()
        {
            if (!active) return;
            if (!target)
            {
                active = false;
                return;
            }

            var time = (unscaled ? Time.unscaledTime : Time.time) * frequency;
            var offset = new Vector3(Mathf.Sin(time * 1.17f), Mathf.Cos(time * 1.41f), 0f) * amplitude;
            target.localPosition = baseLocalPosition + offset;
        }

        private void OnDisable()
        {
            if (active && target) target.localPosition = baseLocalPosition;
            target = null;
            active = false;
        }
    }
}
