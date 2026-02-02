using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject receiptOverlay; // raahaa ReceiptPanel (overlay)

    void Awake()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        else
            Debug.LogError("InventoryToggle: inventoryPanel puuttuu Inspectorista!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("TAB pressed");

            if (receiptOverlay != null && receiptOverlay.activeSelf)
                return;

            if (inventoryPanel == null) return;

            bool open = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(open);

            Cursor.visible = open;
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
