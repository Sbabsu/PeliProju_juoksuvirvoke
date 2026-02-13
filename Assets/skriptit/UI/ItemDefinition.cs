using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string id;          // esim "beer_can"
    public string displayName; // "Olut tölkki"
    public Sprite icon;        // UI-ikoni (voi olla tyhjä aluksi)
}
