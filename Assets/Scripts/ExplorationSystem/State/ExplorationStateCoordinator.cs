using System;
using System.Collections.Generic;
using Naninovel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoshiReRe.Exploration.State
{
    /// <summary>Persistent bridge between authored exploration scenes and Naninovel game saves.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationStateCoordinator : MonoBehaviour
    {
        public const string StateId = "MoshiReRe.Exploration.State";

        private static ExplorationStateCoordinator instance;

        private readonly List<ExplorationMapStateController> maps = new List<ExplorationMapStateController>();
        private readonly List<ExplorationStatefulObject> objects = new List<ExplorationStatefulObject>();
        private ExplorationSaveState savedState = new ExplorationSaveState();
        private ExplorationMapStateController activeMap;
        private IStateManager registeredStateManager;

        public static bool HasInstance => instance != null;
        public static ExplorationStateCoordinator Instance
        {
            get
            {
                EnsureInstance();
                return instance;
            }
        }

        public ExplorationMapStateController ActiveMap => activeMap;
        public ExplorationFlowContext FlowContext => savedState.flow;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateAtRuntime() => EnsureInstance();

        private static void EnsureInstance()
        {
            if (instance != null)
                return;

            instance = FindObjectOfType<ExplorationStateCoordinator>();
            if (instance != null)
                return;

            var host = new GameObject(nameof(ExplorationStateCoordinator));
            instance = host.AddComponent<ExplorationStateCoordinator>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Update() => TryRegisterNaninovelTasks();

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
            SceneManager.sceneLoaded -= HandleSceneLoaded;

            if (registeredStateManager == null)
                return;
            registeredStateManager.RemoveOnGameSerializeTask(SerializeGameState);
            registeredStateManager.RemoveOnGameDeserializeTask(DeserializeGameStateAsync);
            registeredStateManager = null;
        }

        public void RegisterMap(ExplorationMapStateController controller)
        {
            if (controller != null && !maps.Contains(controller))
                maps.Add(controller);
        }

        public void UnregisterMap(ExplorationMapStateController controller)
        {
            maps.Remove(controller);
            if (activeMap == controller)
                activeMap = null;
        }

        public void MarkMapActive(ExplorationMapStateController controller)
        {
            RegisterMap(controller);
            activeMap = controller;
        }

        public void RegisterObject(ExplorationStatefulObject statefulObject)
        {
            if (statefulObject != null && !objects.Contains(statefulObject))
                objects.Add(statefulObject);
        }

        public void UnregisterObject(ExplorationStatefulObject statefulObject) => objects.Remove(statefulObject);

        internal void GetRegisteredObjects(string mapId, List<ExplorationStatefulObject> results)
        {
            if (results == null)
                return;

            RemoveDestroyedReferences();
            for (var i = 0; i < objects.Count; i++)
            {
                var statefulObject = objects[i];
                if (statefulObject != null && statefulObject.MapId == mapId && !results.Contains(statefulObject))
                    results.Add(statefulObject);
            }
        }

        public void BeginSession(string mapId, string sceneName, string spawnId,
            string returnScene, string returnScript, string returnLabel)
        {
            savedState.flow.Begin(mapId, sceneName, spawnId, returnScene, returnScript, returnLabel);
        }

        public ExplorationFlowContext CaptureFlowContext()
        {
            Capture();
            return JsonUtility.FromJson<ExplorationFlowContext>(JsonUtility.ToJson(savedState.flow));
        }

        public void Complete() => savedState.flow.Complete();

        public void CompleteSession() => Complete();

        public void SetObjectState(string mapId, ExplorationObjectState state) =>
            savedState.GetOrCreateMap(mapId).UpsertObject(state);

        public bool TryGetObjectState(string mapId, string objectId, out ExplorationObjectState state)
        {
            state = savedState.FindMap(mapId)?.FindObject(objectId);
            return state != null;
        }

        public ExplorationSaveState Capture()
        {
            RemoveDestroyedReferences();
            for (var i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                if (map == null)
                    continue;

                var mapState = map.CaptureState();
                var index = savedState.maps.FindIndex(existing => existing.mapId == mapState.mapId);
                if (index >= 0)
                {
                    // Runtime capture owns physical scene state; map-scoped values can be
                    // authored by quests and must survive repeated captures of the same map.
                    mapState.locals = savedState.maps[index].locals;
                    savedState.maps[index] = mapState;
                }
                else
                    savedState.maps.Add(mapState);
            }

            // Direct Unity or @loadScene scene loads have no BeginSession call. Treat the registered map as active.
            if (activeMap != null && !string.IsNullOrWhiteSpace(activeMap.MapId))
            {
                var activeState = savedState.FindMap(activeMap.MapId);
                savedState.flow.Begin(activeMap.MapId, activeMap.SceneName,
                    activeState != null ? activeState.spawnId : activeMap.CurrentSpawnId,
                    savedState.flow.returnScene, savedState.flow.returnScript, savedState.flow.returnLabel);
            }

            return savedState.Clone();
        }

        public void SetLocal(string mapId, string key, string value) => savedState.GetOrCreateMap(mapId).SetLocal(key, value);

        public bool TryGetLocal(string mapId, string key, out string value)
        {
            var map = savedState.FindMap(mapId);
            if (map == null)
            {
                value = null;
                return false;
            }
            return map.TryGetLocal(key, out value);
        }

        public string GetLocal(string mapId, string key, string defaultValue = "") =>
            TryGetLocal(mapId, key, out var value) ? value : defaultValue;

        private void TryRegisterNaninovelTasks()
        {
            if (registeredStateManager != null || !Engine.Initialized ||
                !Engine.TryGetService<IStateManager>(out var stateManager))
                return;

            registeredStateManager = stateManager;
            registeredStateManager.AddOnGameSerializeTask(SerializeGameState);
            registeredStateManager.AddOnGameDeserializeTask(DeserializeGameStateAsync);
        }

        private void SerializeGameState(GameStateMap stateMap) => stateMap.SetState(Capture(), StateId);

        private async UniTask DeserializeGameStateAsync(GameStateMap stateMap)
        {
            savedState = stateMap.GetState<ExplorationSaveState>(StateId) ?? new ExplorationSaveState();
            if (!savedState.flow.active)
                return;

            var sceneName = savedState.flow.sceneName;
            if (string.IsNullOrWhiteSpace(sceneName))
                sceneName = savedState.FindMap(savedState.flow.mapId)?.sceneName;
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            if (SceneManager.GetActiveScene().name != sceneName)
            {
                var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                if (loadOperation == null)
                {
                    Debug.LogError($"[ExplorationState] Could not load saved exploration scene '{sceneName}'.", this);
                    return;
                }
                while (!loadOperation.isDone)
                    await AsyncUtils.WaitEndOfFrame();
            }

            RestoreLoadedMap(savedState.flow.mapId);
        }

        private void RestoreLoadedMap(string mapId)
        {
            RemoveDestroyedReferences();
            for (var i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                if (map == null || map.MapId != mapId)
                    continue;

                activeMap = map;
                map.RestoreState(savedState.FindMap(mapId));
                return;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var flow = savedState.flow;
            if (!flow.active || flow.sceneName != scene.name)
                return;

            RemoveDestroyedReferences();
            for (var i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                if (map == null || map.MapId != flow.mapId || map.gameObject.scene != scene)
                    continue;

                activeMap = map;
                map.RestoreState(savedState.FindMap(flow.mapId));
                map.PlaceAtSpawn(flow.spawnId);
                return;
            }
        }

        private void RemoveDestroyedReferences()
        {
            maps.RemoveAll(map => map == null);
            objects.RemoveAll(statefulObject => statefulObject == null);
        }
    }
}
