using UnityEngine;

public class PickUp : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int maxPickupsFromArea = 999999;
    public string itemId = "beer_can";
    public int amountPerPickup = 1;

    [Header("Visual Feedback")]
    public GameObject pickupEffect;
    public Material collectedMaterial;

    private int itemsCollectedFromArea = 0;
    private bool isAreaDepleted = false;
    private MeshRenderer areaRenderer;

    private InventoryService inv;
    private InventoryUI invUI;

    private void Start()
    {
        areaRenderer = GetComponent<MeshRenderer>();
        inv = InventoryService.Instance;
        invUI = Object.FindFirstObjectByType<InventoryUI>();
    }

    public void CollectItem()
    {
        if (isAreaDepleted) return;

        if (inv == null)
        {
            Debug.LogError("PickUp: InventoryService.Instance missing in scene.");
            return;
        }

        itemsCollectedFromArea++;
        inv.AddItem(itemId, amountPerPickup);

        // optional pickup feed
        if (invUI != null) invUI.ShowPickup(itemId);

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, transform.rotation);

        if (itemsCollectedFromArea >= maxPickupsFromArea)
            DepleteArea();
    }

    private void DepleteArea()
    {
        isAreaDepleted = true;

        if (collectedMaterial != null && areaRenderer != null)
            areaRenderer.material = collectedMaterial;

        transform.localScale *= 0.8f;
    }
}
