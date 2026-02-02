using UnityEngine;

public class PickUp : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int maxPickupsFromArea = 999999; // ei ylärajaa käytännössä
    public string itemId = "beer_can";
    public int amountPerPickup = 1;

    [Header("Visual Feedback")]
    public GameObject pickupEffect;
    public Material collectedMaterial;

    private int itemsCollectedFromArea = 0;
    private bool isAreaDepleted = false;
    private MeshRenderer areaRenderer;

    private void Start()
    {
        areaRenderer = GetComponent<MeshRenderer>();
    }

    public void CollectItem()
    {
        if (isAreaDepleted) return;

        // hae Inventory
        var inv = FindObjectOfType<Inventory>(true);
        if (inv == null)
        {
            Debug.LogError("PickUp: Inventoryä ei löytynyt scenestä/Playerista.");
            return;
        }

        itemsCollectedFromArea++;
        inv.AddItem(itemId, amountPerPickup);

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
