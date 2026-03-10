using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryService : MonoBehaviour
{
    public static InventoryService Instance { get; private set; }

    [Header("Database (optional but recommended)")]
    public ItemDatabase database;

    // id -> count
    private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

    // Fired whenever inventory changes
    public event Action OnChanged;

    // Backward compatibility for older UI/scripts
    public event Action<string, int> OnItemAdded;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Optional:
        // DontDestroyOnLoad(gameObject);
    }

    public int GetCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return 0;
        return counts.TryGetValue(itemId, out var c) ? c : 0;
    }

    public IReadOnlyDictionary<string, int> GetAll() => counts;

    public void AddItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        if (amount <= 0) amount = 1;

        if (counts.ContainsKey(itemId))
            counts[itemId] += amount;
        else
            counts[itemId] = amount;

        OnChanged?.Invoke();
        OnItemAdded?.Invoke(itemId, amount);
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        if (amount <= 0) amount = 1;

        if (!counts.TryGetValue(itemId, out var current) || current <= 0)
            return false;

        current -= amount;

        if (current <= 0)
            counts.Remove(itemId);
        else
            counts[itemId] = current;

        OnChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        counts.Clear();
        OnChanged?.Invoke();
    }

    public Dictionary<string, int> GetReceiptItemsByName()
    {
        var result = new Dictionary<string, int>();

        foreach (var kvp in counts)
        {
            if (kvp.Value <= 0) continue;

            string name = kvp.Key;

            if (database != null)
            {
                var def = database.GetById(kvp.Key);
                if (def != null && !string.IsNullOrEmpty(def.displayName))
                    name = def.displayName;
            }

            if (result.ContainsKey(name))
                result[name] += kvp.Value;
            else
                result[name] = kvp.Value;
        }

        return result;
    }

    public string GetDisplayName(string itemId)
    {
        if (database == null) return itemId;

        var def = database.GetById(itemId);
        return (def != null && !string.IsNullOrEmpty(def.displayName)) ? def.displayName : itemId;
    }

    public Sprite GetIcon(string itemId)
    {
        if (database == null) return null;

        var def = database.GetById(itemId);
        return def != null ? def.icon : null;
    }

    public ItemDefinition GetDefinition(string itemId)
    {
        if (database == null) return null;
        return database.GetById(itemId);
    }

    public bool IsEmpty()
    {
        foreach (var kvp in counts)
        {
            if (kvp.Value > 0)
                return false;
        }

        return true;
    }

    public int GetCountByGroup(string group)
    {
        if (string.IsNullOrEmpty(group) || database == null) return 0;

        int total = 0;

        foreach (var kvp in counts)
        {
            if (kvp.Value <= 0) continue;

            string itemGroup = database.GetGroupById(kvp.Key);
            if (itemGroup == group)
                total += kvp.Value;
        }

        return total;
    }

    public Sprite GetGroupIcon(string group)
    {
        if (database == null) return null;
        return database.GetFirstIconByGroup(group);
    }

    public string GetGroupDisplayName(string group)
    {
        if (database == null) return group;
        return database.GetFirstDisplayNameByGroup(group);
    }
}