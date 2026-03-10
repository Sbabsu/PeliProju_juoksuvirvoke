using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("World Drop")]
    public GameObject worldPrefab;

    [Header("Audio")]
    public AudioClip[] pickupSoundClip;
    [Range(0f, 1f)] public float pickupVolume = 1f;
}