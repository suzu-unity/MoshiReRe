using System.Collections.Generic;
using Naninovel;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.CharacterMotion
{
    public readonly struct MotionPose
    {
        public readonly Vector3 Position;
        public readonly Vector3 Scale;
        public readonly Quaternion Rotation;

        public MotionPose(Vector3 position, Vector3 scale, Quaternion rotation)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
        }

        public static MotionPose From(IActor actor) => new(actor.Position, actor.Scale, actor.Rotation);
    }

    public static class CharacterMotionMath
    {
        public static List<MotionPose> BuildSequence(MotionPose origin, CharacterMotionPreset preset)
        {
            var result = new List<MotionPose>();
            if (preset == null) return result;

            var target = new MotionPose(
                origin.Position + preset.PositionOffset,
                Vector3.Scale(origin.Scale, preset.ScaleMultiplier),
                origin.Rotation * Quaternion.Euler(preset.RotationEuler));
            var cycles = preset.Loop ? preset.RepeatCount : 1;

            for (var i = 0; i < cycles; i++)
            {
                result.Add(target);
                if (preset.Loop) result.Add(origin);
            }

            if (preset.ReturnToOrigin && (result.Count == 0 || !Approximately(result[result.Count - 1], origin)))
                result.Add(origin);

            return result;
        }

        public static MotionPose Snap(MotionPose from, MotionPose to, int steps, int step)
        {
            if (steps <= 0) return to;
            var ratio = Mathf.Clamp(step, 0, steps) / (float)steps;
            return Lerp(from, to, ratio);
        }

        public static MotionPose Lerp(MotionPose from, MotionPose to, float ratio)
        {
            return new MotionPose(
                Vector3.LerpUnclamped(from.Position, to.Position, ratio),
                Vector3.LerpUnclamped(from.Scale, to.Scale, ratio),
                Quaternion.SlerpUnclamped(from.Rotation, to.Rotation, ratio));
        }

        private static bool Approximately(MotionPose left, MotionPose right)
        {
            return Vector3.Distance(left.Position, right.Position) < .0001f &&
                   Vector3.Distance(left.Scale, right.Scale) < .0001f &&
                   Quaternion.Angle(left.Rotation, right.Rotation) < .01f;
        }
    }
}
