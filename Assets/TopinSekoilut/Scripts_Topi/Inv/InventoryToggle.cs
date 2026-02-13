using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public KeyCode key = KeyCode.Tab;
    public InventoryUI ui;

    void Awake()
    {
        if (ui == null) ui = Object.FindFirstObjectByType<InventoryUI>();
    }

    void Update()
    {
        if (Input.GetKeyDown(key) && ui != null)
            ui.Toggle();
    }
}
