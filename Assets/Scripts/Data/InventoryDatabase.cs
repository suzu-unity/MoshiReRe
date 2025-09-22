using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Databases/Inventory Database")]
public class InventoryDatabase : ScriptableObject
{
    public List<InventoryItem> items = new List<InventoryItem>();

    public IReadOnlyList<InventoryItem> GetAll() => items;
}
