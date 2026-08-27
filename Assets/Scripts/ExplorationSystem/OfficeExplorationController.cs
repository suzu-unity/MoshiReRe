using Naninovel;
using MoshiReRe.Exploration.State;
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
        [SerializeField, Tooltip("Background used when this reusable scene hosts the papa-cafe demo map.")]
        private Sprite papaCafeBackground;
        [SerializeField] private string papaCafeMapId = "papa_cafe";
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
        public bool IsPapaCafeVariant { get; private set; }

        private void Awake()
        {
            active = this;
            if (arrivingNpc != null)
                arrivingNpc.position = npcStartPosition;
            SetBackground(initialBackgroundIndex);
            ApplyCurrentMapVariant();
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

        public static bool IsPapaCafeMap(string mapId, string configuredMapId = "papa_cafe")
        {
            return !string.IsNullOrWhiteSpace(mapId) &&
                   string.Equals(mapId.Trim(), configuredMapId?.Trim(), System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reuses the tested office exploration scene as the first cafe vertical slice. The
        /// production cafe can later become its own scene without changing the scenario contract.
        /// </summary>
        public void ApplyCurrentMapVariant()
        {
            var mapId = ExplorationStateCoordinator.HasInstance
                ? ExplorationStateCoordinator.Instance.FlowContext.mapId
                : string.Empty;
            IsPapaCafeVariant = IsPapaCafeMap(mapId, papaCafeMapId);
            if (!IsPapaCafeVariant)
                return;

            GetComponent<ExplorationMapStateController>()?.ConfigureMapId(papaCafeMapId);
            foreach (var stateful in FindObjectsByType<ExplorationStatefulObject>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
                if (stateful.gameObject.scene == gameObject.scene)
                    stateful.ConfigureMapId(papaCafeMapId);

            if (backgroundRenderer != null && papaCafeBackground != null)
                backgroundRenderer.sprite = papaCafeBackground;

            if (arrivingNpc != null)
                arrivingNpc.gameObject.SetActive(false);

            ConfigureCafeInteraction("OfficeDesk_01", "PapaCafeExitTable",
                "待ち合わせ席を確認する", "下調べを終えるなら、この席で待とう。", true);
            ConfigureCafeInteraction("OfficeDesk_02", "PapaCafeClue",
                "窓際の席を調べる", "ReReが気にしていた席だ。小物を確認してみよう。", false);

            for (var index = 3; index <= 5; index++)
                ConfigureCafeInteraction($"OfficeDesk_{index:00}", "PapaCafeGeneric",
                    "店内を調べる", "待ち合わせ前なら、客の導線と席の距離を見ておける。", false);
        }

        private static void ConfigureCafeInteraction(
            string objectName,
            string label,
            string prompt,
            string fallback,
            bool showNpcPortrait)
        {
            var target = GameObject.Find(objectName);
            var interaction = target != null ? target.GetComponent<NaninovelDialogueInteractable>() : null;
            if (interaction == null)
                return;

            interaction.ConfigurePrompt(prompt);
            interaction.ConfigureScenario("Scenario/PapaCafeExploration", label);
            interaction.ConfigureFallback("ReRe", fallback);
            interaction.ConfigurePortraits(showNpcPortrait, "player_default", "npc_default");
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
