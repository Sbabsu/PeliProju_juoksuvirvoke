using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PickUp : MonoBehaviour
{
    [Header("Pickup Settings")]
    public string playerTag = "Player";

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isCollected)
        {
            ShowPickupPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            HidePickupPrompt();
        }
    }

    private void ShowPickupPrompt()
    {
        // Optional: Show UI prompt like "Press E to pickup"
        GameManager.Instance?.ShowPickupPrompt(true);
    }

    private void HidePickupPrompt()
    {
        GameManager.Instance?.ShowPickupPrompt(false);
    }

    public void CollectItem()
    {
        if (isCollected) return;

        isCollected = true;

        // Add to player's score
        GameManager.Instance?.AddScore();

        // Hide/destroy the pickup object
        gameObject.SetActive(false);
        // Or: Destroy(gameObject);
    }
}
