using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Database")]
    public ItemDatabase database;

    [Header("UI - Inventory list (TAB)")]
    public GameObject inventoryPanel;
    public TMP_Text inventoryText;

    [Header("UI - Pickup feed (top right)")]
    public PickupFeedUI pickupFeed;

    private Dictionary<string, int> counts = new Dictionary<string, int>();

    void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        RefreshListUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryPanel != null)
                inventoryPanel.SetActive(!inventoryPanel.activeSelf);

            RefreshListUI();
        }
    }

    public void AddItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        if (amount <= 0) amount = 1;

        if (counts.ContainsKey(itemId))
            counts[itemId] += amount;
        else
            counts[itemId] = amount;

        RefreshListUI();

        if (pickupFeed != null)
        {
            string name = itemId;
            Sprite icon = null;

            if (database != null)
            {
                var def = database.GetById(itemId);
                if (def != null)
                {
                    name = def.displayName;
                    icon = def.icon;
                }
            }

            pickupFeed.Show(name, counts[itemId], icon);
        }
    }

    private void RefreshListUI()
    {
        if (inventoryText == null) return;

        var sb = new StringBuilder();
        sb.AppendLine("INVENTAARIO");

        if (counts.Count == 0)
        {
            sb.AppendLine("(ei viel mitää)");
        }
        else
        {
            foreach (var kvp in counts)
            {
                string name = kvp.Key;
                if (database != null)
                {
                    var def = database.GetById(kvp.Key);
                    if (def != null) name = def.displayName;
                }

                sb.AppendLine($"{name} x{kvp.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("TAB = avaa/sulje");
        inventoryText.text = sb.ToString();
    }

    // ---- KUITTIA VARTEN: vain kerätyt (displayName -> määrä) ----
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

            // jos sama displayName tulee kahdesta id:stä, kasaa yhteen
            if (result.ContainsKey(name)) result[name] += kvp.Value;
            else result[name] = kvp.Value;
        }

        return result;
    }
}
