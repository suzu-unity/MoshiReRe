using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.TypingBlip
{
    /// <summary>
    /// Author-aware settings for dialogue typing blips.
    /// Leave Author Id empty on one entry to use it as the profile fallback.
    /// </summary>
    [CreateAssetMenu(menuName = "MoshiReRe/Dialogue/Typing Blip Profile", fileName = "TypingBlipProfile")]
    public sealed class TypingBlipProfile : ScriptableObject
    {
        [Tooltip("Entries are matched by author ID. An empty Author Id is the fallback entry.")]
        [SerializeField] private List<TypingBlipProfileEntry> entries = new();

        public IReadOnlyList<TypingBlipProfileEntry> Entries => entries;

        public TypingBlipProfileEntry FindEntry(string authorId)
        {
            if (entries == null || entries.Count == 0) return null;

            if (!string.IsNullOrWhiteSpace(authorId))
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (entry != null && string.Equals(entry.AuthorId, authorId, StringComparison.OrdinalIgnoreCase))
                        return entry;
                }
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != null && string.IsNullOrWhiteSpace(entry.AuthorId))
                    return entry;
            }

            return null;
        }

        public void ReplaceEntries(IEnumerable<TypingBlipProfileEntry> values)
        {
            entries = values == null ? new List<TypingBlipProfileEntry>() : new List<TypingBlipProfileEntry>(values);
        }
    }

    [Serializable]
    public sealed class TypingBlipProfileEntry
    {
        [Tooltip("Naninovel character actor ID. Leave empty to use this entry as the fallback.")]
        [SerializeField] private string authorId;
        [Tooltip("Short clip played for each blip. It is safe to leave this empty until audio is available.")]
        [SerializeField] private AudioClip clip;
        [Tooltip("Base playback pitch.")]
        [SerializeField, Min(0.01f)] private float pitch = 1f;
        [Tooltip("Random pitch offset applied in both directions.")]
        [SerializeField, Min(0f)] private float pitchRandomness = 0.04f;
        [Tooltip("Per-blip volume passed to AudioSource.PlayOneShot.")]
        [SerializeField, Range(0f, 1f)] private float volume = 0.55f;
        [Tooltip("Minimum unscaled seconds between blips.")]
        [SerializeField, Min(0f)] private float minimumInterval = 0.045f;
        [Tooltip("Number of speakable characters consumed before a blip is played.")]
        [SerializeField, Min(1)] private int charactersPerBlip = 1;

        public string AuthorId => authorId;
        public AudioClip Clip => clip;
        public float Pitch => Mathf.Max(0.01f, pitch);
        public float PitchRandomness => Mathf.Max(0f, pitchRandomness);
        public float Volume => Mathf.Clamp01(volume);
        public float MinimumInterval => Mathf.Max(0f, minimumInterval);
        public int CharactersPerBlip => Mathf.Max(1, charactersPerBlip);

        public TypingBlipProfileEntry() { }

        public TypingBlipProfileEntry(string authorId, AudioClip clip, float pitch, float pitchRandomness,
            float volume, float minimumInterval, int charactersPerBlip)
        {
            this.authorId = authorId;
            this.clip = clip;
            this.pitch = pitch;
            this.pitchRandomness = pitchRandomness;
            this.volume = volume;
            this.minimumInterval = minimumInterval;
            this.charactersPerBlip = charactersPerBlip;
        }
    }
}
