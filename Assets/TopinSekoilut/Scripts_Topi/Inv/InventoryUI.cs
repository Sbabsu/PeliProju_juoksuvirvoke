using UnityEngine;
using System.Text;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TMP_Text listText;

    [Header("Optional: pickup feed UI")]
    public PickupFeedUI pickupFeed;

    private InventoryService inv;

    void Awake()
    {
        inv = InventoryService.Instance;
        if (panel != null) panel.SetActive(false);
    }

    void OnEnable()
    {
        if (inv == null) inv = InventoryService.Instance;
        if (inv != null) inv.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (inv != null) inv.OnChanged -= Refresh;
    }

    public void Toggle()
    {
        if (panel == null) return;

        bool willOpen = !panel.activeSelf;
        panel.SetActive(willOpen);

        if (willOpen)
        {
            Refresh();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            // Time.timeScale = 0f; // optional
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            // Time.timeScale = 1f; // optional
        }
    }


    public void Refresh()
    {
        if (listText == null) return;
        if (inv == null) { listText.text = "InventoryService missing"; return; }

        var sb = new StringBuilder();
        sb.AppendLine("INVENTORY");

        var all = inv.GetAll();
        if (all.Count == 0)
        {
            sb.AppendLine("(nothing yet)");
        }
        else
        {
            foreach (var kvp in all)
            {
                var name = inv.GetDisplayName(kvp.Key);
                sb.AppendLine($"{name} x{kvp.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("TAB = open/close");
        listText.text = sb.ToString();
    }

    // Call this from pickup scripts if you want feed popups:
    public void ShowPickup(string itemId)
    {
        if (pickupFeed == null || inv == null) return;

        string name = inv.GetDisplayName(itemId);
        Sprite icon = inv.GetIcon(itemId);
        int count = inv.GetCount(itemId);

        pickupFeed.Show(name, count, icon);
    }
}
