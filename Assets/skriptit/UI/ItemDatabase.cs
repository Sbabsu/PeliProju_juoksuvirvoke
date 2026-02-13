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
}
