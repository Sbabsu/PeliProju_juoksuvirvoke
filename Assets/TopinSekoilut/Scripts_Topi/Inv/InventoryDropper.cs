using System.Collections.Generic;
using UnityEngine;

public class InventoryDropper : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private InventoryService inventory;
    [SerializeField] private ItemDatabase database;

    [Header("Drop Settings")]
    [SerializeField] private Transform dropOrigin;   // hips/chest transform
    [SerializeField] private float spawnRadius = 0.35f;

    [SerializeField] private float upwardVel = 4f;
    [SerializeField] private float outwardVel = 3f;
    [SerializeField] private float spinVel = 4f;

    [Header("Safety")]
    [SerializeField] private int maxSpawnPerItemType = 50;

    private void Awake()
    {
        if (inventory == null) inventory = InventoryService.Instance;
        if (dropOrigin == null) dropOrigin = transform;
    }

    public void BurstDropAndClear()
    {
        if (inventory == null || database == null) return;

        // snapshot counts so we can clear after
        IReadOnlyDictionary<string, int> all = inventory.GetAll();
        var snapshot = new Dictionary<string, int>(all);

        foreach (var kvp in snapshot)
        {
            string itemId = kvp.Key;
            int count = Mathf.Max(0, kvp.Value);
            if (count == 0) continue;

            var def = database.GetById(itemId);
            if (def == null || def.worldPrefab == null)
            {
                Debug.LogWarning($"No ItemDefinition/worldPrefab for '{itemId}'.");
                continue;
            }

            int spawnCount = Mathf.Min(count, maxSpawnPerItemType);

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 offset = Random.insideUnitSphere * spawnRadius;
                offset.y = Mathf.Abs(offset.y) * 0.2f; // keep near ground-ish

                Vector3 pos = dropOrigin.position + offset;
                Quaternion rot = Random.rotation;

                GameObject go = Instantiate(def.worldPrefab, pos, rot);

                // Allow picking up again
                var pick = go.GetComponent<PickUpItem>();
                if (pick != null)
                {
                    pick.itemId = itemId;
                    pick.amount = 1;
                    pick.pickedUp = false;
                }

                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 outward = offset.sqrMagnitude > 0.001f ? offset.normalized : Random.onUnitSphere;
                    outward.y = 0f;
                    outward.Normalize();

                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    rb.AddForce(Vector3.up * upwardVel, ForceMode.VelocityChange);
                    rb.AddForce(outward * outwardVel, ForceMode.VelocityChange);
                    rb.AddTorque(Random.insideUnitSphere * spinVel, ForceMode.VelocityChange);
                }
            }
        }

        inventory.Clear();
    }
}
