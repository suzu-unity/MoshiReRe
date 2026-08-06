using Naninovel;
using UnityEngine;

namespace MoshiReRe.Exploration
{
    /// <summary>Controls the office's time-of-day backgrounds and the temporary NPC arrival.</summary>
    [DisallowMultipleComponent]
    public sealed class OfficeExplorationController : MonoBehaviour
    {
        private static OfficeExplorationController active;

        [SerializeField] private Transform arrivingNpc;
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private Sprite[] timeOfDayBackgrounds;
        [SerializeField, Min(0)] private int initialBackgroundIndex;
        [SerializeField, Min(0f)] private float npcArrivalDelay = 2f;
        [SerializeField, Min(0.01f)] private float npcSlideDuration = 1.2f;
        [SerializeField] private Vector3 npcStartPosition = new Vector3(8.25f, -2.65f, 0f);
        [SerializeField] private Vector3 npcArrivalPosition = new Vector3(1.5f, -2.7f, 0f);

        private bool npcArrived;
        private bool npcSliding;

        public static OfficeExplorationController Active => active;
        public bool NpcArrived => npcArrived;
        public int CurrentBackgroundIndex { get; private set; }

        private void Awake()
        {
            active = this;
            if (arrivingNpc != null)
                arrivingNpc.position = npcStartPosition;
            SetBackground(initialBackgroundIndex);
        }

        private void OnDestroy()
        {
            if (active == this)
                active = null;
        }

        public void SetBackground(int index)
        {
            if (timeOfDayBackgrounds == null || timeOfDayBackgrounds.Length == 0 || backgroundRenderer == null)
                return;

            var clamped = Mathf.Clamp(index, 0, timeOfDayBackgrounds.Length - 1);
            var sprite = timeOfDayBackgrounds[clamped];
            if (sprite == null)
                return;

            CurrentBackgroundIndex = clamped;
            backgroundRenderer.sprite = sprite;
        }

        public void ResetNpc()
        {
            npcArrived = false;
            npcSliding = false;
            if (arrivingNpc != null)
                arrivingNpc.position = npcStartPosition;
        }

        public async UniTask SlideNpcAsync()
        {
            if (npcArrived || npcSliding || arrivingNpc == null)
                return;

            npcSliding = true;
            await WaitSecondsAsync(npcArrivalDelay);

            var start = arrivingNpc.position;
            var elapsed = 0f;
            while (elapsed < npcSlideDuration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / npcSlideDuration);
                arrivingNpc.position = Vector3.Lerp(start, npcArrivalPosition, progress);
                await AsyncUtils.WaitEndOfFrame();
            }

            arrivingNpc.position = npcArrivalPosition;
            npcArrived = true;
            npcSliding = false;
        }

        private static async UniTask WaitSecondsAsync(float seconds)
        {
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                await AsyncUtils.WaitEndOfFrame();
            }
        }
    }

    [Command.CommandAlias("officeNpcSlide")]
    public sealed class OfficeNpcSlideCommand : Command
    {
        public override UniTask Execute(AsyncToken asyncToken = default)
        {
            var controller = OfficeExplorationController.Active;
            return controller == null ? UniTask.CompletedTask : controller.SlideNpcAsync();
        }
    }

    [Command.CommandAlias("officeBackground")]
    public sealed class OfficeBackgroundCommand : Command
    {
        [Command.ParameterAlias("index"), Command.RequiredParameter]
        public DecimalParameter Index;

        public override UniTask Execute(AsyncToken asyncToken = default)
        {
            OfficeExplorationController.Active?.SetBackground(Mathf.RoundToInt(Index.Value));
            return UniTask.CompletedTask;
        }
    }
}
