using Naninovel;
using UnityEngine;

namespace MoshiReRe.Exploration
{
    /// <summary>Loops a footstep SFX through Naninovel's audio (respecting the SFX volume) while the player walks.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationFootsteps : MonoBehaviour
    {
        [SerializeField] private ExplorationPlayerController player;
        [SerializeField, Tooltip("Naninovel SFX resource path.")]
        private string sfxPath = "SFX/革靴で歩く";
        [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
        [SerializeField, Min(0f)] private float fadeInSeconds = 0.06f;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 0.18f;
        [SerializeField, Min(0f), Tooltip("Walking must last this long before steps start, so taps stay silent.")]
        private float startDelay = 0.08f;
        [SerializeField, Min(0f), Tooltip("Idle time before steps stop, so brief direction changes do not restart the loop.")]
        private float stopDelay = 0.12f;
        [SerializeField, Tooltip("Steps play only while one of these outfits is worn (the default outfit is barefoot at home).")]
        private ExplorationOutfit[] outfitsWithFootsteps = { ExplorationOutfit.Wardrobe };

        private bool playing;
        private float walkingTime;
        private float idleTime;

        private void Reset() => player = GetComponent<ExplorationPlayerController>();

        private void OnDisable() => SetPlaying(false);

        private void Update()
        {
            var walking = IsWalking();
            if (walking)
            {
                walkingTime += Time.deltaTime;
                idleTime = 0f;
            }
            else
            {
                idleTime += Time.deltaTime;
                walkingTime = 0f;
            }

            if (!playing && walking && walkingTime >= startDelay)
                SetPlaying(true);
            else if (playing && !walking && idleTime >= stopDelay)
                SetPlaying(false);
        }

        private bool IsWalking()
        {
            if (player == null || !OutfitHasFootsteps())
                return false;

            return player.IsScriptedMoving ||
                   (player.MovementEnabled && Mathf.Abs(player.VelocityX) > player.MovementSpeed * 0.3f);
        }

        private bool OutfitHasFootsteps()
        {
            var outfit = player.SpriteAnimator != null ? player.SpriteAnimator.Outfit : ExplorationOutfit.Default;
            if (outfitsWithFootsteps == null)
                return false;
            for (var i = 0; i < outfitsWithFootsteps.Length; i++)
                if (outfitsWithFootsteps[i] == outfit)
                    return true;
            return false;
        }

        private void SetPlaying(bool value)
        {
            if (playing == value)
                return;

            playing = value;
            if (string.IsNullOrWhiteSpace(sfxPath) || !Engine.Initialized ||
                !Engine.TryGetService<IAudioManager>(out var audio) || audio == null)
                return;

            if (value)
                audio.PlaySfx(sfxPath, volume, fadeInSeconds, true).Forget();
            else
                audio.StopSfx(sfxPath, fadeOutSeconds).Forget();
        }
    }
}
