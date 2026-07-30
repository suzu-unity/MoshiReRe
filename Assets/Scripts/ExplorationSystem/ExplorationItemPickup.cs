using System.Collections.Generic;
using Naninovel;
using UnityEngine;

namespace MoshiReRe.Exploration
{
    /// <summary>Connects a Naninovel choice to a scene item and the runtime inventory.</summary>
    public sealed class ExplorationItemPickup : NaninovelDialogueInteractable
    {
        private static readonly List<ExplorationItemPickup> activePickups = new List<ExplorationItemPickup>();

        [SerializeField] private InventoryDatabase inventoryDatabase;
        [SerializeField] private InventoryItem item;

        /// <summary>Assigns the persistent item references used by the generated prototype scene.</summary>
        public void Configure(InventoryDatabase database, InventoryItem inventoryItem)
        {
            inventoryDatabase = database;
            item = inventoryItem;
        }

        private void OnEnable()
        {
            if (!activePickups.Contains(this)) activePickups.Add(this);
        }

        private void OnDisable() => activePickups.Remove(this);

        public static bool AcquireById(string itemId)
        {
            foreach (var pickup in activePickups)
            {
                if (pickup == null || pickup.item == null || pickup.item.id != itemId) continue;
                if (pickup.inventoryDatabase == null || !pickup.inventoryDatabase.Acquire(pickup.item)) return false;
                pickup.gameObject.SetActive(false);
                return true;
            }
            return false;
        }
    }

    [Command.CommandAlias("acquireExplorationItem")]
    public sealed class AcquireExplorationItemCommand : Command
    {
        [Command.ParameterAlias("id")]
        public StringParameter ItemId;

        public override UniTask Execute(AsyncToken asyncToken = default)
        {
            if (!ExplorationItemPickup.AcquireById(ItemId?.Value))
                Debug.LogWarning($"[acquireExplorationItem] No active pickup matched '{ItemId?.Value}'.");
            return UniTask.CompletedTask;
        }
    }
}
