using System.Collections.Generic;
using UnityEngine;

public class InventoryTracker : MonoBehaviour
{
    // itemName -> count
    private readonly Dictionary<string, int> items = new Dictionary<string, int>();

    public void Add(string itemName, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemName)) return;
        if (amount <= 0) return;

        if (items.ContainsKey(itemName)) items[itemName] += amount;
        else items[itemName] = amount;
    }

    public IReadOnlyDictionary<string, int> GetAll() => items;

    public void Clear() => items.Clear();
}
