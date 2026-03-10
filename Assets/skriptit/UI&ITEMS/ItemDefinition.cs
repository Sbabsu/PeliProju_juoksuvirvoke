using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string id;          // esim "beer_can_heinäkenkä"
    public string displayName; // esim "Heinäkenkä"
    public Sprite icon;        // UI-ikoni

    [Header("Grouping")]
    public string itemGroup;   // esim "beer_can"

    [Header("Audio")]
    public AudioClip[] pickupSoundClips;
    [Range(0f, 1f)] public float pickupVolume = 1f;

    [Header("World Drop")]
    public GameObject worldPrefab;
}