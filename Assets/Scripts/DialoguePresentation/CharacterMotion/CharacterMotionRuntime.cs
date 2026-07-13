using System;
using System.Collections.Generic;
using System.Threading;
using Naninovel;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.CharacterMotion
{
    public static class CharacterMotionRuntime
    {
        private sealed class MotionRun
        {
            public readonly CancellationTokenSource Cancellation = new();
            public readonly UniTaskCompletionSource Completion = new();
        }

        private static readonly Dictionary<string, MotionRun> ActiveRuns = new(StringComparer.Ordinal);
        private static readonly HashSet<string> WarningKeys = new(StringComparer.Ordinal);

        public static async UniTask Play(string actorId, string motionName, CharacterMotionLibrary library,
            AsyncToken token = default)
        {
            if (!Engine.Initialized) return;

            if (!Engine.TryGetService<ICharacterManager>(out var manager) || manager == null)
            {
                WarnOnce("manager", "[charMotion] Character manager is not available.");
                return;
            }

            var preset = library ? library.Find(motionName) : null;
            if (preset == null)
            {
                WarnOnce("motion:" + CharacterMotionTypeUtility.Normalize(motionName),
                    $"[charMotion] Motion '{motionName}' is not defined.");
                return;
            }

            if (string.IsNullOrWhiteSpace(actorId) || !manager.ActorExists(actorId))
            {
                WarnOnce("actor:" + actorId, $"[charMotion] Actor '{actorId}' is not present.");
                return;
            }

            ICharacterActor actor;
            try { actor = manager.GetActor(actorId); }
            catch (Exception) { actor = null; }
            if (actor == null)
            {
                WarnOnce("actor:" + actorId, $"[charMotion] Actor '{actorId}' is not present.");
                return;
            }

            await CancelExisting(actorId);
            var run = new MotionRun();
            ActiveRuns[actorId] = run;
            var removedHandler = new Action<string>(removedId =>
            {
                if (string.Equals(removedId, actorId, StringComparison.Ordinal)) run.Cancellation.Cancel();
            });
            manager.OnActorRemoved += removedHandler;
            var origin = MotionPose.From(actor);
            var restoreOnExit = preset.ReturnToOrigin;

            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                    token.CancellationToken, token.CompletionToken, run.Cancellation.Token);
                var playToken = (AsyncToken)linked.Token;
                var sequence = CharacterMotionMath.BuildSequence(origin, preset);
                var current = origin;
                var segmentDuration = sequence.Count == 0 ? 0f : preset.Duration / sequence.Count;

                for (var i = 0; i < sequence.Count; i++)
                {
                    if (!IsAvailable(manager, actorId)) break;
                    var target = sequence[i];
                    await ApplySegment(manager, actorId, actor, current, target, segmentDuration, preset, playToken);
                    current = target;
                }
            }
            catch (OperationCanceledException)
            {
                restoreOnExit = true;
                if (token.Canceled) token.ThrowIfCanceled();
            }
            catch (MissingReferenceException) { restoreOnExit = true; }
            catch (NullReferenceException) { restoreOnExit = true; }
            catch (ObjectDisposedException) { restoreOnExit = true; }
            finally
            {
                manager.OnActorRemoved -= removedHandler;
                var isCurrent = ActiveRuns.TryGetValue(actorId, out var currentRun) && ReferenceEquals(currentRun, run);
                if (isCurrent)
                {
                    ActiveRuns.Remove(actorId);
                    if (restoreOnExit && IsAvailable(manager, actorId))
                        await Restore(actor, origin);
                }

                run.Completion.TrySetResult();
                run.Cancellation.Dispose();
            }
        }

        public static void CancelAll()
        {
            foreach (var run in ActiveRuns.Values) run.Cancellation.Cancel();
        }

        private static async UniTask CancelExisting(string actorId)
        {
            if (!ActiveRuns.TryGetValue(actorId, out var run)) return;
            run.Cancellation.Cancel();
            try { await run.Completion.Task; }
            catch (OperationCanceledException) { }
        }

        private static async UniTask ApplySegment(ICharacterManager manager, string actorId, ICharacterActor actor,
            MotionPose from, MotionPose to, float duration, CharacterMotionPreset preset, AsyncToken token)
        {
            var steps = preset.Steps;
            if (steps <= 0)
            {
                await ApplyPose(manager, actorId, actor, to, new Tween(duration, preset.Ease, true, true), token);
                return;
            }

            for (var step = 1; step <= steps; step++)
            {
                var pose = CharacterMotionMath.Snap(from, to, steps, step);
                await ApplyPose(manager, actorId, actor, pose,
                    new Tween(duration / steps, preset.Ease, true, true), token);
            }
        }

        private static async UniTask ApplyPose(ICharacterManager manager, string actorId, ICharacterActor actor,
            MotionPose pose, Tween tween, AsyncToken token)
        {
            if (!IsAvailable(manager, actorId)) return;
            await UniTask.WhenAll(
                actor.ChangePosition(pose.Position, tween, token),
                actor.ChangeScale(pose.Scale, tween, token),
                actor.ChangeRotation(pose.Rotation, tween, token));
        }

        private static async UniTask Restore(ICharacterActor actor, MotionPose origin)
        {
            try
            {
                var tween = new Tween(0f, EasingType.Linear, true, true);
                await UniTask.WhenAll(
                    actor.ChangePosition(origin.Position, tween),
                    actor.ChangeScale(origin.Scale, tween),
                    actor.ChangeRotation(origin.Rotation, tween));
            }
            catch (MissingReferenceException) { }
            catch (NullReferenceException) { }
            catch (ObjectDisposedException) { }
        }

        private static bool IsAvailable(ICharacterManager manager, string actorId)
        {
            try { return manager != null && manager.ActorExists(actorId); }
            catch (Exception) { return false; }
        }

        private static void WarnOnce(string key, string message)
        {
            if (WarningKeys.Add(key)) Debug.LogWarning(message);
        }
    }
}
