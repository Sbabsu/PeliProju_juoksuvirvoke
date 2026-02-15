using UnityEngine;

public class GuardCatchPlayer : MonoBehaviour
{
    [SerializeField] private float cooldown = 2f;
    private float _cd;

    [Header("Refs")]
    [SerializeField] private InventoryService inventory;
    [SerializeField] private ReceiptUI_TMP receiptUI;

    private void Awake()
    {
        if (inventory == null) inventory = InventoryService.Instance;
    }

    private void Update()
    {
        if (_cd > 0f) _cd -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_cd > 0f) return;

        // Player check
        if (!other.CompareTag("Player") && other.GetComponentInParent<PlayerController>() == null)
            return;

        _cd = cooldown;

        // 1) Jos tyhjä inv -> häviö
        if (inventory == null || inventory.IsEmpty())
        {
            if (receiptUI != null) receiptUI.OpenFailReceipt();
            else Debug.LogWarning("ReceiptUI_TMP puuttuu GuardCatchPlayerista.");
            return;
        }

        // 2) Muuten pudota inventory, mutta älä lopeta peliä
        var dropper = other.GetComponentInParent<InventoryDropper>();
        if (dropper != null)
            dropper.BurstDropAndClear();
    }
}
