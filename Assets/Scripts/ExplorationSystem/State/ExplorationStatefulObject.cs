using UnityEngine;

namespace MoshiReRe.Exploration.State
{
    /// <summary>Captures a stable scene object's lightweight exploration state.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationStatefulObject : MonoBehaviour
    {
        [SerializeField] private string objectId;
        [SerializeField] private string mapId;
        [SerializeField] private bool interacted;
        [SerializeField, TextArea] private string localState;

        public string ObjectId => objectId;
        public string MapId => !string.IsNullOrWhiteSpace(mapId)
            ? mapId
            : GetComponentInParent<ExplorationMapStateController>(true)?.MapId;
        public bool Interacted => interacted;
        public string LocalState => localState;

        private void Awake() => ExplorationStateCoordinator.Instance.RegisterObject(this);

        // Deliberately do not unregister in OnDisable: a disabled scene object must remain saveable.
        private void OnDestroy()
        {
            if (ExplorationStateCoordinator.HasInstance)
                ExplorationStateCoordinator.Instance.UnregisterObject(this);
        }

        public void MarkInteracted() => interacted = true;

        public void SetInteracted(bool value) => interacted = value;

        public void SetLocalState(string value) => localState = value ?? string.Empty;

        /// <summary>Moves this reusable scene object into the active logical map namespace.</summary>
        public void ConfigureMapId(string value)
        {
            mapId = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        public ExplorationObjectState CaptureState()
        {
            return new ExplorationObjectState {
                objectId = objectId,
                activeSelf = gameObject.activeSelf,
                interacted = interacted,
                localState = localState ?? string.Empty
            };
        }

        public void RestoreState(ExplorationObjectState state)
        {
            if (state == null)
                return;

            interacted = state.interacted;
            localState = state.localState ?? string.Empty;
            if (gameObject.activeSelf != state.activeSelf)
                gameObject.SetActive(state.activeSelf);
        }
    }
}
