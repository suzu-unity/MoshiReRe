using Naninovel;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.CharacterMotion
{
    [System.Serializable]
    public sealed class CharacterMotionPreset
    {
        [SerializeField] private string motionName;
        [SerializeField] private Vector3 positionOffset;
        [SerializeField] private Vector3 scaleMultiplier = Vector3.one;
        [SerializeField] private Vector3 rotationEuler;
        [Min(0f), SerializeField] private float duration = .25f;
        [SerializeField] private EasingType ease = EasingType.EaseOutQuad;
        [SerializeField] private bool loop;
        [Min(1), SerializeField] private int repeatCount = 1;
        [SerializeField] private bool returnToOrigin = true;
        [Min(0), SerializeField] private int steps;

        public string MotionName => motionName;
        public Vector3 PositionOffset => positionOffset;
        public Vector3 ScaleMultiplier => new(
            Mathf.Max(.001f, scaleMultiplier.x),
            Mathf.Max(.001f, scaleMultiplier.y),
            Mathf.Max(.001f, scaleMultiplier.z));
        public Vector3 RotationEuler => rotationEuler;
        public float Duration => Mathf.Max(0f, duration);
        public EasingType Ease => ease;
        public bool Loop => loop;
        public int RepeatCount => Mathf.Clamp(repeatCount, 1, 32);
        public bool ReturnToOrigin => returnToOrigin;
        public int Steps => Mathf.Clamp(steps, 0, 64);

        public CharacterMotionPreset(string name, Vector3 offset, Vector3 scale, Vector3 rotation,
            float duration, EasingType ease, bool loop, int repeatCount, bool returnToOrigin, int steps)
        {
            motionName = name;
            positionOffset = offset;
            scaleMultiplier = scale;
            rotationEuler = rotation;
            this.duration = duration;
            this.ease = ease;
            this.loop = loop;
            this.repeatCount = repeatCount;
            this.returnToOrigin = returnToOrigin;
            this.steps = steps;
        }
    }
}
