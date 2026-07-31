using UnityEngine;

namespace MoshiReRe.Exploration.State
{
    /// <summary>Stable identifier for an authored exploration entry point.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnId;

        public string SpawnId => spawnId;
    }
}
