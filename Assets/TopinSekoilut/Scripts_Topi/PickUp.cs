using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    [System.Serializable]
    public class ItemOption
    {
        public string itemId = "beer_can";

        [Min(1)] public int amountMin = 1;
        [Min(1)] public int amountMax = 1;

        [Tooltip("Higher = more likely.")]
        [Min(0f)] public float weight = 1f;
    }

    [Header("Pickup Settings")]
    public int maxPickupsFromArea = 10;

    [Tooltip("Possible items this area can give.")]
    public List<ItemOption> possibleItems = new List<ItemOption>()
    {
        new ItemOption(){ itemId="beer_glass", amountMin=1, amountMax=1, weight=1f },
        new ItemOption(){ itemId="beer_bottle_nalle", amountMin=1, amountMax=1, weight=1f }
    };

    [Header("Selection Mode")]
    [Tooltip("If true, chooses random item each pickup. If false, cycles through list.")]
    public bool randomEachPickup = true;

    [Header("Visual Feedback")]
    public GameObject pickupEffect;
    public Material collectedMaterial;

    private int itemsCollectedFromArea = 0;
    private bool isAreaDepleted = false;
    private MeshRenderer areaRenderer;

    private InventoryService inv;
    private InventoryUI invUI;

    private int cycleIndex = 0;

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

        if (possibleItems == null || possibleItems.Count == 0)
        {
            Debug.LogWarning($"PickUp '{name}' has no possibleItems set.");
            return;
        }

        ItemOption chosen = randomEachPickup ? ChooseWeighted(possibleItems) : ChooseCycled(possibleItems);

        if (string.IsNullOrEmpty(chosen.itemId))
        {
            Debug.LogWarning($"PickUp '{name}' chosen itemId is empty.");
            return;
        }

        int amount = Random.Range(chosen.amountMin, chosen.amountMax + 1);

        itemsCollectedFromArea++;
        inv.AddItem(chosen.itemId, amount);

        if (invUI != null) invUI.ShowPickup(chosen.itemId);

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, transform.rotation);

        if (itemsCollectedFromArea >= maxPickupsFromArea)
            DepleteArea();
    }

    private ItemOption ChooseCycled(List<ItemOption> list)
    {
        if (cycleIndex >= list.Count) cycleIndex = 0;
        return list[cycleIndex++];
    }

    private ItemOption ChooseWeighted(List<ItemOption> list)
    {
        float total = 0f;
        for (int i = 0; i < list.Count; i++)
            total += Mathf.Max(0f, list[i].weight);

        // If all weights are 0, just pick uniformly
        if (total <= 0.0001f)
            return list[Random.Range(0, list.Count)];

        float r = Random.value * total;
        float running = 0f;

        for (int i = 0; i < list.Count; i++)
        {
            running += Mathf.Max(0f, list[i].weight);
            if (r <= running)
                return list[i];
        }

        return list[list.Count - 1];
    }

    private void DepleteArea()
    {
        isAreaDepleted = true;

        if (collectedMaterial != null && areaRenderer != null)
            areaRenderer.material = collectedMaterial;

        transform.localScale *= 0.8f;
    }
}
