using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryService : MonoBehaviour
{
    public static InventoryService Instance { get; private set; }

    [Header("Item Database")]
    public ItemDatabase database;

    private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

    public event Action OnChanged;
    public event Action<string, int> OnItemAdded;

    void Awake()
    {
        Debug.Log($"InventoryService Awake on {name} instanceID={GetInstanceID()}");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"InventoryService: Duplicate instance in scene {SceneManager.GetActiveScene().name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
<<<<<<< HEAD

        if (database == null)
            Debug.LogError($"InventoryService: ItemDatabase is not assigned in scene {SceneManager.GetActiveScene().name}");
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
=======
>>>>>>> 3939870 (stamina + kaljacounter sekä inventoryyn nimet kaikille eikä vaan ölppönen nimeä montaa kertaa)
    }

    public int GetCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return 0;
        return counts.TryGetValue(itemId, out var c) ? c : 0;
    }

    public IReadOnlyDictionary<string, int> GetAll() => counts;

    public void AddItem(string itemId, int amount = 1)
    {
        Debug.Log($"AddItem called: {itemId} x{amount}");

        if (string.IsNullOrEmpty(itemId)) return;
        if (amount <= 0) amount = 1;

        if (counts.ContainsKey(itemId))
            counts[itemId] += amount;
        else
            counts[itemId] = amount;

        Debug.Log($"Before OnItemAdded invoke: {itemId}");
        OnItemAdded?.Invoke(itemId, amount);
        Debug.Log($"After OnItemAdded invoke: {itemId}");

        OnChanged?.Invoke();
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

    public bool IsEmpty()
    {
        foreach (var kvp in counts)
        {
            if (kvp.Value > 0)
                return false;
        }
        return true;
    }

<<<<<<< HEAD
    public ItemDefinition GetDefinition(string itemId)
    {
        if (database == null) return null;
        return database.GetById(itemId);
=======
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
>>>>>>> 3939870 (stamina + kaljacounter sekä inventoryyn nimet kaikille eikä vaan ölppönen nimeä montaa kertaa)
    }
}