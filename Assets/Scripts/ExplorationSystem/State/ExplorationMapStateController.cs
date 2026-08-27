using System.Collections.Generic;
using UnityEngine;

namespace MoshiReRe.Exploration.State
{
    /// <summary>Owns capture and restoration of one authored exploration map.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationMapStateController : MonoBehaviour
    {
        [SerializeField] private string mapId;
        [SerializeField] private Transform player;
        [SerializeField] private ExplorationSpriteAnimator spriteAnimator;
        [SerializeField] private ExplorationSpawnPoint currentSpawnPoint;

        public string MapId => mapId;
        public string SceneName => gameObject.scene.name;
        public string CurrentSpawnId => currentSpawnPoint != null ? currentSpawnPoint.SpawnId : string.Empty;

        /// <summary>Assigns the logical map represented by a reusable scene before restoration.</summary>
        public void ConfigureMapId(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                mapId = value.Trim();
        }

        private void Reset()
        {
            if (player == null)
                player = transform;
            if (spriteAnimator == null && player != null)
                spriteAnimator = player.GetComponent<ExplorationSpriteAnimator>();
        }

        private void Awake() => ExplorationStateCoordinator.Instance.RegisterMap(this);

        private void OnEnable()
        {
            var coordinator = ExplorationStateCoordinator.Instance;
            coordinator.RegisterMap(this);
            coordinator.MarkMapActive(this);
        }

        private void OnDestroy()
        {
            if (ExplorationStateCoordinator.HasInstance)
                ExplorationStateCoordinator.Instance.UnregisterMap(this);
        }

        public void SetCurrentSpawnPoint(ExplorationSpawnPoint value) => currentSpawnPoint = value;

        public void SetCurrentSpawnPoint(string spawnId)
        {
            var points = GetComponentsInChildren<ExplorationSpawnPoint>(true);
            for (var i = 0; i < points.Length; i++)
            {
                if (points[i].SpawnId != spawnId)
                    continue;

                currentSpawnPoint = points[i];
                return;
            }
        }

        public bool PlaceAtSpawn(string spawnId)
        {
            if (string.IsNullOrWhiteSpace(spawnId))
                return false;

            var points = GetComponentsInChildren<ExplorationSpawnPoint>(true);
            for (var i = 0; i < points.Length; i++)
            {
                if (points[i].SpawnId != spawnId)
                    continue;

                currentSpawnPoint = points[i];
                if (player != null)
                    player.position = points[i].transform.position;
                return true;
            }

            Debug.LogWarning($"[ExplorationState] Spawn '{spawnId}' was not found on map '{mapId}'.", this);
            return false;
        }

        public ExplorationMapState CaptureState()
        {
            var state = new ExplorationMapState {
                mapId = mapId ?? string.Empty,
                sceneName = SceneName,
                spawnId = CurrentSpawnId,
                playerPosition = player != null ? player.position : Vector3.zero,
                outfit = spriteAnimator != null ? spriteAnimator.Outfit : ExplorationOutfit.Default
            };

            var statefulObjects = GetStatefulObjects();
            for (var i = 0; i < statefulObjects.Count; i++)
            {
                var statefulObject = statefulObjects[i];
                if (statefulObject.MapId != mapId || string.IsNullOrWhiteSpace(statefulObject.ObjectId))
                    continue;
                state.UpsertObject(statefulObject.CaptureState());
            }

            return state;
        }

        public void RestoreState(ExplorationMapState state)
        {
            if (state == null || state.mapId != mapId)
                return;

            if (player != null)
                player.position = state.playerPosition;
            spriteAnimator?.SetOutfit(state.outfit);
            SetCurrentSpawnPoint(state.spawnId);

            var statesById = new Dictionary<string, ExplorationObjectState>();
            for (var i = 0; i < state.objects.Count; i++)
                if (!string.IsNullOrWhiteSpace(state.objects[i].objectId))
                    statesById[state.objects[i].objectId] = state.objects[i];

            // Include inactive children so an object saved inactive can be restored on a fresh scene load.
            var statefulObjects = GetStatefulObjects();
            for (var i = 0; i < statefulObjects.Count; i++)
            {
                var statefulObject = statefulObjects[i];
                if (statefulObject.MapId != mapId ||
                    !statesById.TryGetValue(statefulObject.ObjectId, out var objectState))
                    continue;
                statefulObject.RestoreState(objectState);
            }
        }

        private List<ExplorationStatefulObject> GetStatefulObjects()
        {
            var result = new List<ExplorationStatefulObject>(GetComponentsInChildren<ExplorationStatefulObject>(true));
            ExplorationStateCoordinator.Instance.GetRegisteredObjects(mapId, result);
            return result;
        }
    }
}
