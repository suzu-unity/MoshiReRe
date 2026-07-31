using Naninovel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoshiReRe.Exploration.State
{
    /// <summary>Starts a reusable exploration map while recording where it should return.</summary>
    [Command.CommandAlias("enterExploration")]
    public sealed class EnterExplorationCommand : Command
    {
        [Command.ParameterAlias("scene")]
        public StringParameter Scene;

        [Command.ParameterAlias("map")]
        public StringParameter Map;

        [Command.ParameterAlias("spawn")]
        public StringParameter Spawn;

        [Command.ParameterAlias("returnScene")]
        public StringParameter ReturnScene;

        [Command.ParameterAlias("returnScript")]
        public StringParameter ReturnScript;

        [Command.ParameterAlias("returnLabel")]
        public StringParameter ReturnLabel;

        public override async UniTask Execute(AsyncToken asyncToken = default)
        {
            var sceneName = Scene?.Value;
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[enterExploration] 'scene' is required.");
                return;
            }

            ExplorationStateCoordinator.Instance.BeginSession(
                string.IsNullOrWhiteSpace(Map?.Value) ? sceneName : Map.Value,
                sceneName,
                Spawn?.Value,
                string.IsNullOrWhiteSpace(ReturnScene?.Value)
                    ? NaninovelDialogueInteractable.NovelHostSceneName
                    : ReturnScene.Value,
                ReturnScript?.Value,
                ReturnLabel?.Value);

            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError($"[enterExploration] Could not load scene '{sceneName}'.");
                ExplorationStateCoordinator.Instance.CompleteSession();
                return;
            }

            while (!operation.isDone)
                await AsyncUtils.WaitEndOfFrame();
        }
    }

    /// <summary>Writes a map-scoped string value which is included in Naninovel saves.</summary>
    [Command.CommandAlias("setExplorationLocal")]
    public sealed class SetExplorationLocalCommand : Command
    {
        [Command.ParameterAlias("key")]
        public StringParameter Key;

        [Command.ParameterAlias("value")]
        public StringParameter Value;

        [Command.ParameterAlias("map")]
        public StringParameter Map;

        public override UniTask Execute(AsyncToken asyncToken = default)
        {
            var coordinator = ExplorationStateCoordinator.Instance;
            var mapId = string.IsNullOrWhiteSpace(Map?.Value)
                ? coordinator.FlowContext.mapId
                : Map.Value;
            if (string.IsNullOrWhiteSpace(mapId) || string.IsNullOrWhiteSpace(Key?.Value))
                Debug.LogWarning("[setExplorationLocal] A map/session and key are required.");
            else
                coordinator.SetLocal(mapId, Key.Value, Value?.Value);
            return UniTask.CompletedTask;
        }
    }
}
