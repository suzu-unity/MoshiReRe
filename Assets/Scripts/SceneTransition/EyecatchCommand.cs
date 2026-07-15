using System;
using System.Collections.Generic;
using System.Globalization;
using Naninovel;
using Naninovel.Commands;
using UnityEngine;

namespace MoshiReRe.SceneTransition
{
    [Doc(
        "Shows a random manual-quote eyecatch, waits, then fades to the configured end background.",
        null,
        @"; Use the shared pool configured at Resources/MoshiReRe/eyecatch_pool.txt.
@eyecatch",
        @"; Override timing for a major chapter break.
@eyecatch hold:4 fade:0.6",
        @"; Use a one-off image pool.
@eyecatch images:title&eyecatch/Load01,title&eyecatch/Load02")]
    [Command.CommandAlias("eyecatch")]
    public sealed class EyecatchCommand : Command
    {
        private const string PoolResourcePath = "MoshiReRe/eyecatch_pool";
        private const string DefaultEndAppearance = "title&eyecatch/kuro";

        private static readonly string[] FallbackPool =
        {
            "title&eyecatch/Load01",
            "title&eyecatch/Load02"
        };

        private static readonly object BagLock = new();
        private static readonly System.Random Random = new();
        private static readonly List<string> ShuffleBag = new();
        private static string shuffleBagKey;
        private static string lastAppearance;

        [Doc("Optional comma-separated Naninovel background appearance IDs. Uses the shared pool when omitted.")]
        [Command.ParameterAlias("images")]
        public StringListParameter Images;

        [Doc("Duration in seconds to keep the eyecatch fully visible.")]
        [Command.ParameterAlias("hold"), Command.ParameterDefaultValue("3")]
        public DecimalParameter HoldDuration = 3f;

        [Doc("Fade duration in seconds used to show the eyecatch and its end background.")]
        [Command.ParameterAlias("fade"), Command.ParameterDefaultValue("0.45")]
        public DecimalParameter FadeDuration = 0.45f;

        [Doc("Background appearance shown after the eyecatch. Specify 'none' to leave the eyecatch visible.")]
        [Command.ParameterAlias("end"), Command.ParameterDefaultValue(DefaultEndAppearance)]
        public StringParameter EndAppearance = DefaultEndAppearance;

        [Doc("Whether to hide Naninovel UI while the eyecatch is displayed.")]
        [Command.ParameterAlias("hideUI"), Command.ParameterDefaultValue("true")]
        public BooleanParameter HideUI = true;

        public override async UniTask Execute(AsyncToken asyncToken = default)
        {
            // Avoid requesting services from scene previews or other non-initialized contexts.
            if (!Engine.Initialized) return;

            var pool = ResolvePool();
            if (pool.Count == 0)
            {
                Warn($"No eyecatch appearances are configured. Check Resources/{PoolResourcePath}.txt or the 'images' parameter.");
                return;
            }

            var selectedAppearance = DrawFromShuffleBag(pool);
            var uiManager = HideUI ? Engine.GetServiceOrErr<IUIManager>() : null;
            var uiHidden = false;

            try
            {
                if (uiManager != null)
                {
                    // This changes only the global render state; each UI keeps its own visibility state.
                    uiManager.SetUIVisibleWithToggle(false, false);
                    uiHidden = true;
                }

                var fade = Mathf.Max(0f, FadeDuration);
                await ChangeBackground(selectedAppearance, fade, asyncToken);

                var hold = Mathf.Max(0f, HoldDuration);
                if (hold > 0f)
                    await WaitForSeconds(hold, asyncToken);

                var endAppearance = EndAppearance?.Value;
                if (!string.IsNullOrWhiteSpace(endAppearance) &&
                    !endAppearance.Equals("none", StringComparison.OrdinalIgnoreCase))
                    await ChangeBackground(endAppearance.Trim(), fade, asyncToken);
            }
            finally
            {
                // Always restore rendering when playback is interrupted or the command is canceled.
                if (uiHidden && Engine.Initialized)
                    uiManager.SetUIVisibleWithToggle(true, true);
            }
        }

        private List<string> ResolvePool()
        {
            var pool = new List<string>();
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Assigned(Images))
            {
                foreach (var image in Images)
                    AddIfValid(pool, unique, image);
                return pool;
            }

            var poolAsset = Resources.Load<TextAsset>(PoolResourcePath);
            if (poolAsset)
            {
                var lines = poolAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("#") || trimmed.StartsWith(";")) continue;
                    AddIfValid(pool, unique, trimmed);
                }
            }

            if (pool.Count == 0)
            {
                foreach (var image in FallbackPool)
                    AddIfValid(pool, unique, image);
            }

            return pool;
        }

        private static void AddIfValid(List<string> pool, HashSet<string> unique, string image)
        {
            var value = image?.Trim();
            if (string.IsNullOrEmpty(value) || !unique.Add(value)) return;
            pool.Add(value);
        }

        private static string DrawFromShuffleBag(IReadOnlyList<string> pool)
        {
            var poolKey = string.Join("\n", pool);

            lock (BagLock)
            {
                if (!string.Equals(shuffleBagKey, poolKey, StringComparison.Ordinal) || ShuffleBag.Count == 0)
                    RefillShuffleBag(pool, poolKey);

                var index = ShuffleBag.Count - 1;
                var selected = ShuffleBag[index];
                ShuffleBag.RemoveAt(index);
                lastAppearance = selected;
                return selected;
            }
        }

        private static void RefillShuffleBag(IReadOnlyList<string> pool, string poolKey)
        {
            ShuffleBag.Clear();
            shuffleBagKey = poolKey;

            for (var i = 0; i < pool.Count; i++)
                ShuffleBag.Add(pool[i]);

            for (var i = ShuffleBag.Count - 1; i > 0; i--)
            {
                var swapIndex = Random.Next(i + 1);
                (ShuffleBag[i], ShuffleBag[swapIndex]) = (ShuffleBag[swapIndex], ShuffleBag[i]);
            }

            // Items are drawn from the end. Keep the first draw of a new cycle from matching the last cycle.
            var drawIndex = ShuffleBag.Count - 1;
            if (drawIndex <= 0 || !string.Equals(ShuffleBag[drawIndex], lastAppearance, StringComparison.OrdinalIgnoreCase))
                return;

            for (var i = 0; i < drawIndex; i++)
            {
                if (string.Equals(ShuffleBag[i], lastAppearance, StringComparison.OrdinalIgnoreCase)) continue;
                (ShuffleBag[i], ShuffleBag[drawIndex]) = (ShuffleBag[drawIndex], ShuffleBag[i]);
                break;
            }
        }

        private async UniTask ChangeBackground(string appearance, float duration, AsyncToken asyncToken)
        {
            var command = new ModifyBackground
            {
                AppearanceAndTransition = new NamedString(appearance, null),
                Duration = duration,
                Wait = true,
                PlaybackSpot = PlaybackSpot,
                Indent = Indent
            };
            await command.Execute(asyncToken);
        }

        private async UniTask WaitForSeconds(float duration, AsyncToken asyncToken)
        {
            var command = new Wait
            {
                WaitMode = duration.ToString(CultureInfo.InvariantCulture),
                PlaybackSpot = PlaybackSpot,
                Indent = Indent
            };
            await command.Execute(asyncToken);
        }
    }
}
