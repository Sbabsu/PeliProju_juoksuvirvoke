using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemDefinition> items = new List<ItemDefinition>();

    private Dictionary<string, ItemDefinition> map;

    public ItemDefinition GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (map == null)
        {
            map = new Dictionary<string, ItemDefinition>();
            foreach (var it in items)
            {
                if (it != null && !string.IsNullOrEmpty(it.id) && !map.ContainsKey(it.id))
                    map.Add(it.id, it);
            }
        }

        map.TryGetValue(id, out var def);
        return def;
    }

    public string GetGroupById(string id)
    {
        var def = GetById(id);
        if (def == null) return null;
        return def.itemGroup;
    }

    public Sprite GetFirstIconByGroup(string group)
    {
        if (string.IsNullOrEmpty(group)) return null;

        foreach (var it in items)
        {
            if (it == null) continue;
            if (it.itemGroup == group && it.icon != null)
                return it.icon;
        }

        return null;
    }

    public string GetFirstDisplayNameByGroup(string group)
    {
        if (string.IsNullOrEmpty(group)) return group;

        foreach (var it in items)
        {
            if (it == null) continue;
            if (it.itemGroup == group && !string.IsNullOrEmpty(it.displayName))
                return it.displayName;
        }

        return group;
    }
}