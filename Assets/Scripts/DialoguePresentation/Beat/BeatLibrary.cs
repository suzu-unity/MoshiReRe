using System.Collections.Generic;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.Beat
{
    [CreateAssetMenu(menuName = "MoshiReRe/Dialogue/Beat Library", fileName = "BeatLibrary")]
    public sealed class BeatLibrary : ScriptableObject
    {
        [Tooltip("Beat type names are matched case-insensitively. Empty entries are ignored safely.")]
        [SerializeField] private List<BeatPreset> presets = new();

        public IReadOnlyList<BeatPreset> Presets => presets;

        public BeatPreset Find(string type)
        {
            var normalized = BeatTypeUtility.Normalize(type);
            if (string.IsNullOrEmpty(normalized) || presets == null) return null;

            for (var i = 0; i < presets.Count; i++)
            {
                var preset = presets[i];
                if (preset != null && BeatTypeUtility.Normalize(preset.Type) == normalized)
                    return preset;
            }

            return null;
        }

        public void ReplacePresets(IEnumerable<BeatPreset> values)
        {
            presets = values == null ? new List<BeatPreset>() : new List<BeatPreset>(values);
        }
    }
}
