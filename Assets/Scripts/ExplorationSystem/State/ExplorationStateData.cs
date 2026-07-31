using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoshiReRe.Exploration.State
{
    [Serializable]
    public sealed class ExplorationSaveState
    {
        public ExplorationFlowContext flow = new ExplorationFlowContext();
        public List<ExplorationMapState> maps = new List<ExplorationMapState>();

        public ExplorationMapState GetOrCreateMap(string mapId)
        {
            for (var i = 0; i < maps.Count; i++)
                if (maps[i].mapId == mapId)
                    return maps[i];

            var map = new ExplorationMapState { mapId = mapId ?? string.Empty };
            maps.Add(map);
            return map;
        }

        public ExplorationMapState FindMap(string mapId)
        {
            for (var i = 0; i < maps.Count; i++)
                if (maps[i].mapId == mapId)
                    return maps[i];
            return null;
        }

        public ExplorationSaveState Clone() => JsonUtility.FromJson<ExplorationSaveState>(JsonUtility.ToJson(this));
    }

    [Serializable]
    public sealed class ExplorationFlowContext
    {
        public bool active;
        public string mapId;
        public string sceneName;
        public string spawnId;
        public string returnScene;
        public string returnScript;
        public string returnLabel;

        public void Begin(string newMapId, string newSceneName, string newSpawnId,
            string newReturnScene, string newReturnScript, string newReturnLabel)
        {
            active = true;
            mapId = newMapId ?? string.Empty;
            sceneName = newSceneName ?? string.Empty;
            spawnId = newSpawnId ?? string.Empty;
            returnScene = newReturnScene ?? string.Empty;
            returnScript = newReturnScript ?? string.Empty;
            returnLabel = newReturnLabel ?? string.Empty;
        }

        public void Complete()
        {
            active = false;
            mapId = string.Empty;
            sceneName = string.Empty;
            spawnId = string.Empty;
            returnScene = string.Empty;
            returnScript = string.Empty;
            returnLabel = string.Empty;
        }
    }

    [Serializable]
    public sealed class ExplorationMapState
    {
        public string mapId;
        public string sceneName;
        public string spawnId;
        public Vector3 playerPosition;
        public ExplorationOutfit outfit;
        public List<ExplorationObjectState> objects = new List<ExplorationObjectState>();
        public List<ExplorationStringValue> locals = new List<ExplorationStringValue>();

        public void UpsertObject(ExplorationObjectState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.objectId))
                return;

            for (var i = 0; i < objects.Count; i++)
            {
                if (objects[i].objectId != state.objectId)
                    continue;

                objects[i] = state;
                return;
            }

            objects.Add(state);
        }

        public ExplorationObjectState FindObject(string objectId)
        {
            for (var i = 0; i < objects.Count; i++)
                if (objects[i].objectId == objectId)
                    return objects[i];
            return null;
        }

        public void SetLocal(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            for (var i = 0; i < locals.Count; i++)
            {
                if (locals[i].key != key)
                    continue;

                locals[i].value = value ?? string.Empty;
                return;
            }

            locals.Add(new ExplorationStringValue { key = key, value = value ?? string.Empty });
        }

        public bool TryGetLocal(string key, out string value)
        {
            for (var i = 0; i < locals.Count; i++)
            {
                if (locals[i].key != key)
                    continue;

                value = locals[i].value;
                return true;
            }

            value = null;
            return false;
        }
    }

    [Serializable]
    public sealed class ExplorationObjectState
    {
        public string objectId;
        public bool activeSelf;
        public bool interacted;
        public string localState;
    }

    [Serializable]
    public sealed class ExplorationStringValue
    {
        public string key;
        public string value;
    }
}
