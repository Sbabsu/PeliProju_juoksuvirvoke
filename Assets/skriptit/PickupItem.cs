using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    [Header("Pickup")]
    public string playerTag = "Player";
    public string itemId = "beer_can";
    public int amount = 1;

    [Header("FX")]
    public GameObject pickupEffect;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // hyväksy osuma myös ragdollin lapsiosiin
        bool hitPlayer =
            other.CompareTag(playerTag) ||
            (other.transform.root != null && other.transform.root.CompareTag(playerTag));

        if (!hitPlayer) return;

        // Inventory voi olla Playerissä tai scenessä
        Inventory inv = other.GetComponentInParent<Inventory>();
        if (inv == null && other.transform.root != null)
            inv = other.transform.root.GetComponentInChildren<Inventory>();

        if (inv == null)
            inv = FindObjectOfType<Inventory>(true);

        if (inv == null)
        {
            Debug.LogError("PickupItem: Inventoryä ei löytynyt. Lisää Inventory Playeriin tai sceneen.");
            return;
        }

        inv.AddItem(itemId, amount);

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
