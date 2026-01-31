using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PickUp : MonoBehaviour
{
    [Header("Pickup Settings")]
    public string playerTag = "Player";
    public int maxPickupsFromArea = 5;

    [Header("Visual Feedback")]
    public GameObject pickupEffect;
    public Material collectedMaterial; // Optional: Change material when depleted

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

        itemsCollectedFromArea++;

        // Add to player's total score
        GameManager.Instance?.AddScore();

        // Spawn effect if exists
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, transform.rotation);
        }

        // Check if area is depleted
        if (itemsCollectedFromArea >= maxPickupsFromArea)
        {
            DepleteArea();
        }
    }

    private void DepleteArea()
    {
        isAreaDepleted = true;

        // Visual feedback that area is empty
        if (collectedMaterial != null && areaRenderer != null)
        {
            areaRenderer.material = collectedMaterial;
        }

        // Optionally shrink or change appearance
        transform.localScale *= 0.8f;

        // Update UI

        Debug.Log($"Pickup area depleted. Total collected: {itemsCollectedFromArea}");
    }

    // For debugging/visualization
    private void OnDrawGizmos()
    {
        if (!isAreaDepleted)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
        else
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, 0.8f);
        }

        // Show collection count
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"{itemsCollectedFromArea}/{maxPickupsFromArea}");
#endif
    }
}
