using UnityEngine;

public class GuardCatchPlayer : MonoBehaviour
{
    [SerializeField] private float cooldown = 2f;
    private float _cd;

    private void Update()
    {
        if (_cd > 0f) _cd -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_cd > 0f) return;

        // If your player root has tag "Player"
        if (!other.CompareTag("Player") && other.GetComponentInParent<PlayerController>() == null)
            return;

        _cd = cooldown;

        // Drop inventory from player
        var dropper = other.GetComponentInParent<InventoryDropper>();
        if (dropper != null)
            dropper.BurstDropAndClear();

        // TODO: show game over UI here

        // TODO: Respawn / second chance (you can implement however you want)
    }
}

