using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.CharacterMotion
{
    [CreateAssetMenu(menuName = "MoshiReRe/Dialogue Presentation/Character Motion Library")]
    public sealed class CharacterMotionLibrary : ScriptableObject
    {
        public const string DefaultResourcePath = "DialoguePresentation/CharacterMotion/DefaultCharacterMotionLibrary";

        [SerializeField] private List<CharacterMotionPreset> presets = new();

        public IReadOnlyList<CharacterMotionPreset> Presets => presets;

        public CharacterMotionPreset Find(string name)
        {
            var normalized = CharacterMotionTypeUtility.Normalize(name);
            if (string.IsNullOrEmpty(normalized) || presets == null) return null;

            for (var i = 0; i < presets.Count; i++)
            {
                var preset = presets[i];
                if (preset != null && CharacterMotionTypeUtility.Normalize(preset.MotionName) == normalized)
                    return preset;
            }

            return null;
        }

        public void ReplacePresets(IEnumerable<CharacterMotionPreset> values)
        {
            presets = values == null ? new List<CharacterMotionPreset>() : new List<CharacterMotionPreset>(values);
        }
    }

    public static class CharacterMotionTypeUtility
    {
        public static string Normalize(string value) => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }
}
