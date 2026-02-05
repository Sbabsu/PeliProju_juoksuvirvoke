using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    // This script is attached to pickable items in the scene.
    // It holds the data for the item and handles pickup logic.

    [Header("Inventory Data")]
    public string itemId = "beer_can";
    public int amount = 1;

    [Header("Behavior")]
    [Tooltip("If true, the object is removed from the scene after pickup.")]
    public bool destroyOnPickup = true;

    [HideInInspector] public bool pickedUp; // prevents double pickup by two hands
}
