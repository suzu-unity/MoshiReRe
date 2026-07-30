using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Databases/Inventory Database")]
public class InventoryDatabase : ScriptableObject
{
    public List<InventoryItem> items = new List<InventoryItem>();

    private static readonly HashSet<InventoryItem> acquiredItems = new HashSet<InventoryItem>();

    public static event Action<InventoryItem> ItemAcquired;

    public IReadOnlyList<InventoryItem> GetAll() => items;

    public IReadOnlyList<InventoryItem> GetAcquired()
    {
        var result = new List<InventoryItem>();
        if (items == null) return result;

        foreach (var item in items)
            if (item != null && acquiredItems.Contains(item)) result.Add(item);
        return result;
    }

    public bool HasAcquired(InventoryItem item) => item != null && acquiredItems.Contains(item);

    public bool Acquire(InventoryItem item)
    {
        if (item == null || items == null || !items.Contains(item) || !acquiredItems.Add(item)) return false;
        ItemAcquired?.Invoke(item);
        return true;
    }

    public static void ClearAcquired() => acquiredItems.Clear();
}
