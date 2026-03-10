using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Item Definition")]
public class ItemDefinition : ScriptableObject
{
<<<<<<< HEAD
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("World Drop")]
    public GameObject worldPrefab;

    [Header("Audio")]
    public AudioClip[] pickupSoundClip;
    [Range(0f, 1f)] public float pickupVolume = 1f;
=======
    public string id;          // esim "beer_can_heinäkenkä"
    public string displayName; // esim "ÖLPPÖNEN"
    public Sprite icon;        // UI-ikoni

    [Header("Grouping")]
    public string itemGroup;   // esim "beer_can"

    [Header("World Drop")]
    public GameObject worldPrefab;
>>>>>>> 3939870 (stamina + kaljacounter sekä inventoryyn nimet kaikille eikä vaan ölppönen nimeä montaa kertaa)
}